using Contracts.Response;
using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IUsersCodeConfirmer
{
    Task<Response> ConfirmAsync(ShortUserInfo shortUserInfo, CancellationToken token);
}
