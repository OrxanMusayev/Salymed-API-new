using backend.DTOs;
using backend.Services;
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public PaymentController(
            IPaddleService paddleService,
            ApplicationDbContext context,
            ILogger<PaymentController> logger,
            IConfiguration configuration)
        {
            _paddleService = paddleService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Paddle checkout session oluştur
        /// </summary>
        [HttpPost("create-checkout")]
        [ProducesResponseType(typeof(CreateCheckoutResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CreateCheckoutResponseDto>> CreateCheckout([FromBody] CreateCheckoutRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating checkout for plan {PlanId}, email {Email}",
                    request.PlanId, request.CustomerEmail);

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
                var successUrl = $"{frontendUrl}/payment-success";
                var cancelUrl = $"{frontendUrl}/payment-cancel";

                // Paddle checkout session oluştur
                var checkoutRequest = new PaddleCheckoutRequest
                {
                    ClinicId = request.ClinicId ?? 0,
                    PlanId = request.PlanId,
                    UserId = request.UserId ?? 0,
                    PaddlePriceId = plan.PaddlePriceId,
                    CustomerEmail = request.CustomerEmail,
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl
                };

                var result = await _paddleService.CreateCheckoutSessionAsync(checkoutRequest);

                if (result.Success)
                {
                    _logger.LogInformation("Checkout session created: {TransactionId}", result.TransactionId);

                    // Pending subscription kaydı oluştur (webhook geldiğinde güncellenecek)
                    if (request.ClinicId.HasValue && request.ClinicId.Value > 0)
                    {
                        var subscription = new Subscription
                        {
                            ClinicId = request.ClinicId.Value,
                            PlanId = request.PlanId,
                            Status = SubscriptionStatus.PendingPayment,
                            PaddleTransactionId = result.TransactionId,
                            AmountPaid = plan.Price,
                            Currency = plan.Currency,
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddMonths(1), // Varsayılan olarak 1 ay
                            PaymentMethod = "Paddle"
                        };

                        _context.Subscriptions.Add(subscription);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Pending subscription created for clinic {ClinicId}", request.ClinicId);
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
    }
}
