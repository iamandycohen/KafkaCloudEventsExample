using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kafka.EventBus.Extensions;
using Tenants.Options;
using Kafka.EventBus;
using Kafka.Consumer.CloudEvents;
using Kafka.EventBus.CloudEvents;

namespace Kafka.Consumer
{
    public class Startup
    {
        public IConfiguration _configuration { get; }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json");

            _configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddLogging(opt =>
            {
                opt.AddConsole();
            });
            services.AddSingleton(_configuration);
            services.AddOptions();

            // configure the default kafka connection options
            services.ConfigureKafkaConnectionOptions(_configuration);

            // register the cloud events payload type factory
            services.RegisterCloudEvents();

            // register topic consumer and message processor
            services.RegisterCloudEventConsumer<TenantsConsumerOptions, ExampleCloudEventsMessageProcessor>(_configuration, TenantsConsumerOptions.SectionName);
        }
    }
}
