using Kafka.Consumer.CloudEvents;
using Kafka.EventBus.CloudEvents;
using Kafka.EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Consumer
{
    internal class Program
    {
        private static IConfiguration? _configuration;

        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(hostContext =>
                {
                    hostContext
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("appsettings.Development.json");
                })
                .ConfigureServices((hostContext, services) => 
                {
                    _configuration = hostContext.Configuration;
                    services.AddHostedService<ExampleConsumerService>();

                    services.AddMemoryCache();
                    services.AddOptions();
                    services.AddLogging(opt =>
                    {
                        opt.AddConsole();
                    });

                    // configure the default kafka connection options
                    services.ConfigureKafkaConnectionOptions(_configuration);

                    // register the cloud events payload type factory
                    services.RegisterCloudEvents();

                    // register topic consumer and message processor
                    services.RegisterCloudEventConsumer<TenantsConsumerOptions, ExampleCloudEventsMessageProcessor>(_configuration, TenantsConsumerOptions.SectionName);

                })
                .RunConsoleAsync();
        }

    }
}
