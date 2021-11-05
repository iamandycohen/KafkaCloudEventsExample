using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Confluent.Kafka;
using Kafka.EventBus.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task ProcessBatch(IReadOnlyCollection<CloudEvent> cloudEventsBatch)
        {
            var cloudEventsDataBatch = new List<ICloudEventData>();

            foreach (var cloudEvent in cloudEventsBatch)
            {
                // get event type
                var cloudEventType = _cloudEventPayloadTypeFactory.ResolvePayloadType(cloudEvent);

                if (cloudEventType == null)
                {
                    _logger.LogWarning($"Unable to resolve CloudEvent payload of type '{cloudEvent.Type}' and schema '{cloudEvent.DataSchema}'. ID: '{cloudEvent.Id}' KEY: '{cloudEvent[Partitioning.PartitionKeyAttribute]}'.");
                } else
                {
                    // deserialize
                    var json = ((JsonElement)cloudEvent.Data!).GetRawText();
                    var eventPayload = JsonSerializer.Deserialize(json, cloudEventType);

                    var cloudEventData = new CloudEventData(cloudEvent, eventPayload);
                    cloudEventsDataBatch.Add(cloudEventData);
                }
            }

            await ProcessBatch(cloudEventsDataBatch).ConfigureAwait(false);
        }

        public abstract Task ProcessBatch(IReadOnlyCollection<ICloudEventData> cloudEventsDataBatch);
    }
}
