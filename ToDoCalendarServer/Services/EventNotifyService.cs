using Logic.Abstractions;

namespace ToDoCalendarServer.Services;

public class EventNotifyService : BackgroundService
{
    public EventNotifyService(
        ILogger<EventNotifyService> logger,
        IEventNotificationsHandler notifiesHandler)
    {
        _notifiesHandler = notifiesHandler 
            ?? throw new ArgumentNullException(nameof(notifiesHandler));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Event notify service has been created");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _logger.BeginScope(
            "Beginning handling events notifications");

        return _notifiesHandler.HandleEventsAsync(stoppingToken);
    }

    private readonly ILogger<EventNotifyService> _logger;
    private readonly IEventNotificationsHandler _notifiesHandler;
}
