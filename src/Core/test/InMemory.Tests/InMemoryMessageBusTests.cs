using FluentAssertions;
using Xunit;

namespace Bridge.InMemory.Tests;

public class InMemoryMessageBusTests
{
    [Fact]
    public async Task CreateTestMessage_WhenSend_ConsumerWasCalled()
    {
        // Arrange
        var host = await InMemoryBusHost.Create((q, b) => b.AddConsumer<TestConsumer, TestMessage>(q));
        var testMessage = new TestMessage();
        var inMemoryMessageBus = host.GetMessageBus<IInMemoryMessageBus>();

        // Act
        await inMemoryMessageBus.Send(testMessage, host.QueueName, default);
        await inMemoryMessageBus.WaitForConsumer(host.QueueName);

        // Assert
        host.GetConsumer<TestConsumer, TestMessage>().WasCalled.Should().BeTrue();
    }
    
    [Fact]
    public async Task CreateTestMessage_WhenScheduleInFuture_ConsumerWasNotCalled()
    {
        // Arrange
        var testMessage = new TestMessage();
        var host = await InMemoryBusHost.Create((q, b) => b.AddConsumer<TestConsumer, TestMessage>(q));
        var now = host.TimeProvider.GetUtcNow();
        var inMemoryMessageBus = host.GetMessageBus<IInMemoryMessageBus>();

        // Act
        await inMemoryMessageBus.Schedule(testMessage, host.QueueName, now.AddHours(1), default);
        
        // Assert
        var queue = inMemoryMessageBus.GetQueue(host.QueueName);
        queue.GetItems().Should().ContainSingle(i => i.EnqueueTime == now.AddHours(1));
        host.GetConsumer<TestConsumer, TestMessage>().WasCalled.Should().BeFalse();
    }
    
    [Fact]
    public async Task CreateTestMessage_WhenSchedule_ConsumerWasCalled()
    {
        // Arrange
        var testMessage = new TestMessage();
        var host = await InMemoryBusHost.Create((q, b) => b.AddConsumer<TestConsumer, TestMessage>(q));
        var now = host.TimeProvider.GetUtcNow();
        var inMemoryMessageBus = host.GetMessageBus<IInMemoryMessageBus>();

        // Act
        await inMemoryMessageBus.Schedule(testMessage, host.QueueName, now.AddHours(1), default);
        host.TimeProvider.Advance(TimeSpan.FromHours(2));
        await inMemoryMessageBus.WaitForConsumer(host.QueueName);
        
        // Assert
        host.GetConsumer<TestConsumer, TestMessage>().WasCalled.Should().BeTrue();
    }

    private record TestMessage;

    private class TestConsumer : IConsumer<TestMessage>
    {
        internal bool WasCalled { get; private set; }
        
        public ValueTask Handle(TestMessage message, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return ValueTask.CompletedTask;
        }
    }
}