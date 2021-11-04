using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace Kafka.EventBus.Options
{
    public interface IKafkaProducerOptions
    {
        public string Topic { get; set; }
        public SubjectNameStrategy SubjectNameStrategy { get; set; }
        public CompressionType CompressionType { get; set; }
    }
}
