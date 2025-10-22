using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Klinik ID
        /// </summary>
        [Required]
        public Guid ClinicId { get; set; }

        /// <summary>
        /// İstifadəçi ID (nullable - clinic owner ola bilər)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Paddle Payment Method ID
        /// </summary>
        [StringLength(255)]
        public string? PaddlePaymentMethodId { get; set; }

        /// <summary>
        /// Kart tipi (visa, mastercard, amex, etc.)
        /// </summary>
        [StringLength(50)]
        public string? CardType { get; set; }

        /// <summary>
        /// Kartın son 4 rəqəmi
        /// </summary>
        [StringLength(4)]
        public string? CardLast4 { get; set; }

        /// <summary>
        /// Kartın bitmə ayı
        /// </summary>
        public int? CardExpiryMonth { get; set; }

        /// <summary>
        /// Kartın bitmə ili
        /// </summary>
        public int? CardExpiryYear { get; set; }

        /// <summary>
        /// Kart sahibinin adı
        /// </summary>
        [StringLength(255)]
        public string? CardholderName { get; set; }

        /// <summary>
        /// Ödəniş metodunun tipi (card, paypal, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "card";

        /// <summary>
        /// Default ödəniş metodu
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Aktiv ödəniş metodu
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Billing address JSON
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        public string? BillingAddressJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ClinicId")]
        public virtual Clinic? Clinic { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
