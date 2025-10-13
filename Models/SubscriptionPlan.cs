using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Paddle Price ID (Paddle-də yaradılmış məhsul qiyməti)
        /// </summary>
        [StringLength(100)]
        public string? PaddlePriceId { get; set; }

        /// <summary>
        /// Ödəniş dövrü (Həftəlik, Aylıq, İllik)
        /// </summary>
        [Required]
        public BillingPeriod Period { get; set; } = BillingPeriod.Monthly;

        /// <summary>
        /// Plan aktivdir
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ən populyar plan
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Göstərilmə sırası
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Subscription>? Subscriptions { get; set; }
        public virtual ICollection<PlanFeatureMapping>? PlanFeatures { get; set; }
    }
}
