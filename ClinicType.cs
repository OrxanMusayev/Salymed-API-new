using System;
using System.Collections.Generic;

namespace backend;

public partial class ClinicType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? TranslationKey { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
