namespace backend.DTOs
{
    public class DoctorResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? PhoneCountryCode { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public int YearsExperience { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? WorkingHours { get; set; }
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ClinicId { get; set; }
        public string? ClinicName { get; set; } // Clinic adı da göstərmək üçün
    }
}