namespace messages
{
    public class OnboardCustomer
    {
        public string CustomerName { get; set; }
    }

    public class CreateCustomerAccount
    {
        public string CustomerName { get; set; }
    }

    public class CustomerAccountCreated
    {
        public string CustomerName { get; set; }
    }

    public class SendWelcomePackToCustomer
    {
        public string CustomerName { get; set; }
    }

    public class WelcomePackSentToCustomer
    {
        public string CustomerName { get; set; }
    }

    public class ScheduleASalesCall
    {
        public string CustomerName { get; set; }
    }

    public class CancellSalesCall
    {
        public string CustomerName { get; set; }
    }

    public class CustomerOnboarded
    {
        public string CustomerName { get; set; }
    }

    public class VerifyCustomerOnboardingOla
    {
        public string CustomerName { get; set; }
    }

    public class CustomerOnboardingOlaBreached
    {
        public string CustomerName { get; set; }
    }

}
