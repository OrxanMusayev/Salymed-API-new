using System;
using System.Collections.Generic;

namespace backend;

public partial class SubscriptionPlan
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = null!;

    public string? PaddlePriceId { get; set; }

    public int Period { get; set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PlanFeatureMapping> PlanFeatureMappings { get; set; } = new List<PlanFeatureMapping>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
