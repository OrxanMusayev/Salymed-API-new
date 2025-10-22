using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique invoice number (can be Paddle transaction ID or generated)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string InvoiceNumber { get; set; } = string.Empty;

        // Foreign Keys
        [Required]
        public Guid ClinicId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [Required]
        public int PlanId { get; set; }

        /// <summary>
        /// Invoice status: Paid, Failed, Pending, Refunded
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Amount paid for this invoice
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency (USD, EUR, AZN, etc.)
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Is this a trial period payment (usually $0)
        /// </summary>
        public bool IsTrialPeriod { get; set; } = false;

        /// <summary>
        /// Original plan price (before trial discount)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        /// <summary>
        /// Billing period start date
        /// </summary>
        [Required]
        public DateTime BillingPeriodStart { get; set; }

        /// <summary>
        /// Billing period end date
        /// </summary>
        [Required]
        public DateTime BillingPeriodEnd { get; set; }

        /// <summary>
        /// Payment date
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Due date for payment
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Payment method used
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Paddle Transaction ID
        /// </summary>
        [StringLength(255)]
        public string? PaddleTransactionId { get; set; }

        /// <summary>
        /// Paddle Subscription ID
        /// </summary>
        [StringLength(255)]
        public string? PaddleSubscriptionId { get; set; }

        /// <summary>
        /// Paddle Invoice ID
        /// </summary>
        [StringLength(255)]
        public string? PaddleInvoiceId { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Discount amount
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Subtotal (before tax and discount)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Invoice details in JSON format (Paddle webhook data)
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        public string? InvoiceDetailsJson { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ClinicId")]
        public virtual Clinic? Clinic { get; set; }

        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }

        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan? Plan { get; set; }
    }
}
