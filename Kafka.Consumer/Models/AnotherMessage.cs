using Kafka.EventBus;
using Kafka.EventBus.CloudEvents;
using System;

namespace Kafka.Consumer.Models
{
    [CloudEventsPayload(
        dataType: "com.sitecore.xmclouddeploy.example.anothermessage",
        dataSchema: "http://sitecore.com/xmclouddeploy/example/anothermessage.v1",
        source: "http://sitecore.com/xmcloud/example")]
    public class AnotherMessage : IEvent
    {
        public string Id { get; set; } = "<default>";
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
        public string AnotherOne { get; set; } = string.Empty;
    }
}
