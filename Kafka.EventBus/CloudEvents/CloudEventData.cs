using CloudNative.CloudEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public class CloudEventData : ICloudEventData
    {
        public CloudEventData(CloudEvent originalCloudEvent, object? @event)
        {
            OriginalCloudEvent = originalCloudEvent;
            Event = @event;
        }

        public CloudEvent OriginalCloudEvent { get; set; }
        public object? Event { get; set; }
    }
}
