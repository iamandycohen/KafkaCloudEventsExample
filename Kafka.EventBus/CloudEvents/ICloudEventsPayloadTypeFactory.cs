using CloudNative.CloudEvents;
using System;

namespace Kafka.EventBus.CloudEvents
{
    public interface ICloudEventsPayloadTypeFactory
    {
        Type? ResolvePayloadType(CloudEvent cloudEvent);
    }
}
