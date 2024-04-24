using Contracts.Request;
using Contracts.Response.SnapshotsDB;
using Contracts.Response;
using Logic.Abstractions;
using Logic.Transport.Abstractions;
using Logic.Transport.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.BusinessModels;
using Models.Enums;
using Models.StorageModels;
using Models.UserActionModels;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Logic.Transport.Serialization;
using Models.RedisEventModels.UserEvents;
using FirebaseAdmin.Auth;

namespace Logic.ControllerHandlers;

public sealed class UsersDataHandler
    : DataHandlerBase, IUsersDataHandler
{
    public UsersDataHandler(
        IOptions<RootConfiguration> rootConfiguration,
        ISMTPSender usersCodeConfirmer,
        ISerializer<UserInfoContent> userInfoSerializer,
        IDeserializer<UserRegistrationData> usersRegistrationDataDeserializer,
        IDeserializer<UserLoginData> usersLoginDataDeserializer,
        IDeserializer<UserInfoById> userInfoByIdDeserializer,
        IDeserializer<UserLogoutDeviceById> userLogoutByIdDeserializer,
        ICommonUsersUnitOfWork commonUnitOfWork,
        IRedisRepository redisRepository,
        ILogger<UsersDataHandler> logger)
        : base(commonUnitOfWork, redisRepository, logger)
    {
        ArgumentNullException.ThrowIfNull(rootConfiguration);

        _rootConfiguration = rootConfiguration.Value;

        _usersCodeConfirmer = usersCodeConfirmer
            ?? throw new ArgumentNullException(nameof(usersCodeConfirmer));

        _userInfoSerializer = userInfoSerializer
            ?? throw new ArgumentNullException(nameof(userInfoSerializer));

        _usersRegistrationDataDeserializer = usersRegistrationDataDeserializer
            ?? throw new ArgumentNullException(nameof(usersRegistrationDataDeserializer));

        _usersLoginDataDeserializer = usersLoginDataDeserializer
            ?? throw new ArgumentNullException(nameof(usersLoginDataDeserializer));

        _userInfoByIdDeserializer = userInfoByIdDeserializer
            ?? throw new ArgumentNullException(nameof(userInfoByIdDeserializer));

        _userLogoutByIdDeserializer = userLogoutByIdDeserializer
            ?? throw new ArgumentNullException(nameof(userLogoutByIdDeserializer));
    }

    public async Task<Response> TryRegisterUser(
        string registrationData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(registrationData);

        token.ThrowIfCancellationRequested();

        var userRegistrationData =
            _usersRegistrationDataDeserializer.Deserialize(registrationData);

        var email = userRegistrationData.Email;

        var existedUser =
           await CommonUnitOfWork
                .UsersRepository
                .GetUserByEmailAsync(email, token);

        if (existedUser != null)
        {
            Logger.LogInformation(
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
            UserName = userRegistrationData.UserName,
            UserPhone = userRegistrationData.PhoneNumber
        };

        var confirmResponse =
            await _usersCodeConfirmer.ConfirmAsync(shortUserInfo, token);

        var builder = new StringBuilder();
        builder.Append(confirmResponse.OutInfo);
        builder.Append(".\n");

        var confirmResult = confirmResponse.Result;

        if (!confirmResult)
        {
            Logger.LogInformation(
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

        var registrationTime = DateTimeOffset.UtcNow;

        user.Role = DEFAULT_USER_ROLE;
        user.AccountCreation = registrationTime;
        user.Email = email;
        user.UserName = userRegistrationData.UserName;
        user.Password = userRegistrationData.Password;
        user.PhoneNumber = userRegistrationData.PhoneNumber;
        user.GroupingMaps = new List<GroupingUsersMap>();
        user.TasksForImplementation = new List<UserTask>();
        user.EventMaps = new List<EventsUsersMap>();
        user.Snapshots = new List<Snapshot>();
        user.Issues = new List<Issue>();

        string authToken = GenerateNewAuthToken();

        user.AuthToken = authToken;

        Logger.LogInformation("Registrating new user {Email}", user.Email);

        await CommonUnitOfWork.UsersRepository.AddAsync(user, token);

        CommonUnitOfWork.SaveChanges();

        var firebaseToken = userRegistrationData.FirebaseToken;

        var userDeviceMap = new UserDeviceMap();

        userDeviceMap.UserId = user.Id;
        userDeviceMap.FirebaseToken = firebaseToken;
        userDeviceMap.TokenSetMoment = registrationTime;
        userDeviceMap.IsActive = true;

        await CommonUnitOfWork
            .UserDevicesRepository
            .AddAsync(userDeviceMap, token);

        CommonUnitOfWork.SaveChanges();

        await SendEventForCacheAsync(
            new UserRegistrationEvent(
                Id: GenerateNewAuthToken(),
                IsCommited: false,
                UserId: user.Id,
                FirebaseToken: firebaseToken,
                AccountCreationMoment: registrationTime));

        var response = new RegistrationResponse();
        response.Result = true;

        builder.Append(
            $"Registrating new user {user.Email} with id {user.Id}" +
            $" with creating new auth token {authToken}");

        response.Result = true;
        response.OutInfo = builder.ToString();
        response.UserId = user.Id;
        response.Token = authToken;
        response.FirebaseToken = firebaseToken;
        response.UserName = user.UserName;
        response.RegistrationCase = RegistrationCase.ConfirmationSucceeded;

        return await Task.FromResult(response);
    }

    public async Task<Response> TryLoginUser(
        string loginData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(loginData);

        token.ThrowIfCancellationRequested();

        var userLoginData = _usersLoginDataDeserializer.Deserialize(loginData);

        var email = userLoginData.Email;

        var existedUser =
            await CommonUnitOfWork
                .UsersRepository
                .GetUserByEmailAsync(email, token);

        if (existedUser == null)
        {
            Logger.LogInformation(
                "User with email {Email} was not already in DB",
                email);

            var response1 = new LoginResponse();
            response1.Result = false;
            response1.OutInfo = $"User with email {email} was not already in DB";

            return await Task.FromResult(response1);
        }

        if (existedUser.Password != userLoginData.Password)
        {
            Logger.LogInformation("Password not equals");

            var response2 = new LoginResponse();
            response2.Result = false;
            response2.OutInfo = "Password not equals";

            return await Task.FromResult(response2);
        }

        var authToken = GenerateNewAuthToken();

        existedUser.AuthToken = authToken;

        await CommonUnitOfWork
            .UsersRepository
            .UpdateAsync(existedUser, token);

        var loginTime = DateTimeOffset.UtcNow;

        var currentFirebaseToken = userLoginData.FirebaseToken;

        var userDeviceMap = new UserDeviceMap();

        userDeviceMap.UserId = existedUser.Id;
        userDeviceMap.FirebaseToken = currentFirebaseToken;
        userDeviceMap.TokenSetMoment = loginTime;
        userDeviceMap.IsActive = true;

        await CommonUnitOfWork
            .UserDevicesRepository
            .AddAsync(userDeviceMap, token);

        CommonUnitOfWork.SaveChanges();

        await SendEventForCacheAsync(
            new UserLoginEvent(
                Id: GenerateNewAuthToken(),
                IsCommited: false,
                UserId: existedUser.Id,
                FirebaseToken: currentFirebaseToken,
                TokenSetMoment: loginTime));

        var userName = existedUser.UserName;

        var response = new LoginResponse();

        response.Result = true;
        response.OutInfo =
            $"Login existed user {userName}" +
            $" with new auth token {authToken}";
        response.UserId = existedUser.Id;
        response.Token = authToken;
        response.FirebaseToken = currentFirebaseToken;
        response.UserName = userName;

        return await Task.FromResult(response);
    }

    public async Task<Response> TryLogoutUser(
        string logoutData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(logoutData);

        token.ThrowIfCancellationRequested();

        var userLogoutData = _userLogoutByIdDeserializer.Deserialize(logoutData);

        var userId = userLogoutData.UserId;
        var firebaseToken = userLogoutData.FirebaseToken;

        var existedUser =
            await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            Logger.LogInformation(
                "User with id {Id} was not already in DB",
                userId);

            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"User with id {userId} was not already in DB";

            return await Task.FromResult(response1);
        }

        var userDeviceMaps = await CommonUnitOfWork
            .UserDevicesRepository
            .GetAllDevicesMapsAsync(token);

        var existedDeviceMap =
            userDeviceMaps.FirstOrDefault(
                x => x.UserId == userId && x.FirebaseToken == firebaseToken);

        var userName = existedUser.UserName;

        if (existedDeviceMap == null)
        {
            Logger.LogInformation(
                "User with id {Id} not synchronized" +
                " with firebase token {Token} was not already in DB",
                userId,
                firebaseToken);

            var response1 = new Response();
            response1.Result = true;
            response1.OutInfo =
                $"Existed user {userName}" +
                $" not found in data of firebase auth system";

            return await Task.FromResult(response1);
        }

        await CommonUnitOfWork
            .UserDevicesRepository
            .DeleteAsync(userId, firebaseToken, token);

        CommonUnitOfWork.SaveChanges();

        await SendEventForCacheAsync(
            new UserLogoutEvent(
                Id: GenerateNewAuthToken(),
                IsCommited: false,
                UserId: existedUser.Id,
                FirebaseToken: firebaseToken));

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"Existed user {userName}" +
            $" has been successfuly logged out from firebase auth system";

        return await Task.FromResult(response);
    }

    public async Task<GetResponse> GetUserInfo(
        string userInfoById,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(userInfoById);

        token.ThrowIfCancellationRequested();

        var userInfoByIdRequest = _userInfoByIdDeserializer.Deserialize(userInfoById);

        var user =
            await CommonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(userInfoByIdRequest.UserId, token);

        if (user == null)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo = 
                $"No such user with id" +
                $" {userInfoByIdRequest.UserId} found in db";

            return await Task.FromResult(response1);
        }

        var userInfoContent = 
            await FillContentModelRelatedUserInformationAsync(user, token);

        var response = new GetResponse();
        response.Result = true;
        response.OutInfo =
            $"Info about user with with id" +
            $" {userInfoByIdRequest.UserId}" +
            $" has been received";

        response.RequestedInfo = _userInfoSerializer.Serialize(userInfoContent);

        return response;
    }

    public async Task<Response> UpdateUserInfo(
        string userUpdateInfo,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(userUpdateInfo);

        token.ThrowIfCancellationRequested();

        var updateUserInfo = 
            JsonConvert.DeserializeObject<UserUpdateInfoDTO>(userUpdateInfo);

        Debug.Assert(updateUserInfo != null);

        var userId = updateUserInfo.UserId;

        var existedUser = await CommonUnitOfWork
            .UsersRepository
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

            if (!string.IsNullOrWhiteSpace(updateUserInfo.UserName))
            {
                existedUser.UserName = updateUserInfo.UserName;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(updateUserInfo.Email))
            {
                existedUser.Email = updateUserInfo.Email;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(updateUserInfo.Password))
            {
                existedUser.Password = updateUserInfo.Password;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(updateUserInfo.PhoneNumber))
            {
                existedUser.PhoneNumber = updateUserInfo.PhoneNumber;
                numbers_of_new_params++;
            }

            string authToken = GenerateNewAuthToken();

            existedUser.AuthToken = authToken;

            await CommonUnitOfWork
                .UsersRepository
                .UpdateAsync(existedUser, token);

            CommonUnitOfWork.UsersRepository.SaveChanges();

            response1.Result = true;

            if (numbers_of_new_params > 0)
            {
                response1.OutInfo =
                    $"Updating info existed user with id {userId}" +
                    $" and email {existedUser.Email}" +
                    $" with new auth token {authToken}";

                var shortUserInfo = new ShortUserInfo
                {
                    UserId = existedUser.Id,
                    UserName = existedUser.UserName,
                    UserEmail = existedUser.Email,
                    UserPhone = existedUser.PhoneNumber,
                    Role = existedUser.Role,
                };

                var json = JsonConvert.SerializeObject(shortUserInfo);

                await SendEventForCacheAsync(
                    new UserInfoUpdateEvent(
                        Id: GenerateNewAuthToken(),
                        IsCommited: false,
                        UserId: existedUser.Id,
                        UpdateMoment: DateTimeOffset.UtcNow,
                        Json: json));
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
        string userUpdateRoleDTO,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(userUpdateRoleDTO);

        token.ThrowIfCancellationRequested();

        var updateUserRole = 
            JsonConvert.DeserializeObject<UserUpdateRoleDTO>(userUpdateRoleDTO);

        Debug.Assert(updateUserRole != null);

        var userId = updateUserRole.UserId;

        var existedUser = await CommonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"No such user with id {userId} found in db";

            return await Task.FromResult(response1);
        }

        var requestedRole = updateUserRole.NewRole;

        if (existedUser.Role == requestedRole)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = 
                $"User with id {userId}" +
                $" already has a role {requestedRole}";

            return await Task.FromResult(response1);
        }

        if (requestedRole is UserRole.User or UserRole.None)
        {
            var response1 = new Response();

            existedUser.Role = requestedRole;

            await CommonUnitOfWork
                .UsersRepository
                .UpdateAsync(existedUser, token);

            CommonUnitOfWork.SaveChanges();

            response1.Result = true;
            response1.OutInfo =
                $"User with id {userId} has succesfully changed" +
                $" his account type to {requestedRole}";

            return await Task.FromResult(response1);
        }

        if (updateUserRole.RootPassword != _rootConfiguration.RootPassword)
        {
            var response1 = new Response();
            response1.Result = false;

            if (string.IsNullOrWhiteSpace(updateUserRole.RootPassword))
            {
                response1.OutInfo = $"Request was added with empty or white-spaces password";
            }
            else
            {
                response1.OutInfo =
                    $"Request has illegal root password" +
                    $" {updateUserRole.RootPassword}";
            }

            return await Task.FromResult(response1);
        }


        var difference = DateTimeOffset.UtcNow - existedUser.AccountCreation;

        if (difference.TotalDays < _rootConfiguration.MinimalAccountAgeDays)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Requested user with id {userId}" +
                $" has has an account which was created" +
                $" less than {_rootConfiguration.MinimalAccountAgeDays} ago";

            return await Task.FromResult(response1);
        }

        var response = new Response();

        existedUser.Role = requestedRole;

        await CommonUnitOfWork
            .UsersRepository
            .UpdateAsync(existedUser, token);

        CommonUnitOfWork.SaveChanges();

        await SendEventForCacheAsync(
            new UserRoleChangedEvent(
                Id: GenerateNewAuthToken(),
                IsCommited: false,
                UserId: existedUser.Id,
                UpdateMoment: DateTimeOffset.UtcNow,
                NewRole: updateUserRole.NewRole));

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
            await CommonUnitOfWork
            .GroupingUsersMapRepository
            .GetAllMapsAsync(token);

        var allEventsMaps =
            await CommonUnitOfWork
            .EventsUsersMapRepository
            .GetAllMapsAsync(token);

        var userGroupMaps = allGroupsMaps
            .Where(map => map.UserId == userId)
            .ToList();

        var userEventMaps = allEventsMaps
            .Where(map => map.UserId == userId)
            .ToList();

        var allEvents = await CommonUnitOfWork
            .EventsRepository
            .GetAllEventsAsync(token);

        var allTasks =
            await CommonUnitOfWork
            .TasksRepository
            .GetAllTasksAsync(token);

        var userTasksModels = allTasks
            .Where(task => task.ImplementerId == userId)
            .ToList();

        var allIssues = await CommonUnitOfWork
            .IssuesRepository
            .GetAllIssuesAsync(token);

        var userIssuesModels =
            allIssues
            .Where(x => x.UserId == userId)
            .ToList();

        var allSnapshots = await CommonUnitOfWork
            .SnapshotsRepository
            .GetAllSnapshotsAsync(token);

        var userSnapshotsModels = allSnapshots
            .Where(snapshot => snapshot.UserId == userId)
            .ToList();

        var userTasks = new List<TaskInfoResponse>();

        foreach (var task in userTasksModels)
        {
            var reporterId = task.ReporterId;

            var reporter = await CommonUnitOfWork
                .UsersRepository
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

            var group = await CommonUnitOfWork
                .GroupsRepository
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

            var @event = await CommonUnitOfWork
                .EventsRepository
                .GetEventByIdAsync(eventId, token);

            if (@event != null)
            {
                var managerId = @event!.ManagerId;

                var manager = await CommonUnitOfWork
                    .UsersRepository
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

        var userSnapshots = new List<PersonalSnapshotInfoResponse>();

        foreach (var snapshot in userSnapshotsModels)
        {
            var snapshotContent =
                JsonConvert.DeserializeObject<PersonalSnapshotContent>(snapshot.Description);

            Debug.Assert(snapshotContent is not null);

            var snapshotInfo = new PersonalSnapshotInfoResponse
            {
                BeginMoment = snapshot.BeginMoment,
                EndMoment = snapshot.EndMoment,
                SnapshotType = snapshot.SnapshotType,
                CreationTime = snapshot.CreateMoment,
                AuditType = snapshot.SnapshotAuditType,
                KPI = snapshotContent.KPI,
                Content = snapshotContent.Content
            };

            userSnapshots.Add(snapshotInfo);
        }

        var userIssues = new List<IssueInfoResponse>();

        foreach (var issue in userIssuesModels)
        {
            var issueInfo = new IssueInfoResponse
            {
                IssueId = issue.Id,
                Title = issue.Title,
                IssueStatus = issue.Status,
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
        return Guid.NewGuid().ToString();
    }

    private const UserRole DEFAULT_USER_ROLE = UserRole.User;

    private readonly ISMTPSender _usersCodeConfirmer;
    private readonly ISerializer<UserInfoContent> _userInfoSerializer;
    private readonly RootConfiguration _rootConfiguration;

    private readonly IDeserializer<UserRegistrationData> _usersRegistrationDataDeserializer;
    private readonly IDeserializer<UserLoginData> _usersLoginDataDeserializer;
    private readonly IDeserializer<UserInfoById> _userInfoByIdDeserializer;
    private readonly IDeserializer<UserLogoutDeviceById> _userLogoutByIdDeserializer;
}
