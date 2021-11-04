using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Kafka.EventBus.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public abstract class CloudEventsMessageProcessor<TConsumerOptions> : ICloudEventsMessageProcessor<TConsumerOptions>
        where TConsumerOptions : IKafkaConsumerOptions
    {
        private readonly ILogger _logger;
        private readonly ICloudEventsPayloadTypeFactory _cloudEventPayloadTypeFactory;

        public CloudEventsMessageProcessor(
            ICloudEventsPayloadTypeFactory cloudEventPayloadTypeFactory,
            ILogger logger)
        {
            _cloudEventPayloadTypeFactory = cloudEventPayloadTypeFactory ?? throw new ArgumentNullException(nameof(cloudEventPayloadTypeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessCloudEvent(CloudEvent cloudEvent)
        {
            // get event type
            var cloudEventType = _cloudEventPayloadTypeFactory.ResolvePayloadType(cloudEvent);

            if (cloudEventType == null)
            {
                _logger.LogWarning($"Unable to resolve CloudEvent payload of type '{cloudEvent.Type}' and schema '{cloudEvent.DataSchema}'. ID: '{cloudEvent.Id}' KEY: '{cloudEvent[Partitioning.PartitionKeyAttribute]}'.");

                return;
            }

            // deserialize
            var json = ((JsonElement)cloudEvent.Data!).GetRawText();
            var eventPayload = JsonSerializer.Deserialize(json, cloudEventType);

            await Process(eventPayload, cloudEvent).ConfigureAwait(false);
        }

        public abstract Task Process(object? @event, CloudEvent originalCloudEvent);
    }
}
