using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.EventBus.Extensions
{
    public static class ConsumerExtensions
    {
        public static IReadOnlyCollection<ConsumeResult<TKey?, TValue>> ConsumeBatch<TKey, TValue>(this IConsumer<TKey, TValue> consumer, TimeSpan consumeTimeout, int maxBatchSize)
        {
            var message = consumer.Consume(consumeTimeout);

            if (message?.Message is null)
                return Array.Empty<ConsumeResult<TKey?, TValue>>();

            var messageBatch = new List<ConsumeResult<TKey?, TValue>> { message! };

            while (messageBatch.Count < maxBatchSize)
            {
                message = consumer.Consume(consumeTimeout);
                if (message?.Message is null)
                    break;

                messageBatch.Add(message!);
            }

            return messageBatch;
        }
    }
}
