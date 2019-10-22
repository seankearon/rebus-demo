using common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.ServiceProvider;

namespace sender
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            Extensions.ConnectionString = configuration["MSSqlConnectionString"];

            var services = new ServiceCollection();
            services.AddSingleton<Producer>();

            // Bus MacBusface
            services.ConfigureAsSender();

            using (var provider = services.BuildServiceProvider())
            {
                provider.UseRebus();

                // The bus is started.  Begin the domain work.
                var producer = provider.GetRequiredService<Producer>();
                producer.Produce();
            }
        }
    }
}
