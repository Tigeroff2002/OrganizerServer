using Contracts.Request;
using Models.BusinessModels;
using Models.UserActionModels;

namespace Logic.Abstractions;

public interface IUsersDataHandler
{
    public Task<Response> TryRegisterUser(
        string registrationData,
        CancellationToken token);

    public Task<Response> TryLoginUser(
        string loginData,
        CancellationToken token);

    public Task<Response> TryLogoutUser(
        string logoutData,
        CancellationToken token);

    public Task<GetResponse> GetUserInfo(
        string userInfoById,
        CancellationToken token);

    public Task<Response> UpdateUserInfo(
        string userUpdateInfo,
        CancellationToken token);

    public Task<Response> UpdateUserRoleAsync(
        string userUpdateRoleDTO,
        CancellationToken token);
}
