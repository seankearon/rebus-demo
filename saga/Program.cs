using System;
using common;
using messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Auditing.Messages;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;

namespace saga
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            Extensions.SBConnectionString    = configuration["SBConnectionString"];
            Extensions.MSSqlConnectionString = configuration["MSSqlConnectionString"];

            var services = new ServiceCollection();
            services.AutoRegisterHandlersFromAssemblyOf<OnboardCustomerSaga>();

            // Bus MacBusface
            Extensions.SetTitle("saga / process manager");

            services.AddRebus(configure => configure
                // The same as for two-way.
                .Logging(l => l.SetLogging())

                // We're going to send commands to another endpoint, so we need to tell Rebus where to send those.  They are sent using Bus.Send().
                .Routing(r => r.TypeBased().Map<SendWelcomePackToCustomer>(Queues.WelcomePackService)

                // But, when the message handler is in the same endpoint as the sender, which is often the case, you can use Bus.SendLocal() and no routing configuration is needed.
                // So, you don't need to do map locally handled messages.
                //  .Map<ScheduleASalesCall>(Queues.CreateCustomerService)
                )

                .Options(t => t.SimpleRetryStrategy(maxDeliveryAttempts: 2, errorQueueAddress: Queues.Error))
                .Options(t => t.EnableMessageAuditing(Queues.Audit))
                //.Transport(t => t.UseAzureServiceBus(Extensions.SBConnectionString, Queues.CreateCustomerService).AutomaticallyRenewPeekLock())
                
                .Transport(t => t.UseSqlServer(Extensions.MSSqlConnectionString, Queues.CreateCustomerService))
                .Subscriptions(t => t.StoreInSqlServer(Extensions.MSSqlConnectionString, "Subscriptions"))

                // Just with the saga storage added
                .Sagas(x => x.StoreInSqlServer(Extensions.MSSqlConnectionString, "Sagas", "SagaIndexes"))
            );

            //services.AddScoped<CreateCustomerSaga>();

            using (var provider = services.BuildServiceProvider())
            {
                provider.UseRebus(bus =>
                {
                    // Subscribe to the messages that we handle.
                    bus.Subscribe<OnboardCustomer>();
                    bus.Subscribe<WelcomePackSentToCustomer>();
                });

                KeepRunning();
            }

        }

        private static void KeepRunning()
        {
            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}

