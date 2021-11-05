using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Kafka.EventBus.CloudEvents;
using Kafka.EventBus.Extensions;
using Kafka.EventBus.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.EventBus.Kafka
{
    public class CloudEventsConsumer<TConsumerOptions> : ICloudEventsConsumer<TConsumerOptions>
        where TConsumerOptions : class, IKafkaConsumerOptions
    {
        private const string MachineNameToken = "@machinename";

        private readonly ILogger _logger;
        private readonly KafkaConnectionOptions _connectionOptions;
        private readonly IKafkaConsumerOptions _consumerOptions;
        private readonly CloudEventsMessageProcessor<TConsumerOptions> _cloudEventMessageProcessor;

        public CloudEventsConsumer(
            IOptions<KafkaConnectionOptions> connectionOptions,
            IOptions<TConsumerOptions> consumerOptions,
            CloudEventsMessageProcessor<TConsumerOptions> cloudEventMessageProcessor,
            ILogger<CloudEventsConsumer<TConsumerOptions>> logger)
        {
            _connectionOptions = connectionOptions == null ? throw new ArgumentNullException(nameof(connectionOptions)) : connectionOptions.Value ?? throw new ArgumentNullException(nameof(connectionOptions));
            _consumerOptions = consumerOptions == null ? throw new ArgumentNullException(nameof(consumerOptions)) : consumerOptions.Value ?? throw new ArgumentNullException(nameof(consumerOptions));
            _cloudEventMessageProcessor = cloudEventMessageProcessor ?? throw new ArgumentNullException(nameof(cloudEventMessageProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                var consumer = ConsumerBuilder();
                consumer.Subscribe(_consumerOptions.Topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumerBatch = consumer.ConsumeBatch(_consumerOptions.BatchWaitDuration, _consumerOptions.MaxBatchSize);

                        var cloudEventsBatch = consumerBatch
                            .Select(result => new
                            {
                                CloudEvent = result.Message.ToCloudEvent(new JsonEventFormatter()),
                                Result = result
                            });

                        if (consumerBatch.Count > 0)
                        {
                            await _cloudEventMessageProcessor.ProcessBatch(cloudEventsBatch.Select(e => e.CloudEvent).ToList()).ConfigureAwait(false);
                        }

                        var lastConsumeResult = consumerBatch.LastOrDefault();
                        if (lastConsumeResult != null)
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                var messages = string.Join(Environment.NewLine, cloudEventsBatch
                                    .Select(evt =>
                                    {
                                        var eventType = evt.CloudEvent.Type;
                                        var identifier = evt.CloudEvent.Id;

                                        return $"\t{eventType} {identifier} (TPO: {evt.Result.TopicPartitionOffset})";
                                    }));

                                _logger.LogDebug($"{Environment.NewLine}{messages}");
                            }
                            
                            // TODO: handle cases when exceptions occur
                            // what do we do with the batch?
                            // as well as reporting the errors in processing
                            if (!_consumerOptions.AutoCommit)
                            {
                                _logger.LogInformation($"Committing batch of {consumerBatch.Count} events");

                                consumer.Commit(lastConsumeResult);
                            }
                        }
                    }
                    catch (OperationCanceledException e)
                    {
                        _logger.LogInformation(e.Message);
                        consumer.Close();
                        break;
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError(e, e.Message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Unexpected error: {e.Message}");
                        consumer.Close();
                        break;
                    }
                }
            }, cancellationToken);
        }

        private IConsumer<string?, byte[]> ConsumerBuilder()
        {
            ConsumerConfig consumerConfiguration = new()
            {
                BootstrapServers = _connectionOptions.BootstrapServers,
                GroupId = GetConsumerGroup(_consumerOptions.GroupId),
                AllowAutoCreateTopics = _consumerOptions.AllowAutoCreateTopics,
                EnableAutoCommit = _consumerOptions.AutoCommit,
                SecurityProtocol = _connectionOptions.SecurityProtocol,
                SaslMechanism = _connectionOptions.SaslMechanism,
                SaslUsername = _connectionOptions.SaslUsername,
                SaslPassword = _connectionOptions.SaslPassword,
                SocketKeepaliveEnable = _connectionOptions.SocketKeepaliveEnable,
                MetadataMaxAgeMs = (int)_connectionOptions.MetadataMaxAge.TotalMilliseconds,
                ClientId = GetClientId(_connectionOptions.ClientId),
                StatisticsIntervalMs = (int)_connectionOptions.StatisticsInterval.TotalMilliseconds,
                HeartbeatIntervalMs = (int)_connectionOptions.HeartbeatInterval.TotalMilliseconds,
                SessionTimeoutMs = (int)_connectionOptions.SessionTimeout.TotalMilliseconds
            };

            return new ConsumerBuilder<string?, byte[]>(consumerConfiguration)
                                .Build();
        }

        private static string GetConsumerGroup(string? value)
        {
            var consumerGroup = string.IsNullOrWhiteSpace(value) || value.Equals("automatic", StringComparison.OrdinalIgnoreCase) ? Environment.MachineName : value;

            if (!(consumerGroup?.Contains(MachineNameToken, StringComparison.OrdinalIgnoreCase)!).Value)
                return consumerGroup;

            return consumerGroup.Replace(MachineNameToken, Environment.MachineName, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetClientId(string? value)
        {
            if (!(value?.Contains(MachineNameToken, StringComparison.OrdinalIgnoreCase)!).Value)
                return value;

            return value.Replace(MachineNameToken, Environment.MachineName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
