using System;
using System.Collections.Generic;

namespace backend;

public partial class Doctor
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? PhoneCountryCode { get; set; }

    public string Specialty { get; set; } = null!;

    public int YearsExperience { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public string? WorkingHours { get; set; }

    public bool IsActive { get; set; }

    public string? AvatarUrl { get; set; }

    public float Rating { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? ClinicId { get; set; }

    public virtual Clinic? Clinic { get; set; }
}
