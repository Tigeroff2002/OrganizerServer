using Contracts.Response;
using Models.BusinessModels;
using Models.UserActionModels.NotificationModels;

namespace Logic.Transport.Senders;

public interface ISMTPSender : INotificationsSender
{
    public Task<Response> ConfirmAsync(
        ShortUserInfo shortUserInfo,
        CancellationToken token);

    Task SendSMTPNotificationAsync(
        UserEmailReminderInfo model,
        CancellationToken token);

    Task SendNotificationAsync(
        UserEmailNotificationInfo notification, 
        CancellationToken token);
}
