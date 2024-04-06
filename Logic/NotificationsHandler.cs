using Logic.Abstractions;
using Microsoft.Extensions.Logging;

namespace Logic;

public sealed class NotificationsHandler
    : INotificationsHandler
{
    public NotificationsHandler(
        IEventNotificationsHandler eventNotificationsHandler,
        IAlertsNotificationsHandler alertsNotificationsHandler,
        ILogger<NotificationsHandler> logger)
    {
        _eventNotificationsHandler = eventNotificationsHandler
            ?? throw new ArgumentNullException(nameof(eventNotificationsHandler));

        _alertsNotificationsHandler = alertsNotificationsHandler
            ?? throw new ArgumentNullException(nameof(alertsNotificationsHandler));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleSystemNotifications(CancellationToken token)
    {
        using var scope = _logger.BeginScope(
            "Beginning handling notifications");

        var notificationsTasks = new List<Task>
        {
            _eventNotificationsHandler.HandleEventsAsync(token),
            _alertsNotificationsHandler.HandleAlersAsync(token)
        };

        await Task.WhenAll(
            notificationsTasks.Select(
                task => Task.Run(async () => await task)));
    }

    private readonly IEventNotificationsHandler _eventNotificationsHandler;
    private readonly IAlertsNotificationsHandler _alertsNotificationsHandler;
    private readonly ILogger<NotificationsHandler> _logger;
}
