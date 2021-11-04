using Kafka.EventBus.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tenants.Options;

namespace Kafka.Consumer
{
    internal class Program
    {
        private static ILogger<Program>? logger;

        static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            Startup startup = new();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            var environmentConsumer = serviceProvider.GetRequiredService<ICloudEventsConsumer<TenantsConsumerOptions>>();

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Canceling...");
                cts.Cancel();
                e.Cancel = true;
            };

            await DoWork(environmentConsumer, cts.Token);
        }

        static async Task DoWork(ICloudEventsConsumer<TenantsConsumerOptions> consumer, CancellationToken cancellationToken)
        {
            await consumer.SubscribeAsync(cancellationToken);
        }

    }
}
