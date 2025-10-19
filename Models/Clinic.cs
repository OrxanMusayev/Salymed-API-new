using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Clinic
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(10)]
        public string? PhoneCountryCode { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }
        
        [StringLength(255)]
        public string? Website { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(100)]
        public string? State { get; set; }
        
        [StringLength(20)]
        public string? ZipCode { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign key
        public Guid? OwnerId { get; set; }
        
        // Navigation properties
        public virtual User? Owner { get; set; }
    }
}