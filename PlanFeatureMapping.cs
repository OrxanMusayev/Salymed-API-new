using System;
using System.Collections.Generic;

namespace backend;

public partial class PlanFeatureMapping
{
    public int Id { get; set; }

    public int PlanId { get; set; }

    public int FeatureId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PlanFeature Feature { get; set; } = null!;

    public virtual SubscriptionPlan Plan { get; set; } = null!;
}
