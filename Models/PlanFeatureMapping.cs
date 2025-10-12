using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class PlanFeatureMapping
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        public int FeatureId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan? Plan { get; set; }

        [ForeignKey("FeatureId")]
        public virtual PlanFeature? Feature { get; set; }
    }
}
