using backend.DTOs;
using backend.Services;
using backend.Models;
using backend.Data;
using backend.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaddleService _paddleService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;
        private readonly SubscriptionSettings _subscriptionSettings;

        public PaymentController(
            IPaddleService paddleService,
            ApplicationDbContext context,
            ILogger<PaymentController> logger,
            IConfiguration configuration,
            IOptions<SubscriptionSettings> subscriptionSettings)
        {
            _paddleService = paddleService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _subscriptionSettings = subscriptionSettings.Value;
        }

        /// <summary>
        /// Paddle checkout session oluştur
        /// </summary>
        [HttpPost("create-checkout")]
        [ProducesResponseType(typeof(CreateCheckoutResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CreateCheckoutResponseDto>> CreateCheckout([FromBody] CreateCheckoutRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating checkout for plan {PlanId}, email {Email}, clinic {ClinicId}",
                    request.PlanId, request.CustomerEmail, request.ClinicId);

                // ✅ SECURITY: Verify clinic has completed registration
                if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
                {
                    var clinic = await _context.Clinics.FindAsync(request.ClinicId.Value);
                    if (clinic == null)
                    {
                        return NotFound(new CreateCheckoutResponseDto
                        {
                            Success = false,
                            Message = "Clinic not found"
                        });
                    }

                    if (!clinic.RegistrationCompleted)
                    {
                        return BadRequest(new CreateCheckoutResponseDto
                        {
                            Success = false,
                            Message = "Registration process not completed"
                        });
                    }
                }

                // ✅ SECURITY: Check for existing active payment processes (idempotency)
                if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
                {
                    var existingActivePayment = await _context.Subscriptions
                        .Where(s => s.ClinicId == request.ClinicId.Value
                                    && s.PlanId == request.PlanId
                                    && s.HasActivePaymentProcess)
                        .FirstOrDefaultAsync();

                    if (existingActivePayment != null)
                    {
                        _logger.LogWarning("Duplicate payment attempt detected for clinic {ClinicId}, plan {PlanId}. Existing transaction: {TransactionId}",
                            request.ClinicId, request.PlanId, existingActivePayment.PaddleTransactionId);

                        return Conflict(new CreateCheckoutResponseDto
                        {
                            Success = false,
                            Message = "A payment is already in progress for this plan",
                            TransactionId = existingActivePayment.PaddleTransactionId
                        });
                    }

                    // ✅ SECURITY: Check for existing completed/active subscription for same plan
                    var existingSubscription = await _context.Subscriptions
                        .Where(s => s.ClinicId == request.ClinicId.Value
                                    && s.PlanId == request.PlanId
                                    && (s.Status == (int)SubscriptionStatus.Active || s.Status == (int)SubscriptionStatus.PendingPayment)
                                    && s.EndDate > DateTime.UtcNow)
                        .FirstOrDefaultAsync();

                    if (existingSubscription != null && existingSubscription.Status == (int)SubscriptionStatus.Active)
                    {
                        _logger.LogWarning("Duplicate subscription detected for clinic {ClinicId}, plan {PlanId}",
                            request.ClinicId, request.PlanId);

                        return Conflict(new CreateCheckoutResponseDto
                        {
                            Success = false,
                            Message = "You already have an active subscription for this plan"
                        });
                    }
                }

                // Plan məlumatlarını al
                var plan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive);

                if (plan == null)
                {
                    return NotFound(new CreateCheckoutResponseDto
                    {
                        Success = false,
                        Message = "Plan not found or inactive"
                    });
                }

                // Paddle Price ID yoxla
                if (string.IsNullOrEmpty(plan.PaddlePriceId))
                {
                    return BadRequest(new CreateCheckoutResponseDto
                    {
                        Success = false,
                        Message = "Plan does not have a Paddle Price ID configured"
                    });
                }

                // Frontend URL-lərini al
                var frontendUrl = (_configuration["Frontend:Url"] ?? "http://localhost:4200").TrimEnd('/');
                var successUrl = request.SuccessUrl ?? $"{frontendUrl}/payment-success";
                var cancelUrl = request.CancelUrl ?? $"{frontendUrl}/payment-cancel";

                // Paddle checkout session oluştur
                var checkoutRequest = new PaddleCheckoutRequest
                {
                    ClinicId = request.ClinicId ?? Guid.Empty,
                    PlanId = request.PlanId,
                    UserId = request.UserId ?? Guid.Empty,
                    PaddlePriceId = plan.PaddlePriceId,
                    CustomerEmail = request.CustomerEmail,
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl
                };

                var result = await _paddleService.CreateCheckoutSessionAsync(checkoutRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Checkout session created: {TransactionId}", result.TransactionId);

                    // ✅ Pending subscription kaydı oluştur with HasActivePaymentProcess flag
                    if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
                    {
                        var subscription = new Subscription
                        {
                            ClinicId = request.ClinicId.Value,
                            PlanId = request.PlanId,
                            Status = (int)SubscriptionStatus.PendingPayment,
                            PaddleTransactionId = result.TransactionId,
                            HasActivePaymentProcess = true, // ✅ Mark as active payment process
                            Currency = plan.Currency,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddMonths(1), // Varsayılan olarak 1 ay
                            PaymentMethod = "Paddle"
                        };

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

                        _context.Subscriptions.Add(subscription);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Pending subscription created for clinic {ClinicId} with active payment process flag", request.ClinicId);
                    }
                }

                return Ok(new CreateCheckoutResponseDto
                {
                    Success = result.Success,
                    CheckoutUrl = result.CheckoutUrl,
                    TransactionId = result.TransactionId,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout");
                return StatusCode(500, new CreateCheckoutResponseDto
                {
                    Success = false,
                    Message = "An error occurred while creating checkout"
                });
            }
        }

        /// <summary>
        /// Ödeme başarılı sayfası için bilgileri al
        /// </summary>
        [HttpGet("success/{transactionId}")]
        [ProducesResponseType(typeof(PaymentSuccessDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentSuccessDto>> GetPaymentSuccess(string transactionId)
        {
            try
            {
                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.PaddleTransactionId == transactionId);

                if (subscription == null)
                {
                    return NotFound(new { message = "Payment not found" });
                }

                return Ok(new PaymentSuccessDto
                {
                    TransactionId = transactionId,
                    ClinicId = subscription.ClinicId,
                    PlanId = subscription.PlanId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment success info");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// ✅ NEW: Validate checkout state (for frontend state validation)
        /// </summary>
        [HttpPost("validate-state")]
        [ProducesResponseType(typeof(CheckoutStateValidationDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CheckoutStateValidationDto>> ValidateState([FromBody] ValidateCheckoutStateRequestDto request)
        {
            try
            {
                var response = new CheckoutStateValidationDto
                {
                    IsValid = true,
                    RegistrationCompleted = false,
                    HasActivePaymentProcess = false,
                    HasActiveSubscription = false
                };

                // Check clinic registration status
                if (request.ClinicId.HasValue && request.ClinicId.Value != Guid.Empty)
                {
                    var clinic = await _context.Clinics.FindAsync(request.ClinicId.Value);
                    if (clinic != null)
                    {
                        response.RegistrationCompleted = clinic.RegistrationCompleted;
                    }
                    else
                    {
                        response.IsValid = false;
                        response.Message = "Clinic not found";
                        return Ok(response);
                    }
                }

                // Check for active payment process
                if (request.ClinicId.HasValue && request.PlanId.HasValue)
                {
                    var activePayment = await _context.Subscriptions
                        .Where(s => s.ClinicId == request.ClinicId.Value
                                    && s.PlanId == request.PlanId.Value
                                    && s.HasActivePaymentProcess)
                        .FirstOrDefaultAsync();

                    response.HasActivePaymentProcess = activePayment != null;
                    response.ActiveTransactionId = activePayment?.PaddleTransactionId;
                }

                // Check for active subscription
                if (request.ClinicId.HasValue)
                {
                    var activeSubscription = await _context.Subscriptions
                        .Where(s => s.ClinicId == request.ClinicId.Value
                                    && s.Status == (int)SubscriptionStatus.Active
                                    && s.EndDate > DateTime.UtcNow)
                        .FirstOrDefaultAsync();

                    response.HasActiveSubscription = activeSubscription != null;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating checkout state");
                return StatusCode(500, new CheckoutStateValidationDto
                {
                    IsValid = false,
                    Message = "An error occurred while validating state"
                });
            }
        }
    }
}
