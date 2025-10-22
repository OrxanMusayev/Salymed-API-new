using System;
using System.Collections.Generic;

namespace backend;

public partial class Clinic
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Address { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? PhoneCountryCode { get; set; }

    public string? Type { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? OwnerId { get; set; }

    public bool RegistrationCompleted { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public virtual User? Owner { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
