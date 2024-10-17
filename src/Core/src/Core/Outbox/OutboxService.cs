using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bridge;

internal class OutboxService : BackgroundService
{
    private readonly TimeProvider _timeProvider;
    private readonly IOutboxProcessor _outboxProcessor;
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(
        TimeProvider timeProvider,
        IOutboxProcessor outboxProcessor,
        ILogger<OutboxService> logger)
    {
        _timeProvider = timeProvider;
        _outboxProcessor = outboxProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10), _timeProvider);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await _outboxProcessor.Execute(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed executing outbox");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Outbox service stopped");
        }
    }
}