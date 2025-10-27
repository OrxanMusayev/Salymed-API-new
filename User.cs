using System;
using System.Collections.Generic;

namespace backend;

public partial class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? PhoneCountryCode { get; set; }

    public string Role { get; set; } = null!;

    public string PreferredLanguage { get; set; } = "az";

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
}
