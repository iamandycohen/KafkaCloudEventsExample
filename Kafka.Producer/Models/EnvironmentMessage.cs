using Kafka.EventBus;
using Kafka.EventBus.CloudEvents;
using System;

namespace Kafka.Producer.Models
{
    [CloudEventsPayload(
        dataType: "com.sitecore.xmclouddeploy.example.environmentmessage",
        dataSchema: "http://sitecore.com/xmclouddeploy/example/environmentmessage.v1",
        source: "http://sitecore.com/xmcloud/example")]
    public class EnvironmentMessage : IEvent
    {
        public string Id { get; set; } = "<default>";
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
        public string Data { get; set; } = string.Empty;
    }
}
