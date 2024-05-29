using Contracts.Request;
using Models.BusinessModels;
using Models.UserActionModels;

namespace Logic.Abstractions;

public interface IUsersDataHandler
{
    public Task<PreRegistrationResponse> TryRegisterUser(
        string registrationData,
        CancellationToken token);

    public Task<RegistrationResponse> ConfirmUser(
        string confirmationData,
        CancellationToken token);

    public Task<Response> CheckIfTimeExpired(
        string requestData,
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

    public Task<GetResponse> GetAllUsers(
        string getAllUsersDTO,
        CancellationToken token);

    public Task<GetResponse> GetAllUsersNotInGroup(
        string getAllUsersNotInGroupDTO,
        CancellationToken token);

    public Task<GetResponse> GetAdmins(
        string getAdminsDTO,
        CancellationToken token);

    public Task<GetResponse> GetUsersFromGroup(
        string getGroupUsersDTO,
        CancellationToken token);

    public Task<GetResponse> GetGroupUsersNotInEvent(
        string getgroupUsersNotInEventDTO,
        CancellationToken token);
}
