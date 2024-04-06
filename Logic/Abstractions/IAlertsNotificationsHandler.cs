namespace Logic.Abstractions;

public interface IAlertsNotificationsHandler
{
    Task HandleAlersAsync(CancellationToken token);
}
