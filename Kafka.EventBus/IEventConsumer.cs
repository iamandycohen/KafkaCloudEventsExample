using Confluent.Kafka;
using Kafka.EventBus.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.EventBus
{
    public interface IEventConsumer<TTopicKey, TEvent>
        where TEvent : class, /*IMessage<TEvent>,*/ new()
    {
        Task SubscribeAsync(Func<ConsumeResult<TTopicKey, TEvent>, Task> processEvent, CancellationToken cancellationToken);
    }
}
