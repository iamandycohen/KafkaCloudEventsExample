using Kafka.EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kafka.Producer
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

            // configure the producer for the tenants topic
            services.RegisterCloudEventProducer<TenantsProducerOptions>(_configuration, TenantsProducerOptions.SectionName);
        }
    }
}
