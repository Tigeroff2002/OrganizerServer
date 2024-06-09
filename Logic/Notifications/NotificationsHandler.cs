using Logic.Abstractions;
using Microsoft.Extensions.Logging;

namespace Logic.Notifications;

public sealed class NotificationsHandler
    : INotificationsHandler
{
    public NotificationsHandler(
        IEventNotificationsHandler eventNotificationsHandler,
        IAlertsNotificationsHandler alertsNotificationsHandler,
        IExceptionHandler exceptionHandler,
        ILogger<NotificationsHandler> logger)
    {
        _eventNotificationsHandler = eventNotificationsHandler
            ?? throw new ArgumentNullException(nameof(eventNotificationsHandler));

        _alertsNotificationsHandler = alertsNotificationsHandler
            ?? throw new ArgumentNullException(nameof(alertsNotificationsHandler));

        _exceptionHandler = exceptionHandler 
            ?? throw new ArgumentNullException(nameof(exceptionHandler));

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

        try
        {
            await Task.WhenAll(
                notificationsTasks.Select(
                    task => Task.Run(async () => await task)));
        }
        catch (Exception ex) 
        {
            await _exceptionHandler.HandleExceptionsAsync(ex, token);

            throw;
        }
    }

    private readonly IEventNotificationsHandler _eventNotificationsHandler;
    private readonly IAlertsNotificationsHandler _alertsNotificationsHandler;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly ILogger<NotificationsHandler> _logger;
}
