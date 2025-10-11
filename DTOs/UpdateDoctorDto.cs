using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UpdateDoctorDto
    {
        [Required(ErrorMessage = "Ad tələb olunur")]
        [StringLength(100, ErrorMessage = "Ad 100 simvoldan çox ola bilməz")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad tələb olunur")]
        [StringLength(100, ErrorMessage = "Soyad 100 simvoldan çox ola bilməz")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail tələb olunur")]
        [EmailAddress(ErrorMessage = "Düzgün e-mail formatı daxil edin")]
        [StringLength(255, ErrorMessage = "E-mail 255 simvoldan çox ola bilməz")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Telefon 20 simvoldan çox ola bilməz")]
        public string? Phone { get; set; }

        [StringLength(10, ErrorMessage = "Ölkə kodu 10 simvoldan çox ola bilməz")]
        public string? PhoneCountryCode { get; set; }

        [Required(ErrorMessage = "İxtisas tələb olunur")]
        [StringLength(100, ErrorMessage = "İxtisas 100 simvoldan çox ola bilməz")]
        public string Specialty { get; set; } = string.Empty;

        [Range(0, 50, ErrorMessage = "Təcrübə 0-50 arasında olmalıdır")]
        public int YearsExperience { get; set; }

        [Range(18, 100, ErrorMessage = "Yaş 18-100 arasında olmalıdır")]
        public int? Age { get; set; }

        [StringLength(20, ErrorMessage = "Cins 20 simvoldan çox ola bilməz")]
        public string? Gender { get; set; }

        [StringLength(50, ErrorMessage = "Çalışma saatları 50 simvoldan çox ola bilməz")]
        public string? WorkingHours { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "Avatar URL 500 simvoldan çox ola bilməz")]
        public string? AvatarUrl { get; set; }

        [Range(0.0, 5.0, ErrorMessage = "Reytinq 0-5 arasında olmalıdır")]
        public double Rating { get; set; } = 0.0;

        public int? ClinicId { get; set; }
    }
}