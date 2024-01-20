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

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("snapshots")]
public sealed class SnapshotController : ControllerBase
{
    public SnapshotController(
        ISnapshotsHandler snapshotsHandler,
        ISnapshotsRepository snapshotsRepository,
        IUsersRepository usersRepository)
    {
        _snapshotsHandler = snapshotsHandler
            ?? throw new ArgumentNullException(nameof(snapshotsHandler));

         _snapshotsRepository = snapshotsRepository
            ?? throw new ArgumentNullException(nameof(snapshotsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));
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
            .CreateSnapshotDescriptionAsync(userId, snapshotToCreate, token);

        Debug.Assert(snapshotDescriptionResult != null);

        var snapshot = new Snapshot
        {
            Description = snapshotDescriptionResult.Content,
            SnapshotType = snapshotToCreate.SnapshotType,
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
        response2.Result = true;
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

            var manager = await _usersRepository.GetUserByIdAsync(userId, token);

            if (manager == null)
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
                    $" user with id {snapshotWithIdRequest.UserId} is not its manager";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var snapshotDescription = existedSnapshot.Description;

            SnapshotDescriptionResult? snapshotDescriptionModel =
                JsonConvert.DeserializeObject<SnapshotDescriptionResult>(snapshotDescription);

            Debug.Assert(snapshotDescriptionModel != null);

            var snapshotInfo = new SnapshotInfoResponse
            {
                BeginMoment = existedSnapshot.BeginMoment,
                EndMoment = existedSnapshot.EndMoment,
                SnapshotType = existedSnapshot.SnapshotType,
                CreationTime = DateTimeOffset.Now,
                Content = existedSnapshot.Description
            };

            var getResponse = new GetResponse();
            getResponse.Result = true;
            getResponse.OutInfo = 
                $"Info about snapshot with id {snapshotId}" +
                $" for user with id {userId} was received";
            getResponse.RequestedInfo = snapshotInfo;

            var json = JsonConvert.SerializeObject(getResponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such snapshot with id {snapshotId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private readonly ISnapshotsHandler _snapshotsHandler;
    private readonly ISnapshotsRepository _snapshotsRepository;
    private readonly IUsersRepository _usersRepository;
}

