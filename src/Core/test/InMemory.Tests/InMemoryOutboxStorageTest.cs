using FluentAssertions;
using Xunit;

namespace Bridge.InMemory.Tests;

public class InMemoryOutboxStorageTest
{
    [Fact]
    public async Task Add_Should_AddItemToPendingItems()
    {
        // Arrange
        var storage = new InMemoryOutboxStorage();

        // Act
        await storage.Add(OutboxItem, CancellationToken.None);

        // Assert
        storage.PendingItems.Should().ContainSingle(i => OutboxItem == i);
    }
    
    [Fact]
    public async Task Delete_Should_ReturnNull_WhenQueueIsEmpty()
    {
        // Arrange
        var storage = new InMemoryOutboxStorage();

        // Act
        var deletedItem = await storage.Delete(CancellationToken.None);

        // Assert
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task Delete_Should_ReturnItem_WhenQueueIsNotEmpty()
    {
        // Arrange
        var storage = new InMemoryOutboxStorage();
        storage.PendingItems.Enqueue(OutboxItem);

        // Act
        var deletedItem = await storage.Delete(CancellationToken.None);

        // Assert
        deletedItem.Should().BeEquivalentTo(OutboxItem);
    }
    
    [Fact]
    public async Task Archive_Should_StoreItemInArchive()
    {
        // Arrange
        var storage = new InMemoryOutboxStorage();

        // Act
        await storage.Archive(OutboxItem, CancellationToken.None);

        // Assert
        storage.ArchivedItems.Values.Should().ContainSingle(i => OutboxItem == i);
    }

    private OutboxItem OutboxItem => new(
        "test", nameof(OutboxItem), "test", ReadOnlyMemory<byte>.Empty, DateTime.MinValue, null);
}