using System;
using System.Threading.Tasks;
using common;
using messages;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace saga
{
    public class OnboardCustomerSagaData : ISagaData
    {
        // these two are required by Rebus
        public Guid Id { get; set; }
        public int Revision { get; set; }

        public string CustomerName { get; set; }
        public bool AccountCreated { get; set; }
        public bool SalesCallScheduled { get; set; }
        public bool WelcomePackSent { get; set; }

        public bool Completed => AccountCreated && WelcomePackSent && SalesCallScheduled;
    }

    public class OnboardCustomerSaga : Saga<OnboardCustomerSagaData>,
        IAmInitiatedBy<OnboardCustomer>,
        IHandleMessages<CustomerAccountCreated>,
        IHandleMessages<WelcomePackSentToCustomer>,
        IHandleMessages<VerifyCustomerOnboardingOla>
    {
        public static TimeSpan OlaTimeout => TimeSpan.FromSeconds(10); // In reality your timeout would be a bit longer!

        public OnboardCustomerSaga(IBus bus)
        {
            _bus = bus;
        }

        private readonly IBus _bus;

        protected override void CorrelateMessages(ICorrelationConfig<OnboardCustomerSagaData> config)
        {
            config.Correlate<OnboardCustomer>(m => m.CustomerName, d => d.CustomerName);
            config.Correlate<CustomerAccountCreated>(m => m.CustomerName, d => d.CustomerName);
            config.Correlate<WelcomePackSentToCustomer>(m => m.CustomerName, d => d.CustomerName);
            config.Correlate<VerifyCustomerOnboardingOla>(m => m.CustomerName, d => d.CustomerName);
        }

        public async Task Handle(OnboardCustomer m)
        {
            if (!IsNew) return; // This makes the create customer event idempotent.
            $"SAGA - starting onboarding of new customer {m.CustomerName}".Log();

            Data.CustomerName = m.CustomerName;
            await _bus.SendLocal(new CreateCustomerAccount {CustomerName                    = Data.CustomerName});
            await _bus.DeferLocal(OlaTimeout, new VerifyCustomerOnboardingOla {CustomerName = Data.CustomerName});
            CheckComplete();
        }

        public async Task Handle(CustomerAccountCreated m)
        {
            $"SAGA - acknowledging account creation for {Data.CustomerName}.".Log();
            Data.AccountCreated = true;
            await _bus.Send(  new SendWelcomePackToCustomer {CustomerName = Data.CustomerName}); // We're sending this to another endpoint.
            await _bus.SendLocal(new ScheduleASalesCall {CustomerName     = Data.CustomerName}); // Use SendLocal the handler is in the same endpoint.
            Data.SalesCallScheduled = true;
            CheckComplete();
        }

        public Task Handle(WelcomePackSentToCustomer m)
        {
            $"SAGA - acknowledging welcome pack sent to {Data.CustomerName}.".Log();
            Data.WelcomePackSent = true;
            CheckComplete();
            return Task.CompletedTask;
        }

        private void CheckComplete()
        {
            if (Data.Completed) $"SAGA - customer creation for {Data.CustomerName} COMPLETED.".Log(); else $"SAGA - customer creation for {Data.CustomerName} ONGOING.".Log();

            if (Data.Completed)
            {
                MarkAsComplete();
                _bus.Publish(new CustomerOnboarded {CustomerName = Data.CustomerName}); // Does nothing here as there are no subscribers to this message.
            }
        }

        public Task Handle(VerifyCustomerOnboardingOla m)
        {
            // If we receive this message, we have breached our OLA.
            $"SAGA - customer onboarding OLA breached for {Data.CustomerName}.".Log(ConsoleColor.Red);
            Compensate();
            _bus.SendLocal(new CustomerOnboardingOlaBreached {CustomerName = Data.CustomerName});
            MarkAsComplete();
            return Task.CompletedTask;
        }

        private void Compensate()
        {
            if (Data.SalesCallScheduled)
            {
                $"SAGA - cancelling sales call for {Data.CustomerName}.".Log(ConsoleColor.Red);
                _bus.SendLocal(new CancellSalesCall {CustomerName = Data.CustomerName});
            }
        }
    }

    public class CustomerAccountCreator : IHandleMessages<CreateCustomerAccount>
    {
        private readonly IBus _bus;

        public CustomerAccountCreator(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(CreateCustomerAccount m)
        {
            $"HANDLER - Creating an account for {m.CustomerName}...".Log();
            3.SecondsSleep();
            $"HANDLER - Account created for {m.CustomerName}.".Log();

            await _bus.Reply(new CustomerAccountCreated {CustomerName = m.CustomerName});
        }
    }

    public class SalesCallSchedulerSender : IHandleMessages<ScheduleASalesCall>
    {
        public Task Handle(ScheduleASalesCall m)
        {
            $"HANDLER - Sales call scheduled with {m.CustomerName}.".Log();
            return Task.CompletedTask;
        }
    }

    public class CustomerOnboardingOlaExceededHandler : IHandleMessages<CustomerOnboardingOlaBreached>
    {
        public Task Handle(CustomerOnboardingOlaBreached m)
        {
            $"HANDLER - onboarding OLA exceeded ticket logged with service desk for customer {m.CustomerName}...".Log(ConsoleColor.Red);
            return Task.CompletedTask;
        }
    }
}