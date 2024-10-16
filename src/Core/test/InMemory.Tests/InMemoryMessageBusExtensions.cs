namespace Bridge.InMemory.Tests;

internal static class InMemoryMessageBusExtensions
{
    internal static Task WaitForConsumer(
        this IInMemoryMessageBus messageBus, 
        string queueName)
    {
        var queue = messageBus.GetQueue(queueName);
        queue.Close();

        return queue.Waiter;
    }
}