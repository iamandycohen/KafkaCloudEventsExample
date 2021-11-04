using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Kafka.EventBus.CloudEvents;
using Kafka.EventBus.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Kafka.EventBus.Kafka
{
    public class CloudEventsProducer<TProducerOptions> : ICloudEventsProducer<TProducerOptions>, IDisposable 
        where TProducerOptions : class, IKafkaProducerOptions
    {
        readonly ILogger _logger;
        private readonly IMemoryCache _memorycache;
        readonly KafkaConnectionOptions _connectionOptions;
        readonly IKafkaProducerOptions _producerOptions;

        private IProducer<string?, byte[]>? _producer;
        private bool disposedValue;

        private const string MachineNameToken = "@machinename";
        private const string cachedProducerCacheKeyPrefix = "CachedProducer";

        public CloudEventsProducer(
            IOptions<KafkaConnectionOptions> connectionOptions,
            IOptions<TProducerOptions> producerOptions,
            IMemoryCache memorycache,
            ILogger<CloudEventsProducer<TProducerOptions>> logger)
        {
            _connectionOptions = connectionOptions == null ? throw new ArgumentNullException(nameof(connectionOptions)) : connectionOptions.Value ?? throw new ArgumentNullException(nameof(connectionOptions));
            _producerOptions = producerOptions == null ? throw new ArgumentNullException(nameof(producerOptions)) : producerOptions.Value ?? throw new ArgumentNullException(nameof(producerOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memorycache = memorycache ?? throw new ArgumentNullException(nameof(memorycache));
        }

        public async Task PublishAsync(IEvent @event, string? key = default)
        {
            try
            {
                // get the attributes
                var cloudEventPayoloadAttribute = (CloudEventsPayloadAttribute?) (Attribute.GetCustomAttribute(@event.GetType(), typeof(CloudEventsPayloadAttribute)));

                if (cloudEventPayoloadAttribute == null)
                {
                    throw new InvalidOperationException($"CloudEvent with ID '{@event.Id}' and key '{key}' must be decorated with a {nameof(CloudEventsPayloadAttribute)} to specify the type, dataschema and source properties of the CloudEvent object");
                }

                if (cloudEventPayoloadAttribute.Source == null)
                {
                    throw new InvalidOperationException($"CloudEvent with ID '{@event.Id}' and key '{key}' must specifiy the 'source' property on the {nameof(CloudEventsPayloadAttribute)}");
                }

                var cloudEvent = new CloudEvent
                {
                    Type = cloudEventPayoloadAttribute.DataType,
                    DataSchema = cloudEventPayoloadAttribute.DataSchema,
                    Source = cloudEventPayoloadAttribute.Source,
                    Id = @event.Id,
                    Time = @event.Time,
                    Data = @event
                };

                if (key != null)
                {
                    cloudEvent[Partitioning.PartitionKeyAttribute] = key;
                }

                var message = cloudEvent.ToKafkaMessage(ContentMode.Binary, new JsonEventFormatter());

                await Producer.ProduceAsync(_producerOptions.Topic, message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        private IProducer<string?, byte[]> Producer
        {
            get
            {
                if (_producer == null)
                {
                    var cachedProducerCacheKey = $"{cachedProducerCacheKeyPrefix}-{typeof(byte[]).FullName}";
                    if (!_memorycache.TryGetValue(cachedProducerCacheKey, out IProducer<string?, byte[]> cachedProducer))
                    {
                        var producerConfiguration = new ProducerConfig
                        {
                            BootstrapServers = _connectionOptions.BootstrapServers,
                            SaslMechanism = _connectionOptions.SaslMechanism,
                            SaslUsername = _connectionOptions.SaslUsername,
                            SaslPassword = _connectionOptions.SaslPassword,
                            SecurityProtocol = _connectionOptions.SecurityProtocol,
                            ClientId = GetClientId(_connectionOptions.ClientId),
                            CompressionType = _producerOptions.CompressionType
                        };

                        cachedProducer = new ProducerBuilder<string?, byte[]>(producerConfiguration)
                                        .Build();

                        _memorycache.Set(cachedProducerCacheKey, cachedProducer);

                        _producer = cachedProducer;
                    }
                    else
                    {
                        _producer = new DependentProducerBuilder<string?, byte[]>(cachedProducer.Handle).Build();
                    }
                }

                return _producer;
            }
        }

        private static string GetClientId(string? value)
        {
            if (!(value?.Contains(MachineNameToken, StringComparison.OrdinalIgnoreCase)!).Value)
                return value;

            return value.Replace(MachineNameToken, Environment.MachineName, StringComparison.OrdinalIgnoreCase);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var cachedProducerCacheKey = $"{cachedProducerCacheKeyPrefix}-{typeof(byte[]).FullName}";
                    if (_memorycache.TryGetValue(cachedProducerCacheKey, out IProducer<string?, byte[]> cachedProducer))
                    {
                        if (cachedProducer != null)
                        {
                            cachedProducer.Flush(TimeSpan.FromSeconds(10));
                        }
                    }

                    if (_producer != null)
                    {
                        _producer.Dispose();
                    }

                    if (cachedProducer != null)
                    {
                        cachedProducer.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~KafkaProducer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
