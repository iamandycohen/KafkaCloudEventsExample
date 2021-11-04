using System.Threading.Tasks;

namespace Kafka.EventBus
{
    public interface IEventProducer<TTopicKey, TEvent> 
        where TEvent : class
    {
        Task PublishAsync(TTopicKey key, TEvent @event);
    }
}
