using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        // Foreign Keys
        [Required]
        public int ClinicId { get; set; }

        [Required]
        public int PlanId { get; set; }

        /// <summary>
        /// Abunəliyin statusu
        /// </summary>
        [Required]
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;

        /// <summary>
        /// Abunəliyin başlama tarixi
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Abunəliyin bitmə tarixi
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Növbəti ödəniş tarixi
        /// </summary>
        public DateTime? NextBillingDate { get; set; }

        /// <summary>
        /// Ödənilmiş məbləğ
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Valyuta
        /// </summary>
        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Ödəniş üsulu
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Ödəniş əməliyyatının ID-si
        /// </summary>
        [StringLength(255)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Paddle Transaction ID
        /// </summary>
        [StringLength(255)]
        public string? PaddleTransactionId { get; set; }

        /// <summary>
        /// Paddle Subscription ID (təkrar ödəmələr üçün)
        /// </summary>
        [StringLength(255)]
        public string? PaddleSubscriptionId { get; set; }

        /// <summary>
        /// Sınaq müddəti aktivdir
        /// </summary>
        public bool IsTrialPeriod { get; set; } = false;

        /// <summary>
        /// Sınaq müddətinin bitmə tarixi
        /// </summary>
        public DateTime? TrialEndDate { get; set; }

        /// <summary>
        /// Avtomatik yeniləmə aktivdir
        /// </summary>
        public bool AutoRenew { get; set; } = true;

        /// <summary>
        /// Abunəliyin ləğv edilmə tarixi
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Ləğv etmə səbəbi
        /// </summary>
        [StringLength(500)]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Faktura məlumatları (JSON)
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        public string? InvoiceDetails { get; set; }

        /// <summary>
        /// Əlavə qeydlər
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ClinicId")]
        public virtual Clinic? Clinic { get; set; }

        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan? Plan { get; set; }
    }
}
