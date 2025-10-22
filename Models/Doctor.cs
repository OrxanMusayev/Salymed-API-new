using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Doctor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(10)]
        public string? PhoneCountryCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [Range(0, 50)]
        public int YearsExperience { get; set; }

        [Range(18, 100)]
        public int? Age { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [StringLength(50)]
        public string? WorkingHours { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [Range(0.0, 5.0)]
        public double Rating { get; set; } = 0.0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key - hansı klinikə aid olduğu
        public Guid? ClinicId { get; set; }

        // Navigation properties
        [ForeignKey("ClinicId")]
        public virtual Clinic? Clinic { get; set; }
    }
}