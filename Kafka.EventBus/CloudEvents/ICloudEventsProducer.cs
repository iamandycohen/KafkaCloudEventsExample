using Kafka.EventBus.Options;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public interface ICloudEventsProducer<TProducerOptions>
        where TProducerOptions : IKafkaProducerOptions
    {
        Task PublishAsync(IEvent @event, string? key = default);
    }
}
