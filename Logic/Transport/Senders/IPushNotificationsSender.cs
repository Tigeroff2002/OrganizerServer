using Models.UserActionModels.NotificationModels;

namespace Logic.Transport.Senders;

public interface IPushNotificationsSender
    : INotificationsSender
{
    Task SendAdsPushNotificationAsync(
        UserAdsPushContentInfo model,
        CancellationToken token);
}
