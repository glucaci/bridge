using Azure.Messaging;

namespace Bridge.InMemory;

internal record InMemoryMessage(DateTimeOffset EnqueueTime, CloudEvent CloudEvent);