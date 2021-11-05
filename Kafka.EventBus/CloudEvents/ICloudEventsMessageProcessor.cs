using CloudNative.CloudEvents;
using Kafka.EventBus.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public interface ICloudEventsMessageProcessor<TConsumerOptions>
        where TConsumerOptions : IKafkaConsumerOptions
    {
        Task ProcessBatch(IReadOnlyCollection<ICloudEventData> cloudEventsDataBatch);
    }
}
