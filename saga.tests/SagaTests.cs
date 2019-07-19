using System;
using System.IO;
using System.Threading.Tasks;
using messages;
using NUnit.Framework;
using Rebus.TestHelpers;

namespace saga.tests
{
    [TestFixture]
    public class SagaTests
    {
        private string RandomName => Path.GetFileNameWithoutExtension(Path.GetRandomFileName()).ToUpperInvariant();

        [Test]
        public void Onboarding_Causes_An_Account_To_Be_Created_For_The_Customer()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName = customerName});
                fixture.SagaDataShould(
                        have: x => x.CustomerName == customerName,
                        because: "store the customer name")
                   .SagaDataIsNotComplete();
                bus.HasSentLocal<CreateCustomerAccount>(
                        with: x => x.CustomerName == customerName,
                        because: "it should initiate account creation"
                    );


                fixture.Deliver(new CustomerAccountCreated{CustomerName = customerName});
                fixture.SagaDataShould(
                        have: x => x.AccountCreated,
                        because: "it should remember the account has been created")
                   .SagaDataIsNotComplete();

                fixture.ShouldNotHaveCompleted();
            }
        }

        [Test]
        public void A_Welcome_Pack_Is_Sent_To_The_Customer_After_Their_Account_Has_Been_Created()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName        = customerName});
                fixture.Deliver(new CustomerAccountCreated{CustomerName = customerName});

                fixture.SagaDataShould(
                            have: x => x.AccountCreated,
                            because: "it should remember the account has been created")
                       .SagaDataIsNotComplete();
                bus.HasSent<SendWelcomePackToCustomer>(
                    with: x => x.CustomerName == customerName,
                    because: "it should initiate welcome pack creation"
                );

                fixture.ShouldNotHaveCompleted();
            }
        }

        [Test]
        public void A_Welcome_Pack_Is_Not_Sent_To_The_Customer_After_Another_Account_Has_Been_Created()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName        = customerName});
                fixture.Deliver(new CustomerAccountCreated{CustomerName = RandomName});

                fixture.SagaDataShould(
                            have: x => !x.AccountCreated,
                            because: "it should not have reacted to the other customer's account being created");
                bus.HasNotSent<SendWelcomePackToCustomer>(
                    because: "it should initiate welcome pack creation because of another customer's account being created"
                );

                fixture.ShouldNotHaveCompleted();
            }
        }

        [Test]
        public void The_Sales_Team_Will_Set_Up_A_Call_With_The_Customer_After_Their_Account_Has_Been_Created()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName = customerName});
                fixture.Deliver(new CustomerAccountCreated{CustomerName = customerName});
                fixture.SagaDataShould(
                        have: x => x.AccountCreated,
                        because: "it should remember the account has been created")
                   .SagaDataIsNotComplete();

                bus.HasSentLocal<ScheduleASalesCall>(
                        with: x => x.CustomerName == customerName,
                        because: "a sales call is scheduled after the account is created");

                fixture.ShouldNotHaveCompleted();
            }
        }
        [Test]
        public void Other_Systems_Are_Notified_After_A_Succcessful_Onboarding()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName = customerName});
                fixture.Deliver(new CustomerAccountCreated{CustomerName = customerName});
                fixture.Deliver(new WelcomePackSentToCustomer{CustomerName = customerName});
                fixture.ShouldHaveCompleted();
                bus.HasPublished<CustomerOnboarded>(
                        with: x => x.CustomerName == customerName,
                        because: "the saga should publishes an event notifying of the customer onboarding")
                   .HasNotSentLocal<CustomerOnboardingOlaBreached>(
                        because: "the onboarding was successful");

                fixture.ShouldHaveCompleted();

                OnboardCustomerSagaData SagaDataShould(Predicate<OnboardCustomerSagaData> have, string because = null)
                {
                    return fixture.SagaData<OnboardCustomerSaga, OnboardCustomerSagaData>().Should(have, because);
                }
            }
        }

        [Test]
        public void The_Systems_Track_The_Ola_For_Customer_Onboarding()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName = customerName});
                bus.HasDeferredLocal<VerifyCustomerOnboardingOla>(
                        with: x => x.CustomerName == customerName,
                        because: "it should start tracking the OLA");
            }
        }

        [Test]
        public void If_The_Ola_Is_Breached_Then_The_Service_Desk_Takes_Over_The_Process()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName = customerName});
                bus.HasDeferredLocal<VerifyCustomerOnboardingOla>(
                    with: x => x.CustomerName == customerName,
                    because: "it should start tracking the OLA");

                fixture.Deliver(new VerifyCustomerOnboardingOla {CustomerName = customerName});

                fixture.ShouldHaveCompleted();
                bus.HasSentLocal<CustomerOnboardingOlaBreached>(
                    with: x => x.CustomerName == customerName,
                    because: "the service desk should be notified of the failed onboarding");
            }
        }

        [Test]
        public void If_The_Ola_Is_Breached_Then_Other_Systems_Will_Not_Be_Notified_About_The_Onboarding()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName = customerName});
                bus.HasDeferredLocal<VerifyCustomerOnboardingOla>(
                    with: x => x.CustomerName == customerName,
                    because: "it should start tracking the OLA");

                fixture.Deliver(new VerifyCustomerOnboardingOla {CustomerName = customerName});

                fixture.ShouldHaveCompleted();
                bus.HasNotPublished<CustomerOnboarded>(because: "the saga was not successful and there should be no event notifying of a customer onboarding");
            }
        }

        [Test]
        public void If_The_Ola_Is_Breached_Then_Any_Placed_Sales_Call_Is_Cancelled()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName        = customerName});
                fixture.Deliver(new CustomerAccountCreated{CustomerName = customerName});
                bus.HasSentLocal<ScheduleASalesCall>(
                    with: x => x.CustomerName == customerName,
                    because: "a sales call is scheduled after the account is created");

                fixture.Deliver(new VerifyCustomerOnboardingOla {CustomerName = customerName});

                fixture.ShouldHaveCompleted();
                bus.HasSentLocal<CancellSalesCall>(
                    with: x => x.CustomerName == customerName,
                    because: "any scheduled sales call is cancelled after an OLA breach");
            }
        }

        [Test]
        public void If_The_Ola_Is_Breached_Then_And_No_Sales_Call_Was_Placed_Then_No_Cancellation_Takes_Place()
        {
            var customerName = RandomName;
            var bus          = new FakeBus();
            using(var fixture = SagaFixture.For(() => new OnboardCustomerSaga(bus)))
            {
                fixture.Deliver(new OnboardCustomer{CustomerName = customerName});
                bus.HasNotSentLocal<ScheduleASalesCall>(
                    because: "the account creation is has not been confirmed");

                fixture.Deliver(new VerifyCustomerOnboardingOla {CustomerName = customerName});

                fixture.ShouldHaveCompleted();
                bus.HasNotSentLocal<CancellSalesCall>(
                    because: "the  sales call is only cancelled after an OLA breach if it has already been requested");
            }
        }
    }
}