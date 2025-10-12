using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ILogger<SubscriptionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get subscription status for the current user/clinic
        /// This is a placeholder endpoint that returns mock data
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(SubscriptionStatusDto), StatusCodes.Status200OK)]
        public ActionResult<SubscriptionStatusDto> GetStatus()
        {
            _logger.LogInformation("Fetching subscription status");

            // TODO: Implement actual subscription status logic
            // For now, return a mock response
            var status = new SubscriptionStatusDto
            {
                HasActiveSubscription = false,
                SubscriptionType = null,
                ExpiresAt = null,
                StartedAt = null
            };

            return Ok(status);
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
