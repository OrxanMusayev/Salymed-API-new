namespace backend.Models
{
    public enum SubscriptionStatus
    {
        /// <summary>
        /// Abunəlik aktiv vəziyyətdədir
        /// </summary>
        Active = 1,

        /// <summary>
        /// Abunəlik ləğv edilmişdir
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// Abunəlik müvəqqəti dayandırılmışdır
        /// </summary>
        Suspended = 3,

        /// <summary>
        /// Abunəlikin müddəti bitib
        /// </summary>
        Expired = 4,

        /// <summary>
        /// Abunəlik sınaq müddətindədir
        /// </summary>
        Trial = 5,

        /// <summary>
        /// Ödəniş gözlənilir
        /// </summary>
        PendingPayment = 6
    }
}
