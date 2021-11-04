using Kafka.EventBus.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public interface ICloudEventsConsumer<TConsumerOptions>
        where TConsumerOptions : IKafkaConsumerOptions
    {
        Task SubscribeAsync(CancellationToken cancellationToken);
    }
}
