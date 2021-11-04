using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Kafka.EventBus.Options;

namespace Kafka.Producer
{
    public class TenantsProducerOptions : IKafkaProducerOptions
    {
        public const string SectionName = "TenantsProducer";

        public string Topic { get; set; } = string.Empty;
        public SubjectNameStrategy SubjectNameStrategy { get; set; }
        public CompressionType CompressionType { get; set; }
    }
}
