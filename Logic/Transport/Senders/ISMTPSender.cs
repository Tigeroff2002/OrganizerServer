using Contracts.Response;
using Models.BusinessModels;

namespace Logic.Transport.Senders;

public interface ISMTPSender : INotificationsSender
{
    public Task<Response> ConfirmAsync(
        ShortUserInfo shortUserInfo,
        CancellationToken token);
}
