using System;
using System.Collections.Generic;

namespace backend;

public partial class PlanFeature
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPremium { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PlanFeatureMapping> PlanFeatureMappings { get; set; } = new List<PlanFeatureMapping>();
}
