namespace Bridge;

public record OutboxItem(
    string Id,
    string MessageType,
    string Queue,
    ReadOnlyMemory<byte> Message,
    DateTime CreatedAt,
    string? ActivityId);

public record SentOutboxItem(
    string Id,
    string MessageType,
    string Queue,
    ReadOnlyMemory<byte> Message,
    DateTime CreatedAt,
    DateTime SentAt,
    string? ActivityId)
    : OutboxItem(Id, MessageType, Queue, Message, CreatedAt, ActivityId)
{
    internal static SentOutboxItem FromOutboxItem(OutboxItem outboxItem, DateTime sentAt)
    {
        return new SentOutboxItem(
            outboxItem.Id,
            outboxItem.MessageType,
            outboxItem.Queue,
            outboxItem.Message,
            outboxItem.CreatedAt,
            sentAt,
            outboxItem.ActivityId);
    }
}