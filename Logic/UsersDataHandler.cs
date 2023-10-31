using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Logic.Transport;
using Logic.Transport.Abstractions;
using Microsoft.Extensions.Logging;
using Models;
using Models.BusinessModels;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace Logic;

public sealed class UsersDataHandler
    : IUsersDataHandler
{
    public UsersDataHandler(
        IUsersRepository usersRepository, 
        IUsersCodeConfirmer usersCodeConfirmer,
        ISerializer<User> userInfoSerializer,
        ILogger<UsersDataHandler> logger)
    {
        _usersRepository = usersRepository 
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _usersCodeConfirmer = usersCodeConfirmer
            ?? throw new ArgumentNullException(nameof(usersCodeConfirmer));

        _userInfoSerializer = userInfoSerializer
            ?? throw new ArgumentNullException(nameof(userInfoSerializer));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Response> TryRegisterUser(
        UserRegistrationData registrationData,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(registrationData);

        token.ThrowIfCancellationRequested();

        var email = registrationData.Email;

        var existedUser = 
           await _usersRepository
                .GetUserByEmailAsync(email, token);

        if (existedUser != null)
        {
            _logger.LogInformation(
                "User with email {Email} was already in DB",
                email);

            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"User with email {email} was already in DB";

            return await Task.FromResult(response1);
        }

        var shortUserInfo = new ShortUserInfo
        {
            UserEmail = email,
            UserName = registrationData.UserName,
            UserPhone = registrationData.PhoneNumber
        };

        var confirmResponse = 
            await _usersCodeConfirmer.ConfirmAsync(shortUserInfo, token);

        var builder = new StringBuilder();
        builder.Append(confirmResponse.OutInfo);
        builder.Append(".\n");

        var confirmResult = confirmResponse.Result;

        if (!confirmResult)
        {
            _logger.LogInformation(
                "Wrong user code confirmation received");

            var response2 = new Response();
            response2.Result = false;

            builder.Append($"Wrong user code confirmation received from user with email {email}.");
            response2.OutInfo = builder.ToString();

            return await Task.FromResult(response2);
        }

        var user = new User();
        user.Email = email;
        user.UserName = registrationData.UserName;
        user.Password = registrationData.Password;
        user.PhoneNumber = registrationData.PhoneNumber;
        user.GroupingMaps = new List<GroupingUsersMap>();
        user.TasksForImplementation = new List<UserTask>();
        user.EventMaps = new List<EventsUsersMap>();
        user.Reports = new List<Report>();

        string authToken = GenerateNewAuthToken();

        user.AuthToken = authToken;

        _logger.LogInformation("Registrating new user {Email}", user.Email);

        await _usersRepository.AddAsync(user, token);

        _usersRepository.SaveChanges();

        var response = new Response();
        response.Result = true;

        builder.Append(
            $"Registrating new user {user.Email} with id {user.Id}" +
            $" with creating new auth token {authToken}");

        response.OutInfo = builder.ToString();

        return await Task.FromResult(response);
    }

    public async Task<Response> TryLoginUser(
        UserLoginData loginData,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(loginData);

        token.ThrowIfCancellationRequested();

        var email = loginData.Email;

        var existedUser =
            await _usersRepository
                .GetUserByEmailAsync(email, token);

        if (existedUser == null)
        {
            _logger.LogInformation(
                "User with email {Email} was not already in DB",
                email);

            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"User with email {email} was not already in DB";

            return await Task.FromResult(response1);
        }

        if (existedUser.Password != loginData.Password)
        {
            _logger.LogInformation("Password not equals");

            var response2 = new Response();
            response2.Result = false;
            response2.OutInfo = "Password not equals";

            return await Task.FromResult(response2);
        }

        var authToken = GenerateNewAuthToken();

        existedUser.AuthToken = authToken;

        await _usersRepository.UpdateAsync(existedUser, token);

        _usersRepository.SaveChanges();

        var userName = existedUser.UserName;

        var response = new Response();
        response.Result = true;
        response.OutInfo = $"Login existed user {userName} with new auth token {authToken}";

        return await Task.FromResult(response);
    }

    public async Task<GetResponse> GetUserInfo(UserInfoById userInfoById, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(userInfoById);

        token.ThrowIfCancellationRequested();

        var user = 
            await _usersRepository.GetUserByIdAsync(userInfoById.UserId, token);

        if (user == null)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo = $"No such user with id {userInfoById.UserId} in system";

            return await Task.FromResult(response1);
        }

        var response = new GetResponse();
        response.Result = true;
        response.OutInfo = $"Info about user with with id {userInfoById.UserId} has been received";
        response.RequestedInfo = _userInfoSerializer.Serialize(user);

        return response;
    }

    public async Task<GetResponse> UpdateUserInfo(
        UserUpdateInfoDTO userUpdateInfo, 
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(userUpdateInfo);

        var userId = userUpdateInfo.UserId;

        var existedUser = await _usersRepository.GetUserByIdAsync(userId, token);

        if (existedUser == null) 
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo = $"No such user with id {userId} in system";

            return await Task.FromResult(response1);
        }

        if (existedUser != null)
        {
            if (!string.IsNullOrWhiteSpace(userUpdateInfo.UserName))
            {
                existedUser.UserName = userUpdateInfo.UserName;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateInfo.Email))
            {
                existedUser.Email = userUpdateInfo.Email;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateInfo.Password))
            {
                existedUser.Password = userUpdateInfo.Password;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateInfo.PhoneNumber))
            {
                existedUser.PhoneNumber = userUpdateInfo.PhoneNumber;
            }

            string authToken = GenerateNewAuthToken();

            existedUser.AuthToken = authToken;

            await _usersRepository.UpdateAsync(existedUser, token);

            _usersRepository.SaveChanges();

            var response1 = new GetResponse();
            response1.Result = true;
            response1.OutInfo = $"Updating info existed user with id = {userId}" +
                $" and email {existedUser.Email}" +
                $" with new auth token {authToken}";

            var shortUserInfo = new ShortUserInfo
            {
                UserEmail = existedUser.Email,
                UserName = existedUser.UserName,
                UserPhone = existedUser.PhoneNumber
            };

            response1.RequestedInfo = shortUserInfo;

            return await Task.FromResult(response1);
        }

        var response = new GetResponse();
        response.Result = true;
        response.OutInfo = $"No such existed user with id {userId}";

        return await Task.FromResult(response);
    }

    private static string GenerateNewAuthToken()
    {
        return (RandomNumberGenerator.GetInt32(10000000) * 1000)
                .ToString()
                .PadLeft(10, '0');
    }

    private readonly IUsersRepository _usersRepository;
    private readonly IUsersCodeConfirmer _usersCodeConfirmer;
    private readonly ISerializer<User> _userInfoSerializer;
    private ILogger _logger;
}
