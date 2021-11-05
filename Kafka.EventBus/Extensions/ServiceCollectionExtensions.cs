using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Kafka.EventBus.Kafka;
using Kafka.EventBus.Options;
using Kafka.EventBus.CloudEvents;

namespace Kafka.EventBus.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection RegisterCloudEvents(this IServiceCollection services)
        {
            services.AddSingleton<ICloudEventsPayloadTypeFactory, CloudEventsPayloadTypeFactory>();

            return services;
        }

        public static IServiceCollection ConfigureKafkaConnectionOptions(this IServiceCollection services, IConfiguration configuration)
        {
            var kafkaConnectionOptions = configuration.GetSection(KafkaConnectionOptions.SectionName);
            services.Configure<KafkaConnectionOptions>(kafkaConnectionOptions);

            return services;
        }

        public static IServiceCollection RegisterCloudEventConsumer<TConsumerOptions, TMessageProcessor>(this IServiceCollection services, IConfiguration configuration, string sectionName)
            where TConsumerOptions : class, IKafkaConsumerOptions
            where TMessageProcessor : CloudEventsMessageProcessor<TConsumerOptions>
        {
            services = ConfigureKafkaConsumerOptions<TConsumerOptions>(services, configuration, sectionName);

            // register message processors for the topic
            services.AddSingleton<CloudEventsMessageProcessor<TConsumerOptions>, TMessageProcessor>();

            // register the consumer to the topic
            services.AddSingleton<ICloudEventsConsumer<TConsumerOptions>, CloudEventsConsumer<TConsumerOptions>>();

            return services;
        }

        public static IServiceCollection ConfigureKafkaConsumerOptions<TConsumerOptions>(this IServiceCollection services, IConfiguration configuration, string sectionName)
                where TConsumerOptions : class, IKafkaConsumerOptions
        {
            var kafkaConsumerOptions = configuration.GetSection(sectionName);
            services.Configure<TConsumerOptions>(kafkaConsumerOptions);

            return services;
        }

        public static IServiceCollection RegisterCloudEventProducer<TProducerOptions>(this IServiceCollection services, IConfiguration configuration, string sectionName)
            where TProducerOptions : class, IKafkaProducerOptions
        {
            services = RegisterCloudEventProducer<TProducerOptions, CloudEventsMappingStrategy<TProducerOptions>>(services, configuration, sectionName);

            return services;
        }

        public static IServiceCollection RegisterCloudEventProducer<TProducerOptions, TMappingStrategy>(this IServiceCollection services, IConfiguration configuration, string sectionName)
            where TProducerOptions : class, IKafkaProducerOptions
            where TMappingStrategy : class, ICloudEventsMappingStrategy<TProducerOptions>
        {
            services = ConfigureKafkaProducerOptions<TProducerOptions>(services, configuration, sectionName);

            services.AddSingleton<ICloudEventsProducer<TProducerOptions>, CloudEventsProducer<TProducerOptions>>();
            services.AddSingleton<ICloudEventsMappingStrategy<TProducerOptions>, TMappingStrategy>();

            return services;
        }

        public static IServiceCollection ConfigureKafkaProducerOptions<TProducerOptions>(this IServiceCollection services, IConfiguration configuration, string sectionName)
            where TProducerOptions : class, IKafkaProducerOptions
        {
            var kafkaProducerOptions = configuration.GetSection(sectionName);
            services.Configure<TProducerOptions>(kafkaProducerOptions);

            return services;
        }

    }
}
