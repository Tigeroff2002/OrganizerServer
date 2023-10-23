using Contracts.Request;
using Models;
using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IUsersDataHandler
{
    public Task<Response> TryRegisterUser(
        UserRegistrationData registrationData,
        CancellationToken token);

    public Task<Response> TryLoginUser(
        UserLoginData loginData,
        CancellationToken token);

    public Task<GetResponse> GetUserInfo(
        UserInfoById userInfoById,
        CancellationToken token);

    public Task<GetResponse> UpdateUserInfo(
        UserUpdateInfoDTO userUpdateInfo,
        CancellationToken token);
}
