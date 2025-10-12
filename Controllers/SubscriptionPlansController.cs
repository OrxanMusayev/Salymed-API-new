using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly ILogger<SubscriptionPlansController> _logger;

        public SubscriptionPlansController(
            ISubscriptionPlanService subscriptionPlanService,
            ILogger<SubscriptionPlansController> logger)
        {
            _subscriptionPlanService = subscriptionPlanService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active subscription plans with their features
        /// </summary>
        /// <returns>List of subscription plans</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SubscriptionPlansResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SubscriptionPlansResponseDto>> GetAllPlans()
        {
            try
            {
                _logger.LogInformation("Fetching all subscription plans");

                var response = await _subscriptionPlanService.GetAllPlansAsync();

                if (!response.Success)
                {
                    _logger.LogError("Error fetching subscription plans: {Message}", response.Message);
                    return StatusCode(500, response);
                }

                _logger.LogInformation("Successfully fetched {Count} subscription plans", response.Plans.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching subscription plans");
                return StatusCode(500, new SubscriptionPlansResponseDto
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    Plans = new List<SubscriptionPlanDto>()
                });
            }
        }

        /// <summary>
        /// Get a specific subscription plan by ID
        /// </summary>
        /// <param name="id">Plan ID</param>
        /// <returns>Subscription plan details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SubscriptionPlanDto>> GetPlanById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching subscription plan with ID: {PlanId}", id);

                var plan = await _subscriptionPlanService.GetPlanByIdAsync(id);

                if (plan == null)
                {
                    _logger.LogWarning("Subscription plan with ID {PlanId} not found", id);
                    return NotFound(new { message = $"Subscription plan with ID {id} not found" });
                }

                _logger.LogInformation("Successfully fetched subscription plan: {PlanName}", plan.Name);
                return Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subscription plan with ID: {PlanId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
}
