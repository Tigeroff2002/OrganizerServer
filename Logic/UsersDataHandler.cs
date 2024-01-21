using Contracts.Request;
using Contracts.Response;
using Logic.Abstractions;
using Logic.Transport.Abstractions;
using Microsoft.Extensions.Logging;
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
        IUsersRepository usersRepository, 
        IGroupsRepository groupsRepository,
        ITasksRepository tasksRepository,
        IEventsRepository eventsRepository,
        IEventsUsersMapRepository eventsUsersMapRepository,
        IGroupingUsersMapRepository groupingUsersMapRepository,
        ISnapshotsRepository snapshotsRepository,
        IIssuesRepository issuesRepository,

        IUserEmailConfirmer usersCodeConfirmer,
        ISerializer<UserInfoContent> userInfoSerializer,
        ILogger<UsersDataHandler> logger)
    {
        _usersRepository = usersRepository 
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _groupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        _tasksRepository = tasksRepository
            ?? throw new ArgumentNullException(nameof(tasksRepository));

        _eventsRepository = eventsRepository
            ?? throw new ArgumentNullException(nameof(eventsRepository));

        _eventsUsersMapRepository = eventsUsersMapRepository
            ?? throw new ArgumentNullException(nameof(eventsUsersMapRepository));

        _groupingUsersMapRepository = groupingUsersMapRepository
            ?? throw new ArgumentNullException(nameof(groupingUsersMapRepository));

        _snapshotsRepository = snapshotsRepository
            ?? throw new ArgumentNullException(nameof(snapshotsRepository));

        _issuesRepository = issuesRepository
            ?? throw new ArgumentNullException(nameof(issuesRepository));

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

        await _usersRepository.AddAsync(user, token);

        _usersRepository.SaveChanges();

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
        UserInfoById userInfoById, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(userInfoById);

        token.ThrowIfCancellationRequested();

        var user = 
            await _usersRepository.GetUserByIdAsync(userInfoById.UserId, token);

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
            response1.OutInfo = $"No such user with id {userId} found in db";

            return await Task.FromResult(response1);
        }

        if (existedUser != null)
        {
            var response1 = new GetResponse();

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

            if (userUpdateInfo.Role != UserRole.None)
            {
                existedUser.Role = userUpdateInfo.Role;
                numbers_of_new_params++;
            }

            string authToken = GenerateNewAuthToken();

            existedUser.AuthToken = authToken;

            await _usersRepository.UpdateAsync(existedUser, token);

            _usersRepository.SaveChanges();

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

            var shortUserInfo = new ShortUserInfo
            {
                UserId = existedUser.Id,
                UserEmail = existedUser.Email,
                UserName = existedUser.UserName,
                UserPhone = existedUser.PhoneNumber,
                Role = existedUser.Role
            };

            response1.RequestedInfo = shortUserInfo;

            return await Task.FromResult(response1);
        }

        var response = new GetResponse();
        response.Result = true;
        response.OutInfo = $"No such existed user with id {userId}";

        return await Task.FromResult(response);
    }

    private async Task<UserInfoContent> FillContentModelRelatedUserInformationAsync(
        User user,
        CancellationToken token)
    {
        var userId = user.Id;

        var allGroupsMaps = await _groupingUsersMapRepository.GetAllMapsAsync(token);

        var allEventsMaps = await _eventsUsersMapRepository.GetAllMapsAsync(token);

        var userGroupMaps = allGroupsMaps
            .Where(map => map.UserId == userId)
            .ToList();

        var userEventMaps = allEventsMaps
            .Where(map => map.UserId == userId)
            .ToList();

        var allTasks = await _tasksRepository.GetAllTasksAsync(token);

        var userTasksModels = allTasks
            .Where(task => task.ImplementerId == userId)
            .ToList();

        var userTasks = new List<TaskInfoResponse>();

        foreach (var task in userTasksModels)
        {
            var reporterId = task.ReporterId;

            var reporter = await _usersRepository.GetUserByIdAsync(reporterId, token);

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

        var allEvents = await _eventsRepository.GetAllEventsAsync(token);

        var allSnapshots = await _snapshotsRepository.GetAllSnapshotsAsync(token);

        var userSnapshotsModels = allSnapshots
            .Where(snapshot => snapshot.UserId == userId)
            .ToList();

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

        var allIssues = await _issuesRepository.GetAllIssuesAsync(token);

        var userIssuesModels = 
            allIssues
            .Where(x => x.UserId == userId)
            .ToList();

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

        var userGroups = new List<GroupInfoResponse>();

        foreach (var groupMap in userGroupMaps)
        {
            var groupId = groupMap.GroupId;

            var group = await _groupsRepository.GetGroupByIdAsync(groupId, token);

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

            var @event = await _eventsRepository.GetEventByIdAsync(eventId, token);

            if (@event != null)
            {
                var managerId = @event!.ManagerId;

                var manager = await _usersRepository.GetUserByIdAsync(managerId, token);

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

        return new UserInfoContent
        {
            UserId = userId,
            Role = user.Role,
            UserName = user.UserName,
            Email = user.Email,
            Password = user.Password,
            PhoneNumber = user.PhoneNumber,
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

    private readonly IUsersRepository _usersRepository;
    private readonly IGroupsRepository _groupsRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly IEventsUsersMapRepository _eventsUsersMapRepository;
    private readonly IGroupingUsersMapRepository _groupingUsersMapRepository;
    private readonly ISnapshotsRepository _snapshotsRepository;
    private readonly IIssuesRepository _issuesRepository;

    private readonly IUserEmailConfirmer _usersCodeConfirmer;
    private readonly ISerializer<UserInfoContent> _userInfoSerializer;
    private ILogger _logger;
}
