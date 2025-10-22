namespace backend.Configuration
{
    public class SubscriptionSettings
    {
        public bool HasTrialPeriod { get; set; } = false;
        public int TrialPeriodMonths { get; set; } = 1;
    }
}
