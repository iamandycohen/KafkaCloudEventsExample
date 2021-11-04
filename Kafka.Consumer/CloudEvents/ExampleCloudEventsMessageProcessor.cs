using CloudNative.CloudEvents;
using Kafka.Consumer.Models;
using Kafka.EventBus.CloudEvents;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using Tenants.Options;

namespace Kafka.Consumer.CloudEvents
{
    public class ExampleCloudEventsMessageProcessor : CloudEventsMessageProcessor<TenantsConsumerOptions>
    {
        private readonly ILogger _logger;

        public ExampleCloudEventsMessageProcessor(ICloudEventsPayloadTypeFactory cloudEventPayloadTypeFactory, ILogger<ExampleCloudEventsMessageProcessor> logger) : base(cloudEventPayloadTypeFactory, logger)
        {
            _logger = logger;
        }

        public override async Task Process(object? @event, CloudEvent originalCloudEvent)
        {
            switch (@event)
            {
                case AnotherMessage anotherMessage:
                    await HandleAnotherMessage(anotherMessage).ConfigureAwait(false);
                    break;
                case EnvironmentMessage environmentMessage:
                    await HandleEnvironmentMessage(environmentMessage).ConfigureAwait(false);
                    break;
                default:
                    break;
            }
        }

        private Task HandleEnvironmentMessage(EnvironmentMessage? @event)
        {
            _logger.LogInformation(JsonSerializer.Serialize(@event));

            return Task.CompletedTask;
        }

        private Task HandleAnotherMessage(AnotherMessage? @event)
        {
            _logger.LogInformation(JsonSerializer.Serialize(@event));

            return Task.CompletedTask;
        }
    }
}
