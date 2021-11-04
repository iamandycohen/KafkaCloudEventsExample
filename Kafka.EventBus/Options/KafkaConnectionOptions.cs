using Confluent.Kafka;
using System;

namespace Kafka.EventBus.Options
{
    public class KafkaConnectionOptions
    {
        public const string SectionName = "Kafka";

        public string BootstrapServers { get; set; } = string.Empty;
        public string SchemaRegistryServer { get; set; } = string.Empty;
        public string SchemaRegistryCredentials { get; set; } = string.Empty;
        public SecurityProtocol SecurityProtocol { get; set; }
        public SaslMechanism SaslMechanism { get; set; }
        public string SaslUsername { get; set; } = string.Empty;
        public string SaslPassword { get; set; } = string.Empty;
        public bool SocketKeepaliveEnable { get; set; }
        public TimeSpan MetadataMaxAge { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public TimeSpan StatisticsInterval { get; set; }
        public TimeSpan HeartbeatInterval { get; set; }
        public TimeSpan SessionTimeout { get; set; }
    }
}
