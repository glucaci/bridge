using System.Diagnostics;

namespace Bridge;

internal static class BridgeBusActivity
{
    private static readonly ActivitySource ReceiverSource = new("Bridge.Bus.Receiver");

    private static readonly string _outboxProcessActivityName = "Outbox process";
    internal static Activity? StartProcessOutbox()
    {
        return StartNewActivity(name: _outboxProcessActivityName, kind: ActivityKind.Internal);
    }

    internal static Activity? StartProcessOutbox(OutboxItem outboxItem)
    {
        var name = $"{_outboxProcessActivityName} {outboxItem.MessageType}";
        Activity? activity = CreateNewActivity(name: name, kind: ActivityKind.Internal);

        if (!string.IsNullOrEmpty(outboxItem.ActivityId) &&
            ActivityContext.TryParse(outboxItem.ActivityId, null, out ActivityContext context))
        {
            activity?.SetParentId(context.TraceId, context.SpanId, context.TraceFlags);
            activity?.Start();
        }

        return activity;
    }

    internal static Activity? StartSendOutbox(string scope, OutboxItem outboxItem)
    {
        var name = $"Outbox {scope} {outboxItem.MessageType}";

        if (Activity.Current is not null)
        {
            return ReceiverSource.StartActivity(name: name, kind: ActivityKind.Producer);
        }

        if (!string.IsNullOrEmpty(outboxItem.ActivityId) &&
            ActivityContext.TryParse(outboxItem.ActivityId, null, out ActivityContext context))
        {
            return ReceiverSource.StartActivity(name: name, kind: ActivityKind.Producer, context);
        }

        return StartNewActivity(name, ActivityKind.Consumer);
    }

    internal static Activity? StartNewActivity(string name, ActivityKind kind)
    {
        Activity.Current = null;
        return ReceiverSource.StartActivity(name: name, kind: kind);
    }

    internal static Activity? CreateNewActivity(string name, ActivityKind kind)
    {
        Activity.Current = null;
        return ReceiverSource.CreateActivity(name: name, kind: kind);
    }
}
