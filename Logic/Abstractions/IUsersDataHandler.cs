using Models;
using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IUsersDataHandler
{
    public Task<bool> TryRegisterUser(
        UserRegistrationData registrationData,
        CancellationToken token);

    public Task<bool> TryLoginUser(
        UserLoginData loginData,
        CancellationToken token);

    public Task<User?> GetUserInfo(UserInfoById userInfoById, CancellationToken token);
}
