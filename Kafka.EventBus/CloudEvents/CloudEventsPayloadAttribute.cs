using System;

namespace Kafka.EventBus.CloudEvents
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CloudEventsPayloadAttribute : Attribute
    {
        public CloudEventsPayloadAttribute(string dataType, string dataSchema, string source)
        {
            DataType = dataType;
            DataSchema = new Uri(dataSchema);
            Source = new Uri(source);
        }

        public string DataType { get; }
        public Uri DataSchema { get; }
        public Uri Source { get; }
    }
}
