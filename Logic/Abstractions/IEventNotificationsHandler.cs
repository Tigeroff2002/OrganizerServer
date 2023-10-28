namespace Logic.Abstractions;

public interface IEventNotificationsHandler
{
    Task HandleEventsAsync(CancellationToken token);
}
