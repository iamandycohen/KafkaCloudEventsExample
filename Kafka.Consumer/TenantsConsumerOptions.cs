using Kafka.EventBus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Consumer
{
    public class TenantsConsumerOptions : IKafkaConsumerOptions
    {
        public const string SectionName = "TenantsEnvironmentConsumer";

        public string Topic { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public bool AutoCommit { get; set; }
        public bool AllowAutoCreateTopics { get; set; }
    }
}
