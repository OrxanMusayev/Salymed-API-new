namespace backend.DTOs
{
    public class ValidateCheckoutStateRequestDto
    {
        public Guid? ClinicId { get; set; }
        public int? PlanId { get; set; }
    }

    public class CheckoutStateValidationDto
    {
        public bool IsValid { get; set; }
        public bool RegistrationCompleted { get; set; }
        public bool HasActivePaymentProcess { get; set; }
        public bool HasActiveSubscription { get; set; }
        public string? ActiveTransactionId { get; set; }
        public string? Message { get; set; }
    }
}
