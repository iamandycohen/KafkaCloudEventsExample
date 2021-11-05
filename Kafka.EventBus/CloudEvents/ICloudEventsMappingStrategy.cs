using CloudNative.CloudEvents;
using Confluent.Kafka;
using Kafka.EventBus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public interface ICloudEventsMappingStrategy<TProducerOptions>
        where TProducerOptions : IKafkaProducerOptions
    {
        Task<Message<string?, byte[]>> Map(IEvent @event, string? key = default);
    }
}
