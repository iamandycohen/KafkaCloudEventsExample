using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kafka.EventBus.CloudEvents
{
    public class CloudEventsPayloadTypeFactory : ICloudEventsPayloadTypeFactory
    {
        private readonly ILogger _logger;
        private readonly Lazy<List<Tuple<Type, CloudEventsPayloadAttribute>>> _cloudEventPayloadAttributeTypes;
        private Dictionary<string, Type> _cloudEventPayloadMap = new Dictionary<string, Type>();

        public CloudEventsPayloadTypeFactory(
            ILogger<CloudEventsPayloadTypeFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // TODO: move this into service registration
            _cloudEventPayloadAttributeTypes = new Lazy<List<Tuple<Type, CloudEventsPayloadAttribute>>>(() =>
            {
                var typesWithMyAttribute =
                    from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    let attributes = t.GetCustomAttributes(typeof(CloudEventsPayloadAttribute), false)
                    where attributes != null && attributes.Length > 0
                    select new Tuple<Type, CloudEventsPayloadAttribute>(t, attributes.Cast<CloudEventsPayloadAttribute>().First());

                return typesWithMyAttribute.ToList();
            });
        }

        public Type? ResolvePayloadType(CloudEvent cloudEvent)
        {
            if (cloudEvent.DataSchema == null)
            {
                throw new InvalidOperationException("CloudEvent DataSchema must be present.");
            }

            if (string.IsNullOrWhiteSpace(cloudEvent.Type))
            {
                throw new InvalidOperationException("CloudEvent Type must be present.");
            }

            var key = $"{cloudEvent.Type}{cloudEvent.DataSchema.AbsoluteUri}";

            if (!_cloudEventPayloadMap.TryGetValue(key, out Type? dataType))
            {
                dataType = _cloudEventPayloadAttributeTypes.Value
                    .FirstOrDefault(x =>
                        x.Item2.DataSchema == cloudEvent.DataSchema &&
                        x.Item2.DataType.Equals(cloudEvent.Type, StringComparison.InvariantCultureIgnoreCase))?.Item1;

                _cloudEventPayloadMap[key] = dataType!;
            }

            return dataType;
        }
    }
}
