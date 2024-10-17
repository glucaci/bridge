namespace Bridge.InMemory;

internal interface IInMemoryMessageBus : IBrokerMessageBus
{
    InMemoryQueue<InMemoryMessage> GetQueue(string queue);
}