using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Models;
using Models.BusinessModels;
using PostgreSQL.Abstractions;
using System.Security.Cryptography;

namespace Logic;

public sealed class UsersDataHandler
    : IUsersDataHandler
{
    public UsersDataHandler(
        IUsersRepository usersRepository, 
        IUsersCodeConfirmer usersCodeConfirmer,
        ILogger<UsersDataHandler> logger)
    {
        _usersRepository = usersRepository 
            ?? throw new ArgumentNullException(nameof(usersRepository));
        _usersCodeConfirmer = usersCodeConfirmer
            ?? throw new ArgumentNullException(nameof(usersCodeConfirmer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> TryRegisterUser(
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

            return await Task.FromResult(false);
        }

        var confirmResult = 
            await _usersCodeConfirmer.ConfirmAsync(email, token);

        if (!confirmResult)
        {
            _logger.LogInformation(
                "Wrong user code confirmation received");

            return await Task.FromResult(false);
        }

        var user = new User();
        user.Email = email;
        user.UserName = registrationData.UserName;
        user.Password = registrationData.Password;
        user.PhoneNumber = registrationData.PhoneNumber;
        user.Groups = new List<Group>();
        user.TasksForImplementation = new List<UserTask>();
        user.Events = new List<Event>();
        user.Reports = new List<Report>();

        string authToken = GenerateNewAuthToken();

        user.AuthToken = authToken;

        await _usersRepository.AddAsync(user, token);

        return await Task.FromResult(true);
    }

    public async Task<bool> TryLoginUser(
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

            return await Task.FromResult(false);
        }

        var authToken = GenerateNewAuthToken();

        existedUser.AuthToken = authToken;

        await _usersRepository.UpdateAsync(existedUser, token);

        return await Task.FromResult(true);
    }

    public async Task<User?> GetUserInfo(UserInfoById userInfoById, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(userInfoById);

        token.ThrowIfCancellationRequested();

        var user = 
            await _usersRepository.GetUserByIdAsync(userInfoById.UserId, token);

        if (user == null)
        {
            return null!;
        }

        if (userInfoById.Token != user.AuthToken)
        {
            user.AuthToken = userInfoById.Token;

            await _usersRepository.UpdateAsync(user, token);
        }

        return user;
    }

    private static string GenerateNewAuthToken()
    {
        return (RandomNumberGenerator.GetInt32(10000000) * 1000)
                .ToString()
                .PadLeft(10, '0');
    }

    private readonly IUsersRepository _usersRepository;
    private readonly IUsersCodeConfirmer _usersCodeConfirmer;
    private ILogger _logger;
}
