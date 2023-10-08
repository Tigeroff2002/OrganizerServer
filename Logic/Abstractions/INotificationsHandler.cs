namespace Logic.Abstractions;

public interface INotificationsHandler
{
    Task HandleAsync(CancellationToken token);
}
