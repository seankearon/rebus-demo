using common;
using messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Auditing.Messages;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;

namespace api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Extensions.SBConnectionString    = configuration["SBConnectionString"];
            Extensions.MSSqlConnectionString = configuration["MSSqlConnectionString"];
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Bus MacBusface
            ((ServiceCollection) services).AddRebus(configure => configure
                                                                .Logging(l => l.SetLogging())
                                                                .Routing(r => r.TypeBased().Map<OnboardCustomer>(Queues.CreateCustomerService))
                                                                .Options(t => t.SimpleRetryStrategy(maxDeliveryAttempts: 2, errorQueueAddress: Queues.Error))
                                                                .Options(t => t.EnableMessageAuditing(Queues.Audit))
                                                                .Transport(t => t.UseAzureServiceBusAsOneWayClient(Extensions.SBConnectionString))
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}