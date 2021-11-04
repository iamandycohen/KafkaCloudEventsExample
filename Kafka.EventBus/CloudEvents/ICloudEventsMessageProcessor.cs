﻿using CloudNative.CloudEvents;
using Kafka.EventBus.Options;
using System.Threading.Tasks;

namespace Kafka.EventBus.CloudEvents
{
    public interface ICloudEventsMessageProcessor<TConsumerOptions>
        where TConsumerOptions : IKafkaConsumerOptions
    {
        Task Process(object? @event, CloudEvent originalCloudEvent);
    }
}
