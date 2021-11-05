using Kafka.EventBus.CloudEvents;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Consumer
{
    public class ExampleConsumerService : BackgroundService
    {
        private ICloudEventsConsumer<TenantsConsumerOptions> _tenantsConsumer;

        public ExampleConsumerService(ICloudEventsConsumer<TenantsConsumerOptions> tenantsConsumer)
        {
            _tenantsConsumer = tenantsConsumer ?? throw new ArgumentNullException(nameof(tenantsConsumer));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _tenantsConsumer.SubscribeAsync(stoppingToken);
        }
    }
}
