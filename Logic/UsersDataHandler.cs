using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Logic.Transport.Abstractions;
using Logic.Transport.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using Models.BusinessModels;
using Models.Enums;
using PostgreSQL.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace Logic;

public sealed class UsersDataHandler
    : IUsersDataHandler
{
    public UsersDataHandler(
        IUsersUnitOfWork usersUnitOfWork,
        IOptions<RootConfiguration> rootConfiguration,
        ISMTPSender usersCodeConfirmer,
        ISerializer<UserInfoContent> userInfoSerializer,
        ILogger<UsersDataHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(rootConfiguration);

        _rootConfiguration = rootConfiguration.Value;

        _usersUnitOfWork = usersUnitOfWork
            ?? throw new ArgumentNullException(nameof(usersUnitOfWork));

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
           await _usersUnitOfWork.UsersRepository
                .GetUserByEmailAsync(email, token);

        if (existedUser != null)
        {
            _logger.LogInformation(
                "User with email {Email} was already in DB",
                email);

            var response1 = new RegistrationResponse();
            response1.Result = true;
            response1.OutInfo = $"User with email {email} was already in DB";
            response1.RegistrationCase = RegistrationCase.SuchUserExisted;

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
                "User account link confirmation was not succesfull");

            var response2 = new RegistrationResponse();
            response2.Result = true;
            response2.RegistrationCase = RegistrationCase.ConfirmationFailed;

            builder.Append(
                $"User account link confirmation was not succesfull" +
                $" for user with email {email}.");
            response2.OutInfo = builder.ToString();

            return await Task.FromResult(response2);
        }

        var user = new User();

        user.Role = DEFAULT_USER_ROLE;
        user.AccountCreation = DateTimeOffset.Now;

        user.Email = email;
        user.UserName = registrationData.UserName;
        user.Password = registrationData.Password;
        user.PhoneNumber = registrationData.PhoneNumber;
        user.GroupingMaps = new List<GroupingUsersMap>();
        user.TasksForImplementation = new List<UserTask>();
        user.EventMaps = new List<EventsUsersMap>();
        user.Snapshots = new List<Snapshot>();
        user.Issues = new List<Issue>();

        string authToken = GenerateNewAuthToken();

        user.AuthToken = authToken;

        _logger.LogInformation("Registrating new user {Email}", user.Email);

        await _usersUnitOfWork.UsersRepository.AddAsync(user, token);

        _usersUnitOfWork.SaveChanges();

        var response = new RegistrationResponse();
        response.Result = true;

        builder.Append(
            $"Registrating new user {user.Email} with id {user.Id}" +
            $" with creating new auth token {authToken}");

        response.Result = true;
        response.OutInfo = builder.ToString();
        response.UserId = user.Id;
        response.Token = authToken;
        response.UserName = user.UserName;
        response.RegistrationCase = RegistrationCase.ConfirmationSucceeded;

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
            await _usersUnitOfWork.UsersRepository
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

        await _usersUnitOfWork.UsersRepository.UpdateAsync(existedUser, token);

        _usersUnitOfWork.SaveChanges();

        var userName = existedUser.UserName;

        var response = new ResponseWithToken();
        response.Result = true;
        response.OutInfo = 
            $"Login existed user {userName}" +
            $" with new auth token {authToken}";
        response.UserId = existedUser.Id;
        response.Token = authToken;
        response.UserName = userName;

        return await Task.FromResult(response);
    }

    public async Task<GetResponse> GetUserInfo(
        UserInfoById userInfoById,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(userInfoById);

        token.ThrowIfCancellationRequested();

        var user = 
            await _usersUnitOfWork.UsersRepository
            .GetUserByIdAsync(userInfoById.UserId, token);

        if (user == null)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo = $"No such user with id {userInfoById.UserId} found in db";

            return await Task.FromResult(response1);
        }

        var userInfoContent = await FillContentModelRelatedUserInformationAsync(user, token);

        var response = new GetResponse();
        response.Result = true;
        response.OutInfo = 
            $"Info about user with with id" +
            $" {userInfoById.UserId} has been received";

        response.RequestedInfo = _userInfoSerializer.Serialize(userInfoContent);

        return response;
    }

    public async Task<Response> UpdateUserInfo(
        UserUpdateInfoDTO userUpdateInfo, 
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(userUpdateInfo);

        var userId = userUpdateInfo.UserId;

        var existedUser = await _usersUnitOfWork.UsersRepository
            .GetUserByIdAsync(userId, token);

        if (existedUser == null) 
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"No such user with id {userId} found in db";

            return await Task.FromResult(response1);
        }

        if (existedUser != null)
        {
            var response1 = new Response();

            var numbers_of_new_params = 0;

            if (!string.IsNullOrWhiteSpace(userUpdateInfo.UserName))
            {
                existedUser.UserName = userUpdateInfo.UserName;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateInfo.Email))
            {
                existedUser.Email = userUpdateInfo.Email;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateInfo.Password))
            {
                existedUser.Password = userUpdateInfo.Password;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateInfo.PhoneNumber))
            {
                existedUser.PhoneNumber = userUpdateInfo.PhoneNumber;
                numbers_of_new_params++;
            }

            string authToken = GenerateNewAuthToken();

            existedUser.AuthToken = authToken;

            await _usersUnitOfWork.UsersRepository.UpdateAsync(existedUser, token);

            _usersUnitOfWork.UsersRepository.SaveChanges();

            response1.Result = true;

            if (numbers_of_new_params > 0)
            {
                response1.OutInfo =
                    $"Updating info existed user with id {userId}" +
                    $" and email {existedUser.Email}" +
                    $" with new auth token {authToken}";
            }
            else
            {
                response1.OutInfo =
                    $"There are no new data for user with id {userId}." +
                    $" Only new auth token {authToken} has been created";
            }

            return await Task.FromResult(response1);
        }

        var response = new Response();
        response.Result = true;
        response.OutInfo = $"No such existed user with id {userId}";

        return await Task.FromResult(response);
    }

    public async Task<Response> UpdateUserRoleAsync(
        UserUpdateRoleDTO userUpdateRoleDTO,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(userUpdateRoleDTO);

        token.ThrowIfCancellationRequested();

        var userId = userUpdateRoleDTO.UserId;

        var existedUser = await _usersUnitOfWork.UsersRepository
            .GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"No such user with id {userId} found in db";

            return await Task.FromResult(response1);
        }

        var requestedRole = userUpdateRoleDTO.NewRole;

        if (existedUser.Role == requestedRole)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo = $"User with id {userId} already has a role {requestedRole}";

            return await Task.FromResult(response1);
        }

        if (requestedRole is UserRole.User or UserRole.None)
        {
            var response1 = new Response();

            existedUser.Role = requestedRole;

            await _usersUnitOfWork.UsersRepository.UpdateAsync(existedUser, token);

            _usersUnitOfWork.SaveChanges();

            response1.Result = true;
            response1.OutInfo =
                $"User with id {userId} has succesfully changed" +
                $" his account type to {requestedRole}";

            return await Task.FromResult(response1);
        }

        if (userUpdateRoleDTO.RootPassword != _rootConfiguration.RootPassword)
        {
            var response1 = new Response();
            response1.Result = false;

            if (string.IsNullOrWhiteSpace(userUpdateRoleDTO.RootPassword))
            {
                response1.OutInfo = $"Request was added with empty or white-spaces password";
            }
            else
            {
                response1.OutInfo = 
                    $"Request has illegal root password" +
                    $" {userUpdateRoleDTO.RootPassword}";
            }

            return await Task.FromResult(response1);
        }


        var difference = DateTimeOffset.UtcNow - existedUser.AccountCreation;

        if (difference.TotalDays < _rootConfiguration.MinimalAccountAge)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo = 
                $"Requested user with id {userId}" +
                $" has has an account that is too new";

            return await Task.FromResult(response1);
        }

        var response = new Response();

        existedUser.Role = requestedRole;

        await _usersUnitOfWork.UsersRepository.UpdateAsync(existedUser, token);

        _usersUnitOfWork.SaveChanges();

        response.Result = true;
        response.OutInfo = 
            $"User with id {userId} has succesfully changed" +
            $" his account type to {requestedRole}";

        return await Task.FromResult(response);
    }

    private async Task<UserInfoContent> FillContentModelRelatedUserInformationAsync(
        User user,
        CancellationToken token)
    {
        var userId = user.Id;

        var allGroupsMaps = 
            await _usersUnitOfWork.GroupingUsersMapRepository
            .GetAllMapsAsync(token);

        var allEventsMaps = 
            await _usersUnitOfWork.EventsUsersMapRepository
            .GetAllMapsAsync(token);

        var userGroupMaps = allGroupsMaps
            .Where(map => map.UserId == userId)
            .ToList();

        var userEventMaps = allEventsMaps
            .Where(map => map.UserId == userId)
            .ToList();

        var allEvents = await _usersUnitOfWork.EventsRepository
            .GetAllEventsAsync(token);

        var allTasks = 
            await _usersUnitOfWork.TasksRepository
            .GetAllTasksAsync(token);

        var userTasksModels = allTasks
            .Where(task => task.ImplementerId == userId)
            .ToList();

        var allIssues = await _usersUnitOfWork.IssuesRepository
            .GetAllIssuesAsync(token);

        var userIssuesModels =
            allIssues
            .Where(x => x.UserId == userId)
            .ToList();

        var allSnapshots = await _usersUnitOfWork.SnapshotsRepository
            .GetAllSnapshotsAsync(token);

        var userSnapshotsModels = allSnapshots
            .Where(snapshot => snapshot.UserId == userId)
            .ToList();

        var userTasks = new List<TaskInfoResponse>();

        foreach (var task in userTasksModels)
        {
            var reporterId = task.ReporterId;

            var reporter = await _usersUnitOfWork.UsersRepository
                .GetUserByIdAsync(reporterId, token);

            var reporterInfo = new ShortUserInfo
            {
                UserId = reporter!.Id,
                UserEmail = reporter!.Email,
                UserName = reporter!.UserName,
                UserPhone = reporter!.PhoneNumber
            };

            var taskInfo = new TaskInfoResponse
            {
                TaskId = task.Id,
                TaskCaption = task!.Caption,
                TaskDescription = task!.Description,
                TaskType = task!.TaskType,
                TaskStatus = task!.TaskStatus,
                Reporter = reporterInfo
            };

            userTasks.Add(taskInfo);
        }

        var userGroups = new List<GroupInfoResponse>();

        foreach (var groupMap in userGroupMaps)
        {
            var groupId = groupMap.GroupId;

            var group = await _usersUnitOfWork.GroupsRepository
                .GetGroupByIdAsync(groupId, token);

            if (group != null)
            {
                var groupInfo = new GroupInfoResponse
                {
                    GroupId = groupId,
                    GroupName = group!.GroupName,
                    Type = group!.Type
                };

                userGroups.Add(groupInfo);
            }
        }

        var userEvents = new List<EventInfoResponse>();

        foreach (var eventMap in userEventMaps)
        {
            var eventId = eventMap.EventId;

            var @event = await _usersUnitOfWork.EventsRepository
                .GetEventByIdAsync(eventId, token);

            if (@event != null)
            {
                var managerId = @event!.ManagerId;

                var manager = await _usersUnitOfWork.UsersRepository
                    .GetUserByIdAsync(managerId, token);

                var managerInfo = new ShortUserInfo
                {
                    UserId = manager!.Id,
                    UserEmail = manager!.Email,
                    UserName = manager!.UserName,
                    UserPhone = manager!.PhoneNumber
                };

                var eventInfo = new EventInfoResponse
                {
                    EventId = eventId,
                    Caption = @event!.Caption,
                    Description = @event!.Description,
                    EventType = @event!.EventType,
                    EventStatus = @event!.Status,
                    ScheduledStart = @event!.ScheduledStart,
                    Duration = @event!.Duration,
                    Manager = managerInfo
                };

                userEvents.Add(eventInfo);
            }
        }

        var userSnapshots = new List<SnapshotInfoResponse>();

        foreach (var snapshot in userSnapshotsModels)
        {
            var snapshotInfo = new SnapshotInfoResponse
            {
                BeginMoment = snapshot.BeginMoment,
                EndMoment = snapshot.EndMoment,
                SnapshotType = snapshot.SnapshotType,
                CreationTime = DateTimeOffset.Now,
                Content = snapshot.Description
            };

            userSnapshots.Add(snapshotInfo);
        }

        var userIssues = new List<IssueInfoResponse>();

        foreach (var issue in userIssuesModels)
        {
            var issueInfo = new IssueInfoResponse
            {
                Title = issue.Title,
                Description = issue.Description,
                IssueType = issue.IssueType,
                ImgLink = issue.ImgLink,
                CreateMoment = issue.IssueMoment
            };

            userIssues.Add(issueInfo);
        }

        return new UserInfoContent
        {
            UserId = userId,
            Role = user.Role,
            UserName = user.UserName,
            Email = user.Email,
            Password = user.Password,
            PhoneNumber = user.PhoneNumber,
            AccountCreationTime = user.AccountCreation,
            Groups = userGroups,
            Events = userEvents,
            Tasks = userTasks,
            Snapshots = userSnapshots,
            Issues = userIssues
        };
    }

    private static string GenerateNewAuthToken()
    {
        return (RandomNumberGenerator.GetInt32(10000000) * 1000)
                .ToString()
                .PadLeft(10, '0');
    }

    private const UserRole DEFAULT_USER_ROLE = UserRole.User;

    private readonly IUsersUnitOfWork _usersUnitOfWork;

    private readonly ISMTPSender _usersCodeConfirmer;
    private readonly ISerializer<UserInfoContent> _userInfoSerializer;
    private readonly RootConfiguration _rootConfiguration;
    private ILogger _logger;
}
