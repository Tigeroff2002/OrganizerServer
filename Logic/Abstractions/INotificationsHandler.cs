namespace Logic.Abstractions;

public interface INotificationsHandler
{
    Task HandleSystemNotifications(CancellationToken token);
}
