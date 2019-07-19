using System;
using System.Threading.Tasks;
using common;
using messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;
using Rebus.ServiceProvider;

namespace receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            Extensions.SBConnectionString = configuration["SBConnectionString"];

            var services = new ServiceCollection();
            services.AutoRegisterHandlersFromAssemblyOf<Program>();
            services.ConfigureAsReceiver(Queues.Receiver);

            using (var provider = services.BuildServiceProvider())
            {
                provider.UseRebus(bus => { bus.Subscribe<SimonsNews>(); });

                while (true)
                {
                    Console.ReadLine();
                }
            }
        }
    }

    public class NewsHandler : IHandleMessages<SimonsNews>
    {
        public Task Handle(SimonsNews m)
        {
            $"Received {m.GetType().Name}: {m.Message}".LogColored();
            return Task.CompletedTask;
        }
    }

    public class OrdersHandler : IHandleMessages<SimonsOrders>
    {
        public Task Handle(SimonsOrders m)
        {
            $"Received {m.GetType().Name}: {m.Message}".LogColored();
            return Task.CompletedTask;
        }
    }
}
