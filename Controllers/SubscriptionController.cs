using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

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
                    .Where(s => s.Status == SubscriptionStatus.Active)
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
                    subscription.Status = SubscriptionStatus.Expired;
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
                    .Where(s => s.Status == SubscriptionStatus.PendingPayment)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    _logger.LogWarning("No pending subscription found for clinicId: {ClinicId}", clinicId);
                    return NotFound(new { message = "No pending subscription found for this clinic" });
                }

                // Activate the subscription
                subscription.Status = SubscriptionStatus.Active;
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
}
