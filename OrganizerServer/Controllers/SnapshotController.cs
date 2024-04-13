using Contracts.Response;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models.BusinessModels;
using Models;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using Contracts.Request;
using Microsoft.AspNetCore.Authorization;
using Logic.Abstractions;
using Contracts.Response.SnapshotsDB;
using Models.Enums;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("snapshots")]
public sealed class SnapshotController : ControllerBase
{
    public SnapshotController(
        ISnapshotsHandler snapshotsHandler,
        ISnapshotsRepository snapshotsRepository,
        IGroupsRepository groupsRepository,
        IUsersRepository usersRepository)
    {
        _snapshotsHandler = snapshotsHandler
            ?? throw new ArgumentNullException(nameof(snapshotsHandler));

         _snapshotsRepository = snapshotsRepository
            ?? throw new ArgumentNullException(nameof(snapshotsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _groupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));
    }

    [HttpPost]
    [Route("perform_new")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateSnapshot(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var snapshotToCreate = JsonConvert.DeserializeObject<SnapshotInputDTO>(body);

        Debug.Assert(snapshotToCreate != null);

        var userId = snapshotToCreate.UserId;
        var snapshotType = snapshotToCreate.SnapshotType;

        var user = await _usersRepository.GetUserByIdAsync(userId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = 
                $"Snapshot has not been created cause" +
                $" current user with id {userId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var snapshotDescriptionResult = await _snapshotsHandler
            .CreatePersonalSnapshotDescriptionAsync(userId, snapshotToCreate, token);

        Debug.Assert(snapshotDescriptionResult != null);

        var personalSnapshotContent = new PersonalSnapshotContent
        {
            KPI = snapshotDescriptionResult.KPI,
            Content = snapshotDescriptionResult.Content
        };

        var description = JsonConvert.SerializeObject(personalSnapshotContent);

        var snapshot = new Snapshot
        {
            Description = description,
            SnapshotType = snapshotToCreate.SnapshotType,
            SnapshotAuditType = snapshotToCreate.SnapshotAuditType,
            BeginMoment = snapshotToCreate.BeginMoment,
            EndMoment = snapshotToCreate.EndMoment,
            UserId = user.Id
        };

        await _snapshotsRepository.AddAsync(snapshot, token);

        _snapshotsRepository.SaveChanges();

        var snapshotId = snapshot.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New snapshot with id = {snapshotId}" +
            $" for user '{user.UserName}'" +
            $" with type '{snapshotType}' has been created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [HttpPost]
    [Route("perform_for_group")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateGroupSnapshot(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var snapshotToCreate = JsonConvert.DeserializeObject<GroupSnapshotInputDTO>(body);

        Debug.Assert(snapshotToCreate != null);

        var userId = snapshotToCreate.UserId;
        var groupId = snapshotToCreate.GroupId;
        var snapshotType = snapshotToCreate.SnapshotType;

        var user = await _usersRepository.GetUserByIdAsync(userId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Snapshot has not been created cause" +
                $" current user with id {userId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var group = await _groupsRepository.GetGroupByIdAsync(groupId, token);

        if (group == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Snapshot has not been created cause" +
                $" related group with id {groupId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        if (group.ManagerId != userId)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Snapshot has not been created cause" +
                $" current user with id {userId} is not the manager" +
                $" of related group with id {groupId}";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var groupSnapshotDescriptionResult = await _snapshotsHandler
            .CreateGroupKPISnapshotDescriptionAsync(
            userId, snapshotToCreate, token);

        Debug.Assert(groupSnapshotDescriptionResult != null);

        var groupSnapshotContent = new GroupSnapshotContent
        {
            GroupId = groupId,
            ParticipantKPIs = groupSnapshotDescriptionResult.ParticipantKPIs,
            AverageKPI = groupSnapshotDescriptionResult.AverageKPI,
            Content = groupSnapshotDescriptionResult.Content
        };

        var description = JsonConvert.SerializeObject(groupSnapshotContent);

        var snapshot = new Snapshot
        {
            Description = description,
            SnapshotType = snapshotToCreate.SnapshotType,
            SnapshotAuditType = snapshotToCreate.SnapshotAuditType,
            BeginMoment = snapshotToCreate.BeginMoment,
            EndMoment = snapshotToCreate.EndMoment,
            CreateMoment = DateTimeOffset.UtcNow,
            UserId = user.Id
        };

        await _snapshotsRepository.AddAsync(snapshot, token);

        _snapshotsRepository.SaveChanges();

        var snapshotId = snapshot.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New group snapshot with id = {snapshotId}" +
            $" by user '{user.UserName}' for group with id = {groupId}" +
            $" with type '{snapshotType}' has been created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }


    [Route("delete_snapshot")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteSnapshot(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var snapshotToDelete = JsonConvert.DeserializeObject<SnapshotIdDTO>(body);

        Debug.Assert(snapshotToDelete != null);

        var snapshotId = snapshotToDelete.SnapshotId;

        var existedSnapshot = await _snapshotsRepository.GetSnapshotByIdAsync(snapshotId, token);

        if (existedSnapshot != null)
        {
            var userId = existedSnapshot!.UserId;

            if (snapshotToDelete.UserId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Snapshot has not been deleted cause" +
                    $" current user with id {userId} is not its manager";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            await _snapshotsRepository.DeleteAsync(snapshotId, token);

            _snapshotsRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Report with id {snapshotId} was deleted by reporter";

            return Ok(JsonConvert.SerializeObject(response));
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such snapshot with id {snapshotId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_snapshot_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetSnapshotInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var snapshotWithIdRequest = JsonConvert.DeserializeObject<SnapshotIdDTO>(body);

        Debug.Assert(snapshotWithIdRequest != null);

        var snapshotId = snapshotWithIdRequest.SnapshotId;

        var existedSnapshot = await _snapshotsRepository.GetSnapshotByIdAsync(snapshotId, token);

        if (existedSnapshot != null)
        {
            var userId = existedSnapshot!.UserId;

            var creator = await _usersRepository.GetUserByIdAsync(userId, token);

            if (creator == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Cant take info about snapshot" +
                    $" cause user with id {userId} is not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (snapshotWithIdRequest.UserId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Cant take info about snapshot cause" +
                    $" user with id {snapshotWithIdRequest.UserId} is not its creator";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var snapshotDescription = existedSnapshot.Description;

            var getResponse = new GetResponse();
            getResponse.Result = true;

            if (existedSnapshot.SnapshotAuditType == SnapshotAuditType.Personal)
            {
                var personalSnapshotContent =
                    JsonConvert.DeserializeObject<PersonalSnapshotContent>(snapshotDescription);

                Debug.Assert(personalSnapshotContent != null);

                var personalSnapshotInfo = new PersonalSnapshotInfoResponse
                {
                    BeginMoment = existedSnapshot.BeginMoment,
                    EndMoment = existedSnapshot.EndMoment,
                    SnapshotType = existedSnapshot.SnapshotType,
                    CreationTime = DateTimeOffset.UtcNow,
                    AuditType = SnapshotAuditType.Personal,
                    KPI = personalSnapshotContent.KPI,
                    Content = personalSnapshotContent.Content
                };

                getResponse.OutInfo =
                    $"Info about personal snapshot with id {snapshotId}" +
                    $" for user with id {userId} was received";
                getResponse.RequestedInfo = personalSnapshotInfo;
            }
            else if (existedSnapshot.SnapshotAuditType == SnapshotAuditType.Group)
            {
                var groupSnapshotContent =
                    JsonConvert.DeserializeObject<GroupSnapshotContent>(snapshotDescription);

                Debug.Assert(groupSnapshotContent != null);

                var groupSnapshotInfo = new GroupSnapshotInfoResponse
                {
                    BeginMoment = existedSnapshot.BeginMoment,
                    EndMoment = existedSnapshot.EndMoment,
                    SnapshotType = existedSnapshot.SnapshotType,
                    CreationTime = DateTimeOffset.UtcNow,
                    AuditType = SnapshotAuditType.Personal,
                    GroupId = groupSnapshotContent.GroupId,
                    ParticipantKPIs = groupSnapshotContent.ParticipantKPIs,
                    AverageKPI = groupSnapshotContent.AverageKPI,
                    Content = groupSnapshotContent.Content
                };

                getResponse.OutInfo =
                    $"Info about group snapshot with id {snapshotId}" +
                    $" by user with id {userId} for group with id" +
                    $" {groupSnapshotInfo.GroupId} was received";
                getResponse.RequestedInfo = groupSnapshotInfo;
            }

            var json = JsonConvert.SerializeObject(getResponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such snapshot with id {snapshotId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private readonly ISnapshotsHandler _snapshotsHandler;
    private readonly ISnapshotsRepository _snapshotsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IGroupsRepository _groupsRepository;
}

