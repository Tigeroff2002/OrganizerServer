using Contracts.Response;
using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IUserEmailConfirmer
{
    Task<Response> ConfirmAsync(ShortUserInfo shortUserInfo, CancellationToken token);
}
