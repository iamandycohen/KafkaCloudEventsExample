using System;

namespace Kafka.EventBus.Options
{
    public interface IKafkaConsumerOptions
    {
        public string Topic { get; set; }
        public string GroupId { get; set; }
        public bool AutoCommit { get; set; }
        public bool AllowAutoCreateTopics { get; set; }
        public int MaxBatchSize { get; set; }
        public TimeSpan BatchWaitDuration { get; set; }
    }
}
