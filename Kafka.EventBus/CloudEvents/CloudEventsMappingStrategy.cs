using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Kafka.EventBus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public class CloudEventsMappingStrategy<TProducerOptions> : ICloudEventsMappingStrategy<TProducerOptions>
        where TProducerOptions : IKafkaProducerOptions
    {
        public Task<Message<string?, byte[]>> Map(IEvent @event, string? key = null)
        {
            // get the attributes
            var cloudEventPayoloadAttribute = (CloudEventsPayloadAttribute?)(Attribute.GetCustomAttribute(@event.GetType(), typeof(CloudEventsPayloadAttribute)));

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

            return Task.FromResult(message);
        }
    }
}
