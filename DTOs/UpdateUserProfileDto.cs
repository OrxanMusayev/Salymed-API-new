using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UpdateUserProfileDto
    {
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
        public string? PhoneNumber { get; set; }

        [StringLength(10)]
        public string? PhoneCountryCode { get; set; }

        [StringLength(5)]
        public string? PreferredLanguage { get; set; }
    }
}
