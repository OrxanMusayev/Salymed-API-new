using System;
using System.Collections.Generic;

namespace backend;

public partial class Subscription
{
    public int Id { get; set; }

    public Guid ClinicId { get; set; }

    public int PlanId { get; set; }

    public int Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? NextBillingDate { get; set; }

    public decimal AmountPaid { get; set; }

    public string Currency { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? PaddleTransactionId { get; set; }

    public string? PaddleSubscriptionId { get; set; }

    public bool IsTrialPeriod { get; set; }

    public DateTime? TrialEndDate { get; set; }

    public bool AutoRenew { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public string? InvoiceDetails { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool HasActivePaymentProcess { get; set; }

    public virtual Clinic Clinic { get; set; } = null!;

    public virtual SubscriptionPlan Plan { get; set; } = null!;
}
