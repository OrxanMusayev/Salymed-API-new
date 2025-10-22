using backend.Services;
using backend.Data;
using backend.Models;
using backend.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaddleWebhookController : ControllerBase
    {
        private readonly IPaddleService _paddleService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaddleWebhookController> _logger;
        private readonly SubscriptionSettings _subscriptionSettings;

        public PaddleWebhookController(
            IPaddleService paddleService,
            ApplicationDbContext context,
            ILogger<PaddleWebhookController> logger,
            IOptions<SubscriptionSettings> subscriptionSettings)
        {
            _paddleService = paddleService;
            _context = context;
            _logger = logger;
            _subscriptionSettings = subscriptionSettings.Value;
        }

        /// <summary>
        /// Test endpoint - Webhook'u manuel test etmek için
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestWebhook([FromBody] PaddleWebhookEvent testData)
        {
            _logger.LogInformation("Testing webhook with event type: {EventType}", testData.EventType);

            try
            {
                switch (testData.EventType)
                {
                    case "transaction.completed":
                        await HandleTransactionCompleted(testData);
                        break;

                    case "transaction.paid":
                        await HandleTransactionPaid(testData);
                        break;

                    default:
                        _logger.LogWarning("Unhandled test event type: {EventType}", testData.EventType);
                        break;
                }

                return Ok(new { message = "Test webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing test webhook");
                return StatusCode(500, new { message = "Error processing test webhook", error = ex.Message });
            }
        }

        /// <summary>
        /// Paddle webhook endpoint - Ödeme durumu güncellemeleri için
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            try
            {
                // Request body-ni oxu
                using var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                _logger.LogInformation("Received Paddle webhook: {Body}", requestBody);

                // Signature verification - TEMPORARILY DISABLED FOR TESTING
                var signature = Request.Headers["Paddle-Signature"].ToString();
                _logger.LogInformation("Webhook signature: {Signature}", signature);

                // TODO: Re-enable signature verification in production
                // var isValid = await _paddleService.VerifyWebhookSignatureAsync(signature, requestBody);
                // if (!isValid)
                // {
                //     _logger.LogWarning("Invalid webhook signature");
                //     return Unauthorized(new { message = "Invalid signature" });
                // }

                // Webhook data-nı parse et
                var webhookData = JsonSerializer.Deserialize<PaddleWebhookEvent>(requestBody);

                if (webhookData == null || webhookData.EventType == null)
                {
                    _logger.LogWarning("Invalid webhook data");
                    return BadRequest(new { message = "Invalid webhook data" });
                }

                _logger.LogInformation("Processing webhook event: {EventType}", webhookData.EventType);

                // Event type-a görə işlə
                switch (webhookData.EventType)
                {
                    case "transaction.completed":
                        await HandleTransactionCompleted(webhookData);
                        break;

                    case "transaction.paid":
                        await HandleTransactionPaid(webhookData);
                        break;

                    case "transaction.payment_failed":
                        await HandleTransactionFailed(webhookData);
                        break;

                    case "subscription.created":
                        await HandleSubscriptionCreated(webhookData);
                        break;

                    case "subscription.updated":
                        await HandleSubscriptionUpdated(webhookData);
                        break;

                    case "subscription.canceled":
                        await HandleSubscriptionCanceled(webhookData);
                        break;

                    default:
                        _logger.LogInformation("Unhandled event type: {EventType}", webhookData.EventType);
                        break;
                }

                return Ok(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500, new { message = "Error processing webhook" });
            }
        }

        private async Task HandleTransactionCompleted(PaddleWebhookEvent webhookData)
        {
            if (webhookData.Data?.Id == null)
            {
                _logger.LogWarning("Transaction completed event missing transaction ID");
                return;
            }

            var transactionId = webhookData.Data.Id;
            _logger.LogInformation("Handling transaction completed: {TransactionId}", transactionId);

            // Subscription-ı tap və statusunu güncəllə
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.PaddleTransactionId == transactionId);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for transaction: {TransactionId}", transactionId);
                return;
            }

            subscription.Status = (int)SubscriptionStatus.Active;
            subscription.HasActivePaymentProcess = false; // ✅ Clear payment process flag
            subscription.UpdatedAt = DateTime.UtcNow;

            // Set trial period from configuration if enabled
            if (_subscriptionSettings.HasTrialPeriod)
            {
                subscription.IsTrialPeriod = true;
                subscription.TrialEndDate = DateTime.UtcNow.AddMonths(_subscriptionSettings.TrialPeriodMonths);

                // Extend subscription end date with trial period
                subscription.EndDate = subscription.EndDate.AddMonths(_subscriptionSettings.TrialPeriodMonths);
                subscription.NextBillingDate = subscription.TrialEndDate;

                _logger.LogInformation("Trial period set for {Months} months, ending at {TrialEndDate}",
                    _subscriptionSettings.TrialPeriodMonths, subscription.TrialEndDate);
            }

            // Save payment method if available
            await SavePaymentMethodFromWebhook(webhookData, subscription.ClinicId, subscription.PlanId);

            // Create invoice for this payment
            await CreateInvoiceFromWebhook(webhookData, subscription);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription activated for clinic {ClinicId}", subscription.ClinicId);
        }

        private async Task HandleTransactionPaid(PaddleWebhookEvent webhookData)
        {
            if (webhookData.Data?.Id == null)
            {
                _logger.LogWarning("Transaction paid event missing transaction ID");
                return;
            }

            var transactionId = webhookData.Data.Id;
            _logger.LogInformation("Handling transaction paid: {TransactionId}", transactionId);

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.PaddleTransactionId == transactionId);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for transaction: {TransactionId}", transactionId);
                return;
            }

            subscription.Status = (int)SubscriptionStatus.Active;
            subscription.HasActivePaymentProcess = false; // ✅ Clear payment process flag
            subscription.TransactionId = transactionId;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Paddle subscription ID varsa kaydet
            if (webhookData.Data.SubscriptionId != null)
            {
                subscription.PaddleSubscriptionId = webhookData.Data.SubscriptionId;
            }

            // Set trial period from configuration if enabled
            if (_subscriptionSettings.HasTrialPeriod)
            {
                subscription.IsTrialPeriod = true;
                subscription.TrialEndDate = DateTime.UtcNow.AddMonths(_subscriptionSettings.TrialPeriodMonths);

                // Extend subscription end date with trial period
                subscription.EndDate = subscription.EndDate.AddMonths(_subscriptionSettings.TrialPeriodMonths);
                subscription.NextBillingDate = subscription.TrialEndDate;

                _logger.LogInformation("Trial period set for {Months} months, ending at {TrialEndDate}",
                    _subscriptionSettings.TrialPeriodMonths, subscription.TrialEndDate);
            }

            // Save payment method if available
            await SavePaymentMethodFromWebhook(webhookData, subscription.ClinicId, subscription.PlanId);

            // Create invoice for this payment
            await CreateInvoiceFromWebhook(webhookData, subscription);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment confirmed for clinic {ClinicId}", subscription.ClinicId);
        }

        private async Task HandleTransactionFailed(PaddleWebhookEvent webhookData)
        {
            if (webhookData.Data?.Id == null)
            {
                _logger.LogWarning("Transaction failed event missing transaction ID");
                return;
            }

            var transactionId = webhookData.Data.Id;
            _logger.LogInformation("Handling transaction failed: {TransactionId}", transactionId);

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.PaddleTransactionId == transactionId);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for transaction: {TransactionId}", transactionId);
                return;
            }

            subscription.Status = (int)SubscriptionStatus.PaymentFailed;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment failed for clinic {ClinicId}", subscription.ClinicId);
        }

        private async Task HandleSubscriptionCreated(PaddleWebhookEvent webhookData)
        {
            if (webhookData.Data?.SubscriptionId == null)
            {
                _logger.LogWarning("Subscription created event missing subscription ID");
                return;
            }

            _logger.LogInformation("Handling subscription created: {SubscriptionId}", webhookData.Data.SubscriptionId);

            // Paddle subscription ID-ni update et
            if (webhookData.Data.CustomData?.ClinicId != null)
            {
                var subscription = await _context.Subscriptions
                    .Where(s => s.ClinicId == webhookData.Data.CustomData.ClinicId)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (subscription != null)
                {
                    subscription.PaddleSubscriptionId = webhookData.Data.SubscriptionId;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task HandleSubscriptionUpdated(PaddleWebhookEvent webhookData)
        {
            if (webhookData.Data?.SubscriptionId == null)
            {
                _logger.LogWarning("Subscription updated event missing subscription ID");
                return;
            }

            _logger.LogInformation("Handling subscription updated: {SubscriptionId}", webhookData.Data.SubscriptionId);

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.PaddleSubscriptionId == webhookData.Data.SubscriptionId);

            if (subscription != null)
            {
                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task SavePaymentMethodFromWebhook(PaddleWebhookEvent webhookData, Guid clinicId, int planId)
        {
            try
            {
                if (webhookData.Data?.Payments == null || !webhookData.Data.Payments.Any())
                {
                    _logger.LogInformation("No payment information in webhook data");
                    return;
                }

                var paymentInfo = webhookData.Data.Payments.FirstOrDefault();
                var cardDetails = paymentInfo?.MethodDetails?.Card;

                if (cardDetails == null)
                {
                    _logger.LogInformation("No card details in payment information");
                    return;
                }

                // Check if payment method already exists
                var existingPaymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.ClinicId == clinicId
                        && pm.CardLast4 == cardDetails.Last4
                        && pm.CardExpiryMonth == cardDetails.ExpiryMonth
                        && pm.CardExpiryYear == cardDetails.ExpiryYear);

                if (existingPaymentMethod != null)
                {
                    _logger.LogInformation("Payment method already exists for clinic {ClinicId}", clinicId);
                    return;
                }

                // Get userId from custom data if available
                Guid? userId = webhookData.Data.CustomData?.UserId;

                // Set existing payment methods as non-default
                var existingMethods = await _context.PaymentMethods
                    .Where(pm => pm.ClinicId == clinicId && pm.IsDefault)
                    .ToListAsync();

                foreach (var method in existingMethods)
                {
                    method.IsDefault = false;
                    method.UpdatedAt = DateTime.UtcNow;
                }

                // Create new payment method
                var paymentMethod = new PaymentMethod
                {
                    ClinicId = clinicId,
                    UserId = userId,
                    PaddlePaymentMethodId = webhookData.Data.PaymentMethodId,
                    CardType = cardDetails.Type,
                    CardLast4 = cardDetails.Last4,
                    CardExpiryMonth = cardDetails.ExpiryMonth,
                    CardExpiryYear = cardDetails.ExpiryYear,
                    CardholderName = cardDetails.CardholderName,
                    Type = paymentInfo?.MethodDetails?.Type ?? "card",
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PaymentMethods.Add(paymentMethod);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment method saved for clinic {ClinicId}", clinicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payment method for clinic {ClinicId}", clinicId);
            }
        }

        private async Task HandleSubscriptionCanceled(PaddleWebhookEvent webhookData)
        {
            if (webhookData.Data?.SubscriptionId == null)
            {
                _logger.LogWarning("Subscription canceled event missing subscription ID");
                return;
            }

            _logger.LogInformation("Handling subscription canceled: {SubscriptionId}", webhookData.Data.SubscriptionId);

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.PaddleSubscriptionId == webhookData.Data.SubscriptionId);

            if (subscription != null)
            {
                subscription.Status = (int)SubscriptionStatus.Cancelled;
                subscription.CancelledAt = DateTime.UtcNow;
                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription canceled for clinic {ClinicId}", subscription.ClinicId);
            }
        }

        private async Task CreateInvoiceFromWebhook(PaddleWebhookEvent webhookData, Subscription subscription)
        {
            try
            {
                // Get plan details
                var plan = await _context.SubscriptionPlans.FindAsync(subscription.PlanId);
                if (plan == null)
                {
                    _logger.LogWarning("Plan not found for subscription {SubscriptionId}", subscription.Id);
                    return;
                }

                // Check if invoice already exists
                var transactionId = webhookData.Data?.Id;
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.PaddleTransactionId == transactionId);

                if (existingInvoice != null)
                {
                    _logger.LogInformation("Invoice already exists for transaction {TransactionId}", webhookData.Data?.Id);
                    return;
                }

                // Determine if this is a trial period payment
                bool isTrialPeriod = subscription.IsTrialPeriod && subscription.TrialEndDate > DateTime.UtcNow;

                // Calculate amount: $0 for trial, plan price otherwise
                decimal amount = isTrialPeriod ? 0 : plan.Price;
                decimal originalPrice = plan.Price;

                // Extract payment details from webhook
                var paymentInfo = webhookData.Data?.Payments?.FirstOrDefault();
                decimal taxAmount = 0; // Paddle webhook-dan gələcək
                decimal discountAmount = 0; // Paddle webhook-dan gələcək

                // Create invoice
                var invoice = new Invoice
                {
                    InvoiceNumber = webhookData.Data?.Id ?? Guid.NewGuid().ToString(),
                    ClinicId = subscription.ClinicId,
                    SubscriptionId = subscription.Id,
                    PlanId = subscription.PlanId,
                    Status = "Paid",
                    Amount = amount,
                    Currency = subscription.Currency,
                    IsTrialPeriod = isTrialPeriod,
                    OriginalPrice = originalPrice,
                    BillingPeriodStart = subscription.StartDate,
                    BillingPeriodEnd = subscription.EndDate,
                    PaidAt = DateTime.UtcNow,
                    DueDate = subscription.StartDate,
                    PaymentMethod = subscription.PaymentMethod,
                    PaddleTransactionId = webhookData.Data?.Id,
                    PaddleSubscriptionId = webhookData.Data?.SubscriptionId,
                    TaxAmount = taxAmount,
                    DiscountAmount = discountAmount,
                    Subtotal = originalPrice,
                    InvoiceDetailsJson = System.Text.Json.JsonSerializer.Serialize(webhookData),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Invoices.Add(invoice);

                _logger.LogInformation("Invoice created for clinic {ClinicId}, amount: {Amount}, isTrialPeriod: {IsTrialPeriod}",
                    subscription.ClinicId, amount, isTrialPeriod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for subscription {SubscriptionId}", subscription.Id);
            }
        }
    }

    // Webhook event models
    public class PaddleWebhookEvent
    {
        [System.Text.Json.Serialization.JsonPropertyName("event_type")]
        public string? EventType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public PaddleWebhookData? Data { get; set; }
    }

    public class PaddleWebhookData
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string? Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("subscription_id")]
        public string? SubscriptionId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("custom_data")]
        public PaddleCustomData? CustomData { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("payment_method_id")]
        public string? PaymentMethodId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("payments")]
        public List<PaddlePaymentInfo>? Payments { get; set; }
    }

    public class PaddlePaymentInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("method_details")]
        public PaddlePaymentMethodDetails? MethodDetails { get; set; }
    }

    public class PaddlePaymentMethodDetails
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string? Type { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("card")]
        public PaddleCardDetails? Card { get; set; }
    }

    public class PaddleCardDetails
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string? Type { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("last4")]
        public string? Last4 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expiry_month")]
        public int? ExpiryMonth { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expiry_year")]
        public int? ExpiryYear { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("cardholder_name")]
        public string? CardholderName { get; set; }
    }

    public class PaddleCustomData
    {
        [System.Text.Json.Serialization.JsonPropertyName("clinic_id")]
        public Guid? ClinicId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("plan_id")]
        public int? PlanId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("user_id")]
        public Guid? UserId { get; set; }
    }
}
