using backend.Models;

namespace backend.DTOs
{
    public class SubscriptionPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public string Period { get; set; } = "monthly";
        public bool IsFeatured { get; set; }
        public List<PlanFeatureDto> Features { get; set; } = new();
    }

    public class SubscriptionPlansResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<SubscriptionPlanDto> Plans { get; set; } = new();
    }
}
