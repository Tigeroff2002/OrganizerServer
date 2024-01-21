using Models.UserActionModels;

namespace Logic.Transport.Senders;

public interface INotificationsSender
{
    Task SendNotificationAsync(UserReminderInfo model, CancellationToken token);
}
