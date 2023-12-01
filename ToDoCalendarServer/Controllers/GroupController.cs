using Contracts.Request;
using Contracts.Request.RequestById;
using Contracts.Response;
using Logic.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BusinessModels;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("groups")]
public sealed class GroupController : ControllerBase
{
    public GroupController(
        IGroupsRepository groupsRepository,
        IUsersRepository usersRepository,
        IGroupingUsersMapRepository groupingUsersMapRepository) 
    {
        _groupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _groupingUsersMapRepository = groupingUsersMapRepository
            ?? throw new ArgumentNullException(nameof(groupingUsersMapRepository));
    }

    [HttpPost]
    [Route("create")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateGroup(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupToCreate = JsonConvert.DeserializeObject<GroupInputDTO>(body);

        Debug.Assert(groupToCreate != null);

        var selfUser = await _usersRepository.GetUserByIdAsync(groupToCreate.UserId, token);

        if (selfUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Group has not been created cause current user was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var group = new Group()
        {
            GroupName = groupToCreate.GroupName,
            Type = groupToCreate.Type,
            ParticipantsMap = new List<GroupingUsersMap>()
        };

        await _groupsRepository.AddAsync(group, token);

        _groupsRepository.SaveChanges();

        var groupId = group.Id;

        var listGroupingUsersMap = new List<GroupingUsersMap>();

        if (groupToCreate.Participants != null)
        {
            var existedUsers = await _usersRepository.GetAllUsersAsync(token);

            foreach (var userId in groupToCreate.Participants)
            {
                var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                if (currentUser != null)
                {
                    var map = new GroupingUsersMap
                    {
                        UserId = userId,
                        GroupId = groupId,
                    };

                    await _groupingUsersMapRepository.AddAsync(map, token);

                    listGroupingUsersMap.Add(map);
                }
            }

            _groupingUsersMapRepository.SaveChanges();
        }

        var selfUserMap = new GroupingUsersMap
        {
            UserId = selfUser.Id,
            GroupId = groupId,
        };

        await _groupingUsersMapRepository.AddAsync(selfUserMap, token);

        _groupingUsersMapRepository.SaveChanges();

        var response = new Response();
        response.Result = true;
        response.OutInfo = 
            $"New group with id = {groupId}" +
            $" and name {group.GroupName} was created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [Route("update_group_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> AddParticipants(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var updateGroupParams = JsonConvert.DeserializeObject<UpdateGroupParams>(body);

        Debug.Assert(updateGroupParams != null);

        var groupId = updateGroupParams.GroupId;
        var currentUserId = updateGroupParams.UserId;

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(groupId, token);

        if (existedGroup != null)
        {
            if (existedGroup.ParticipantsMap == null)
            {
                existedGroup.ParticipantsMap = new List<GroupingUsersMap>();
            }

            var existedMap = await _groupingUsersMapRepository
                .GetGroupingUserMapByIdsAsync(groupId, currentUserId, token);

            if (existedMap == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Group has not been modified cause user not relate to that";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (updateGroupParams.Participants != null) 
            {
                var listGroupingUsersMap = new List<GroupingUsersMap>();

                var existedUsers = await _usersRepository.GetAllUsersAsync(token);

                foreach (var userId in updateGroupParams.Participants)
                {
                    var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                    if (currentUser != null)
                    {
                        var map = new GroupingUsersMap
                        {
                            UserId = userId,
                            GroupId = groupId,
                        };

                        await _groupingUsersMapRepository.AddAsync(map, token);

                        listGroupingUsersMap.Add(map);
                    }
                }

                _groupingUsersMapRepository.SaveChanges();

                if (existedGroup.ParticipantsMap == null)
                {
                    existedGroup.ParticipantsMap = listGroupingUsersMap;
                }
                else
                {
                    existedGroup.ParticipantsMap.AddRange(listGroupingUsersMap);
                }
            }

            if (!string.IsNullOrWhiteSpace(updateGroupParams.GroupName))
            {
                existedGroup.GroupName = updateGroupParams.GroupName;
            }

            if (updateGroupParams.Type != Models.Enums.GroupType.None)
            {
                existedGroup.Type = updateGroupParams.Type;
            }

            await _groupsRepository.UpdateAsync(existedGroup, token);

            _groupsRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Group with id {groupId} has been modified";

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such group with id {groupId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("delete_participant")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteParticipant(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupDeleteParticipant = JsonConvert.DeserializeObject<GroupDeleteParticipant>(body);

        Debug.Assert(groupDeleteParticipant != null);

        if (groupDeleteParticipant.UserId != groupDeleteParticipant.Participant_Id)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Group has not been modified cause user not deleting yourself";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var groupId = groupDeleteParticipant.GroupId;
        var participantId = groupDeleteParticipant.Participant_Id;

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(groupId, token);

        var existedUser = await _usersRepository.GetUserByIdAsync(groupDeleteParticipant.Participant_Id, token);

        if (existedGroup != null && existedUser != null)
        {
            if (existedGroup.ParticipantsMap == null)
            {
                existedGroup.ParticipantsMap = new List<GroupingUsersMap>();
            }

            var existedUserMap = await _groupingUsersMapRepository
                .GetGroupingUserMapByIdsAsync(groupId, existedUser.Id, token);

            if (existedUserMap == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Participants of group with id {groupId} " +
                    $"has not been modified cause user not relate to that";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            await _groupingUsersMapRepository.DeleteAsync(groupId, participantId, token);

            _groupingUsersMapRepository.SaveChanges();

            existedGroup.ParticipantsMap = 
                existedGroup.ParticipantsMap.Where(x => x.UserId != participantId).ToList();

            await _groupsRepository.UpdateAsync(existedGroup, token);

            _groupsRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Participant with id {groupDeleteParticipant.Participant_Id}" +
                $" has been deleted from group with id {groupDeleteParticipant.GroupId}";

            return Ok(JsonConvert.SerializeObject(response));
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {groupDeleteParticipant.GroupId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_group_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetGroupInfo(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupByIdRequest = JsonConvert.DeserializeObject<GroupDetailsRequest>(body);

        Debug.Assert(groupByIdRequest != null);

        var userId = groupByIdRequest.UserId;
        var groupId = groupByIdRequest.GroupId;

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(groupId, token);

        if (existedGroup != null)
        {
            if (existedGroup.ParticipantsMap == null)
            {
                existedGroup.ParticipantsMap = new List<GroupingUsersMap>();
            }

            var existedMaps = await _groupingUsersMapRepository
                .GetGroupingUsersMapByGroupIdsAsync(groupId, token);

            if (existedMaps == null)
            {
                var response1 = new Response();
                response1.Result = true;
                response1.OutInfo = 
                    $"Info about group with id {groupId} has not been received" +
                    $" cause user with id {userId} not relate to that";

                var json1 = JsonConvert.SerializeObject(response1);

                return BadRequest(json1);
            }

            var listOfUsersInfo = new List<ShortUserInfo>();

            foreach(var userMap in existedMaps)
            {
                var participantId = userMap.UserId;

                var user = await _usersRepository
                    .GetUserByIdAsync(participantId, token);

                if (user != null)
                {
                    var shortUserInfo = new ShortUserInfo
                    {
                        UserId = userMap.UserId,
                        UserName = user.UserName,
                        UserEmail = user.Email,
                        UserPhone = user.PhoneNumber
                    };

                    listOfUsersInfo.Add(shortUserInfo);
                }
            }

            var groupInfo = new GroupInfoResponse
            {
                GroupName = existedGroup.GroupName,
                Type = existedGroup.Type,
                Participants = listOfUsersInfo
            };

            var getReponse = new GetResponse();
            getReponse.Result = true;
            getReponse.OutInfo = $"Info about group with id {groupByIdRequest.GroupId} was found";
            getReponse.RequestedInfo = groupInfo;

            var json = JsonConvert.SerializeObject(getReponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {groupByIdRequest.GroupId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }

    private readonly IGroupsRepository _groupsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IGroupingUsersMapRepository _groupingUsersMapRepository;
}
