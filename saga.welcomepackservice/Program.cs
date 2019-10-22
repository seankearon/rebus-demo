using System;
using System.Threading.Tasks;
using common;
using messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Auditing.Messages;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Retry.Simple;
using Rebus.ServiceProvider;

namespace saga.welcomepackservice
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            Extensions.SBConnectionString    = configuration["SBConnectionString"];
            Extensions.MSSqlConnectionString = configuration["MSSqlConnectionString"];

            var services = new ServiceCollection();
            services.AutoRegisterHandlersFromAssemblyOf<Program>();

            Extensions.SetTitle("welcome pack service");

            services.AddRebus(configure => configure
                .Logging(l => l.SetLogging())
                //.Routing(r => r.TypeBased().Map<SendWelcomePackToCustomer>(Queues.WelcomePackService))
                .Options(t => t.SimpleRetryStrategy(maxDeliveryAttempts: 2, errorQueueAddress: Queues.Error))
                .Options(t => t.EnableMessageAuditing(Queues.Audit))
                //.Transport(t => t.UseAzureServiceBus(Extensions.SBConnectionString, Queues.WelcomePackService))
                .Transport(t => t.UseSqlServer(Extensions.MSSqlConnectionString, Queues.WelcomePackService))
                .Subscriptions(t => t.StoreInSqlServer(Extensions.MSSqlConnectionString, "Subscriptions"))
            );


            using (var provider = services.BuildServiceProvider())
            {
                provider.UseRebus(bus => { bus.Subscribe<SendWelcomePackToCustomer>(); });

                while (true)
                {
                    Console.ReadLine();
                }
            }
        }
    }

    public class SendWelcomePackToCustomerHandler : IHandleMessages<SendWelcomePackToCustomer>
    {
        private readonly IBus _bus;

        public SendWelcomePackToCustomerHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(SendWelcomePackToCustomer m)
        {
            $"HANDLER - Sending a welcome pack to {m.CustomerName}...".Log();
            3.SecondsSleep();
            $"HANDLER - Welcome pack sent to {m.CustomerName}.".Log();

            await _bus.Reply(new WelcomePackSentToCustomer {CustomerName = m.CustomerName});
        }
    }
}
