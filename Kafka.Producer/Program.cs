using Kafka.EventBus;
using Kafka.EventBus.CloudEvents;
using Kafka.Producer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Kafka.Producer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            Startup startup = new();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            var tenantsProducer = serviceProvider.GetRequiredService<ICloudEventsProducer<TenantsProducerOptions>>();
            var environmentMessage = new EnvironmentMessage
            {
                Id = Guid.NewGuid().ToString(),
                Data = "This is a test!"
            };

            await tenantsProducer.PublishAsync(environmentMessage, environmentMessage.Id);

            var anotherMessage = new AnotherMessage
            {
                Id = Guid.NewGuid().ToString(),
                AnotherOne = "This is another test!"
            };

            await tenantsProducer.PublishAsync(anotherMessage, anotherMessage.Id);

        }
    }
}
