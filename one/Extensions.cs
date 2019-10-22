using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using messages;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Auditing.Messages;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Extensions;
using Rebus.Persistence.FileSystem;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.Transport.InMem;

// ReSharper disable once CheckNamespace
namespace common
{
    internal static class Queues
    {
        // We use one queue per logical receiver.
        public static string Receiver => "QueueForReceiver";
        public static string One => "QueueForOne";
        public static string Audit => "Audit";
        public static string Error => "Error";
    }

    internal static class Extensions
    {
        public static string ConnectionString { get; set; }

        public static void SetLogging(this RebusLoggingConfigurer l)
        {
            //l.ColoredConsole();
            l.None();
        }

        public static string LogColored(this string msg, ConsoleColor color = ConsoleColor.DarkCyan)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
            return msg;
        }

        public static void SetTitle(string msg)
        {
            Console.Title = Assembly.GetEntryAssembly()?.GetName().Name + " - " + msg;
        }

        public static ServiceCollection ConfigureTwoWay(this ServiceCollection services, string queueName)
        {
            SetTitle("2-way");

            services.AddRebus(configure => configure
                    .Logging(l => l.SetLogging())
                    .Routing(r => r.TypeBased().MapAssemblyOf<SimonsOrders>(Queues.Receiver))
                    .Options(t => t.SimpleRetryStrategy(maxDeliveryAttempts: 2, errorQueueAddress: Queues.Error))
                    .Options(t => t.EnableMessageAuditing(Queues.Audit))
                    .Transport(t => t.UseSqlServer(ConnectionString, queueName))
                    .Subscriptions(t => t.UseJsonFile("C:/Bus/subscriptions.json"))

                //.Transport(t => t.UseAzureServiceBus(ConnectionString, queueName).AutomaticallyRenewPeekLock())
                // ASB has native support for subscription storage.  .Subscriptions(t => t.UseJsonFile("C:/Bus/subscriptions.json"))
            );

            return services;
        }

        public static ServiceCollection ConfigureAsSender(this ServiceCollection services)
        {
            SetTitle("publisher");

            services.AddRebus(configure => configure
                .Logging(l => l.SetLogging())
                .Routing(r => r.TypeBased().Map<SimonsOrders>(Queues.Receiver))
                .Options(t => t.SimpleRetryStrategy(maxDeliveryAttempts: 2, errorQueueAddress: Queues.Error))
                .Options(t => t.EnableMessageAuditing(Queues.Audit))
                .Transport(t => t.UseSqlServerAsOneWayClient(ConnectionString))
                .Subscriptions(t => t.UseJsonFile("C:/Bus/subscriptions.json"))
            );

            return services;
        }

        public static ServiceCollection ConfigureAsReceiver(this ServiceCollection services, string queueName)
        {
            services.ConfigureTwoWay(queueName);
            SetTitle("subscriber (actually can publish too)");
            return services;
        }

        public static void SubscribeToPocoMessageTypesInAssemblyOf<T>(this IBus bus)
        {
            typeof(T).Assembly
                .GetTypes()
                .Where(x => !x.IsAbstract && !x.IsInterface)
                .ForEach(x => bus.Subscribe(x));
        }

        public static async void SecondsSleep(this int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }
    }
}