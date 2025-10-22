using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class PlanFeature
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Premium xüsusiyyətmi
        /// </summary>
        public bool IsPremium { get; set; } = false;

        /// <summary>
        /// Feature aktivdir
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Göstərilmə sırası
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PlanFeatureMapping>? PlanFeatureMappings { get; set; }
    }
}
