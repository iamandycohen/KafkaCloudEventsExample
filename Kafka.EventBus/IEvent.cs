using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.EventBus
{
    public interface IEvent
    {
        string Id { get; set; }
        DateTimeOffset Time { get; set; }
    }
}
