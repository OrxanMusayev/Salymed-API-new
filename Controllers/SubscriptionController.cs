using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly ApplicationDbContext _context;

        public SubscriptionController(
            ILogger<SubscriptionController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Get subscription status for the current user/clinic
        /// Accepts clinicId as query parameter
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(SubscriptionStatusDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SubscriptionStatusDto>> GetStatus([FromQuery] Guid? clinicId)
        {
            _logger.LogInformation("Fetching subscription status for clinicId: {ClinicId}", clinicId);

            // If clinicId is not provided, return no active subscription
            if (!clinicId.HasValue)
            {
                _logger.LogWarning("No clinicId provided");
                return Ok(new SubscriptionStatusDto
                {
                    HasActiveSubscription = false,
                    SubscriptionType = null,
                    ExpiresAt = null,
                    StartedAt = null
                });
            }

            try
            {
                // Get the most recent active subscription for the clinic
                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.ClinicId == clinicId.Value)
                    .Where(s => s.Status == (int)SubscriptionStatus.Active)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    _logger.LogInformation("No active subscription found for clinicId: {ClinicId}", clinicId);
                    return Ok(new SubscriptionStatusDto
                    {
                        HasActiveSubscription = false,
                        SubscriptionType = null,
                        ExpiresAt = null,
                        StartedAt = null
                    });
                }

                // Check if subscription has expired
                if (subscription.EndDate < DateTime.UtcNow)
                {
                    _logger.LogInformation("Subscription expired for clinicId: {ClinicId}", clinicId);

                    // Update subscription status to expired
                    subscription.Status = (int)SubscriptionStatus.Expired;
                    await _context.SaveChangesAsync();

                    return Ok(new SubscriptionStatusDto
                    {
                        HasActiveSubscription = false,
                        SubscriptionType = subscription.Plan?.Name,
                        ExpiresAt = subscription.EndDate,
                        StartedAt = subscription.StartDate
                    });
                }

                // Return active subscription
                _logger.LogInformation("Active subscription found for clinicId: {ClinicId}", clinicId);
                return Ok(new SubscriptionStatusDto
                {
                    HasActiveSubscription = true,
                    SubscriptionType = subscription.Plan?.Name ?? "Unknown",
                    ExpiresAt = subscription.EndDate,
                    StartedAt = subscription.StartDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subscription status for clinicId: {ClinicId}", clinicId);
                return StatusCode(500, new { message = "Error fetching subscription status", error = ex.Message });
            }
        }

        /// <summary>
        /// Get current subscription details for a clinic
        /// </summary>
        [HttpGet("current")]
        [ProducesResponseType(typeof(CurrentSubscriptionDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CurrentSubscriptionDto>> GetCurrentSubscription([FromQuery] Guid? clinicId)
        {
            _logger.LogInformation("Fetching current subscription details for clinicId: {ClinicId}", clinicId);

            // If clinicId is not provided, return no subscription
            if (!clinicId.HasValue)
            {
                _logger.LogWarning("No clinicId provided");
                return Ok(new CurrentSubscriptionDto
                {
                    HasActiveSubscription = false
                });
            }

            try
            {
                // Get the active subscription with plan details and features
                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                        .ThenInclude(p => p.PlanFeatureMappings)
                            .ThenInclude(pf => pf.Feature)
                    .Where(s => s.ClinicId == clinicId.Value)
                    .Where(s => s.Status == (int)SubscriptionStatus.Active)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (subscription == null || subscription.EndDate < DateTime.UtcNow)
                {
                    _logger.LogInformation("No active subscription found for clinicId: {ClinicId}", clinicId);
                    return Ok(new CurrentSubscriptionDto
                    {
                        HasActiveSubscription = false
                    });
                }

                // Get clinic statistics
                var doctorCount = await _context.Doctors
                    .Where(d => d.ClinicId == clinicId.Value && d.IsActive)
                    .CountAsync();

                // Map plan features
                var features = subscription.Plan?.PlanFeatureMappings?
                    .OrderBy(pf => pf.Feature.DisplayOrder)
                    .Select(pf => pf.Feature.Name)
                    .ToList() ?? new List<string>();

                // Get billing history (last 12 months)
                var billingHistory = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.ClinicId == clinicId.Value)
                    .Where(s => s.Status == (int)SubscriptionStatus.Active || s.Status == (int)SubscriptionStatus.Expired)
                    .Where(s => s.CreatedAt >= DateTime.UtcNow.AddMonths(-12))
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new BillingInvoiceDto
                    {
                        Id = s.TransactionId ?? s.Id.ToString(),
                        Date = s.CreatedAt,
                        PlanName = s.Plan.Name,
                        Amount = s.AmountPaid,
                        Currency = s.Currency,
                        Status = s.Status == (int)SubscriptionStatus.Active || s.Status == (int)SubscriptionStatus.Expired ? "paid" : "pending"
                    })
                    .ToListAsync();

                // Get payment methods
                var paymentMethods = await _context.PaymentMethods
                    .Where(pm => pm.ClinicId == clinicId.Value && pm.IsActive)
                    .OrderByDescending(pm => pm.IsDefault)
                    .ThenByDescending(pm => pm.CreatedAt)
                    .Select(pm => new PaymentMethodDto
                    {
                        Id = pm.Id,
                        CardType = pm.CardType,
                        CardLast4 = pm.CardLast4,
                        CardExpiryMonth = pm.CardExpiryMonth,
                        CardExpiryYear = pm.CardExpiryYear,
                        CardholderName = pm.CardholderName,
                        Type = pm.Type,
                        IsDefault = pm.IsDefault,
                        IsActive = pm.IsActive,
                        CreatedAt = pm.CreatedAt
                    })
                    .ToListAsync();

                var response = new CurrentSubscriptionDto
                {
                    HasActiveSubscription = true,
                    CurrentPlan = new CurrentSubscriptionPlanDto
                    {
                        Id = subscription.Plan.Id.ToString(),
                        Name = subscription.Plan.Name,
                        Price = subscription.Plan.Price,
                        Currency = subscription.Plan.Currency,
                        Description = subscription.Plan.Description,
                        Features = features,
                        Period = subscription.Plan.Period.ToString()
                    },
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    NextBillingDate = subscription.NextBillingDate ?? subscription.EndDate,
                    AutoRenew = subscription.AutoRenew,
                    IsTrialPeriod = subscription.IsTrialPeriod,
                    TrialEndDate = subscription.TrialEndDate,
                    UsageStats = new UsageStatsDto
                    {
                        DoctorCount = doctorCount,
                        // These can be extended with real data
                        AppointmentCount = 0,
                        ReportCount = 0,
                        StorageUsedMB = 0
                    },
                    BillingHistory = billingHistory,
                    PaymentMethods = paymentMethods
                };

                _logger.LogInformation("Current subscription details fetched for clinicId: {ClinicId}", clinicId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current subscription for clinicId: {ClinicId}", clinicId);
                return StatusCode(500, new { message = "Error fetching subscription details", error = ex.Message });
            }
        }

        /// <summary>
        /// Manually activate a pending subscription (for testing/troubleshooting)
        /// </summary>
        [HttpPost("activate/{clinicId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivatePendingSubscription(Guid clinicId)
        {
            _logger.LogInformation("Manually activating pending subscription for clinicId: {ClinicId}", clinicId);

            try
            {
                // Get the most recent pending subscription
                var subscription = await _context.Subscriptions
                    .Where(s => s.ClinicId == clinicId)
                    .Where(s => s.Status == (int)SubscriptionStatus.PendingPayment)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    _logger.LogWarning("No pending subscription found for clinicId: {ClinicId}", clinicId);
                    return NotFound(new { message = "No pending subscription found for this clinic" });
                }

                // Activate the subscription
                subscription.Status = (int)SubscriptionStatus.Active;
                subscription.StartDate = DateTime.UtcNow;
                subscription.EndDate = DateTime.UtcNow.AddMonths(1);
                subscription.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Subscription activated for clinicId: {ClinicId}", clinicId);

                return Ok(new
                {
                    message = "Subscription activated successfully",
                    clinicId = clinicId,
                    subscriptionId = subscription.Id,
                    startDate = subscription.StartDate,
                    endDate = subscription.EndDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating subscription for clinicId: {ClinicId}", clinicId);
                return StatusCode(500, new { message = "Error activating subscription", error = ex.Message });
            }
        }
    }

    public class SubscriptionStatusDto
    {
        public bool HasActiveSubscription { get; set; }
        public string? SubscriptionType { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? StartedAt { get; set; }
    }

    public class CurrentSubscriptionDto
    {
        public bool HasActiveSubscription { get; set; }
        public CurrentSubscriptionPlanDto? CurrentPlan { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public bool AutoRenew { get; set; }
        public bool IsTrialPeriod { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public UsageStatsDto? UsageStats { get; set; }
        public List<BillingInvoiceDto>? BillingHistory { get; set; }
        public List<PaymentMethodDto>? PaymentMethods { get; set; }
    }

    public class CurrentSubscriptionPlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public List<string> Features { get; set; } = new List<string>();
        public string Period { get; set; } = "Monthly";
    }

    public class UsageStatsDto
    {
        public int DoctorCount { get; set; }
        public int AppointmentCount { get; set; }
        public int ReportCount { get; set; }
        public int StorageUsedMB { get; set; }
    }

    public class BillingInvoiceDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "pending";
    }

    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string? CardType { get; set; }
        public string? CardLast4 { get; set; }
        public int? CardExpiryMonth { get; set; }
        public int? CardExpiryYear { get; set; }
        public string? CardholderName { get; set; }
        public string Type { get; set; } = "card";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
