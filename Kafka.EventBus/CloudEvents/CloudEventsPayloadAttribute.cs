using System;

namespace Kafka.EventBus.CloudEvents
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CloudEventsPayloadAttribute : Attribute
    {
        public CloudEventsPayloadAttribute(string dataType, string dataSchema, string? source = null)
        {
            DataType = dataType;
            DataSchema = new Uri(dataSchema);
            if (source != null) 
            {
                Source = new Uri(source);
            }
        }

        public string DataType { get; }
        public Uri DataSchema { get; }
        public Uri? Source { get; }
    }
}
