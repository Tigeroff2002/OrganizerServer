using Models.UserActionModels;

namespace Logic.Transport.Senders;

public interface IPushNotificationsSender
    : INotificationsSender
{
    Task SendAdsPushNotificationAsync(
        UserAdsPushReminderInfo model,
        CancellationToken token);
}
