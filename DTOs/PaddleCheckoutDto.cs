namespace backend.DTOs
{
    public class CreateCheckoutRequestDto
    {
        public int PlanId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public int? ClinicId { get; set; }
        public int? UserId { get; set; }
    }

    public class CreateCheckoutResponseDto
    {
        public bool Success { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PaymentSuccessDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public int ClinicId { get; set; }
        public int PlanId { get; set; }
    }
}
