using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Kafka.EventBus.CloudEvents;
using Kafka.EventBus.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
            await Task.Run((Func<Task?>)(async () =>
            {
                var consumer = ConsumerBuilder();
                consumer.Subscribe(_consumerOptions.Topic);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumerResult = consumer.Consume(cancellationToken);
                        var cloudEvent = consumerResult.Message.ToCloudEvent(new JsonEventFormatter());

                        await _cloudEventMessageProcessor.ProcessCloudEvent(cloudEvent).ConfigureAwait(false);

                        if (!_consumerOptions.AutoCommit)
                            consumer.Commit(consumerResult);
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
            }), cancellationToken);
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
