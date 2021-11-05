using CloudNative.CloudEvents;

namespace Kafka.EventBus.CloudEvents
{
    public interface ICloudEventData
    {
        object? Event { get; set; }
        CloudEvent OriginalCloudEvent { get; set; }
    }
}