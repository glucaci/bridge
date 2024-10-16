using System.Diagnostics;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace Bridge;

internal class OutboxProcessor : IOutboxProcessor
{
    private readonly IBrokerMessageBus _brokerMessageBus;
    private readonly IOutboxStorage _outboxStorage;
    private readonly ILogger<OutboxProcessor> _logger;
    
    public OutboxProcessor(
        IBrokerMessageBus brokerMessageBus,
        IOutboxStorage outboxStorage,
        ILogger<OutboxProcessor> logger)
    {
        _brokerMessageBus = brokerMessageBus;
        _outboxStorage = outboxStorage;
        _logger = logger;
    }
    
    public async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            Activity? activity = BridgeBusActivity.StartProcessOutbox();
            Activity? processActivity = default;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                OutboxItem? outboxItem = await _outboxStorage
                    .Delete(cancellationToken);

                if (outboxItem is not null)
                {
                    activity?.Dispose();
                    processActivity = BridgeBusActivity.StartProcessOutbox(outboxItem);

                    await _brokerMessageBus
                        .Send(outboxItem.Message, outboxItem.Queue, cancellationToken);

                    // TODO: Optional if archive is enabled
                    OutboxItem sentOutboxItem = SentOutboxItem
                        .FromOutboxItem(outboxItem, DateTime.UtcNow);

                    await _outboxStorage.Archive(sentOutboxItem, cancellationToken);
                    // TODO: ----------------------------

                    scope.Complete();
                }
            }

            processActivity?.Dispose();
            activity?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing outbox");
        }
    }
}