using Logic.Abstractions;

namespace ToDoCalendarServer.Services;

public class NotifyService : BackgroundService
{
    public NotifyService(
        ILogger<NotifyService> logger,
        INotificationsHandler notifiesHandler)
    {
        _notifiesHandler = notifiesHandler 
            ?? throw new ArgumentNullException(nameof(notifiesHandler));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(
            async () => 
                await _notifiesHandler
                .HandleAsync(stoppingToken)
                .ConfigureAwait(false));

    private readonly ILogger<NotifyService> _logger;
    private readonly INotificationsHandler _notifiesHandler;
}
