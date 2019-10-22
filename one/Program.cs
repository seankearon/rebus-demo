using common;
using messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.ServiceProvider;

namespace one
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            Extensions.ConnectionString = configuration["MSSqlConnectionString"];

            var services = new ServiceCollection();

            services.AutoRegisterHandlersFromAssemblyOf<SimonSaysHandler>();
            services.ConfigureTwoWay(Queues.One);

            services.AddSingleton<Producer>();

            using (var provider = services.BuildServiceProvider())
            {
                provider.UseRebus( bus => bus.SubscribeToPocoMessageTypesInAssemblyOf<SimonsNews>());

                // The bus is started.  Begin the domain work.
                var producer = provider.GetRequiredService<Producer>();
                producer.Produce();
            }
        }
    }
}
