using backend.Services;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public PaddleWebhookController(
            IPaddleService paddleService,
            ApplicationDbContext context,
            ILogger<PaddleWebhookController> logger)
        {
            _paddleService = paddleService;
            _context = context;
            _logger = logger;
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

            subscription.Status = SubscriptionStatus.Active;
            subscription.UpdatedAt = DateTime.UtcNow;

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

            subscription.Status = SubscriptionStatus.Active;
            subscription.TransactionId = transactionId;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Paddle subscription ID varsa kaydet
            if (webhookData.Data.SubscriptionId != null)
            {
                subscription.PaddleSubscriptionId = webhookData.Data.SubscriptionId;
            }

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

            subscription.Status = SubscriptionStatus.PaymentFailed;
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
                subscription.Status = SubscriptionStatus.Cancelled;
                subscription.CancelledAt = DateTime.UtcNow;
                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription canceled for clinic {ClinicId}", subscription.ClinicId);
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
