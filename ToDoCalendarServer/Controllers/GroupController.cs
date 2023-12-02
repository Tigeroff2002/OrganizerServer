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
        IGroupingUsersMapRepository groupingUsersMapRepository,
        IEventsUsersMapRepository eventsUsersMapRepository,
        IEventsRepository eventsRepository) 
    {
        _groupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _groupingUsersMapRepository = groupingUsersMapRepository
            ?? throw new ArgumentNullException(nameof(groupingUsersMapRepository));

        _eventsUsersMapRepository = eventsUsersMapRepository
            ?? throw new ArgumentNullException(nameof(eventsUsersMapRepository));

        _eventsRepository = eventsRepository
            ?? throw new ArgumentNullException(nameof(eventsRepository));
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

            var groupContent = new GroupContent
            {
                Participants = listOfUsersInfo
            };

            var content = JsonConvert.SerializeObject(groupContent);

            var groupInfo = new GroupInfoResponse
            {
                GroupId = groupId,
                GroupName = existedGroup.GroupName,
                Type = existedGroup.Type,
                Participants = listOfUsersInfo,
                Content = content
            };

            var getResponse = new GetResponse();
            getResponse.Result = true;
            getResponse.OutInfo = $"Info about group with id {groupByIdRequest.GroupId} was found";
            getResponse.RequestedInfo = groupInfo;

            var json = JsonConvert.SerializeObject(getResponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {groupByIdRequest.GroupId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }


    [Route("get_participant_calendar")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetGroupParticipantCalendarInfo(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var participantCalendarRequest = JsonConvert.DeserializeObject<AnotherUserCalendarRequest>(body);

        Debug.Assert(participantCalendarRequest != null);

        var userId = participantCalendarRequest.UserId;
        var groupId = participantCalendarRequest.GroupId;
        var participantId = participantCalendarRequest.ParticipantId;

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(groupId, token);

        if (existedGroup != null)
        {
            var existedUser = await _usersRepository.GetUserByIdAsync(userId, token);

            if (existedUser != null )
            {
                var participant = await _usersRepository.GetUserByIdAsync(participantId, token);

                if (participant != null)
                {
                    var allGroupMaps = await _groupingUsersMapRepository.GetAllMapsAsync(token);

                    var existedGroupMaps = allGroupMaps
                        .Where(x => x.GroupId == groupId)
                        .ToList();

                    if (existedGroupMaps != null)
                    {
                        var isUserRelatedToGroup = existedGroupMaps.Any(x => x.UserId == userId);
                        var isParticipantRelatedToGroup = existedGroupMaps.Any(x => x.UserId == participantId);

                        if (isUserRelatedToGroup && isParticipantRelatedToGroup)
                        {
                            var resultEvents = new List<EventInfoResponse>();

                            var allEventMaps = await _eventsUsersMapRepository.GetAllMapsAsync(token);
                            var participantEventsInGroup = allEventMaps
                                .Where(x => x.UserId == participantId)
                                .ToList();

                            foreach(var eventMap in participantEventsInGroup)
                            {
                                var currentEventId = eventMap.EventId;
                                var existedEvent = await _eventsRepository.GetEventByIdAsync(currentEventId, token);

                                if (existedEvent != null)
                                {
                                    var relatedGroupId = existedEvent.RelatedGroupId;

                                    if (relatedGroupId == groupId)
                                    {
                                        var eventInfo = new EventInfoResponse
                                        {
                                            EventId = currentEventId,
                                            Caption = existedEvent.Caption,
                                            Description = existedEvent.Description,
                                            ScheduledStart = existedEvent.ScheduledStart,
                                            Duration = existedEvent.Duration,
                                            EventType = existedEvent.EventType,
                                            EventStatus = existedEvent.Status
                                        };

                                        resultEvents.Add(eventInfo);
                                    }
                                }
                            }

                            var participantCalendarResponse = new AnotherUserCalendarResponse
                            {
                                UserEvents = resultEvents
                            };

                            var getResponse = new GetResponse();
                            getResponse.Result = true;
                            getResponse.OutInfo = 
                                $"Info about group's participant calendar with id" +
                                $" {participantCalendarRequest.ParticipantId} was found";
                            getResponse.RequestedInfo = 
                                JsonConvert.SerializeObject(participantCalendarResponse);

                            var json = JsonConvert.SerializeObject(getResponse);

                            return Ok(json);
                        }
                    }

                    var response5 = new Response();
                    response5.Result = false;
                    response5.OutInfo = $"Info about participant {participantCalendarRequest.ParticipantId} forbidden";

                    return Forbid(JsonConvert.SerializeObject(response5));
                }

                var response4 = new Response();
                response4.Result = false;
                response4.OutInfo = $"No such participant with id {participantCalendarRequest.ParticipantId}";

                return BadRequest(JsonConvert.SerializeObject(response4));
            }

            var response3 = new Response();
            response3.Result = false;
            response3.OutInfo = $"No such user with id {participantCalendarRequest.UserId}";

            return BadRequest(JsonConvert.SerializeObject(response3));
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {participantCalendarRequest.GroupId}";

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
    private readonly IEventsUsersMapRepository _eventsUsersMapRepository;
    private readonly IEventsRepository _eventsRepository;
}
