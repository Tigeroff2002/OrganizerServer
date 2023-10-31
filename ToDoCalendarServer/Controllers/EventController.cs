using Contracts;
using Contracts.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.BusinessModels;
using Newtonsoft.Json;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("events")]
public sealed class EventController : ControllerBase
{
    public EventController(
        IEventsRepository eventsRepository,
        IGroupsRepository groupsRepository,
        IUsersRepository usersRepository,
        IEventsUsersMapRepository eventsUsersMapRepository,
        IGroupingUsersMapRepository groupingUsersMapRepository)
    {
        _eventsRepository = eventsRepository
            ?? throw new ArgumentNullException(nameof(eventsRepository));

        _groupsRepository = groupsRepository 
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));

        _eventsUsersMapRepository = eventsUsersMapRepository
            ?? throw new ArgumentNullException(nameof(eventsUsersMapRepository));

        _groupingUsersMapRepository = groupingUsersMapRepository
            ?? throw new ArgumentNullException(nameof(groupingUsersMapRepository));
    }

    [HttpPost]
    [Route("schedule_new")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> ScheduleNewEvent(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var eventToCreate = JsonConvert.DeserializeObject<EventInputDTO>(body);

        Debug.Assert(eventToCreate != null);

        var managerId = eventToCreate.UserId;
        var groupId = eventToCreate.GroupId;

        var manager = await _usersRepository.GetUserByIdAsync(managerId, token);

        if (manager == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Event has not been scheduled cause current user was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var group = await _groupsRepository.GetGroupByIdAsync(groupId, token);

        if (group == null)
        {
            if (eventToCreate.EventType != Models.Enums.EventType.Personal)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Event has not been scheduled cause related group was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }
        }

        var @event = new Event
        {
            Caption = eventToCreate.Caption,
            Description = eventToCreate.Description,
            ScheduledStart = eventToCreate.ScheduledStart,
            Duration = eventToCreate.Duration,
            EventType = eventToCreate.EventType,
            Status = eventToCreate.EventStatus,
            ManagerId = managerId,
            RelatedGroupId = groupId
        };

        await _eventsRepository.AddAsync(@event, token);

        _eventsRepository.SaveChanges();

        var eventId = @event.Id;

        @event.Manager = manager;
        @event.RelatedGroup = group!;

        var listGuestsMaps = new List<EventsUsersMap>();

        if (eventToCreate.GuestsIds != null)
        {
            var existedUsers = await _usersRepository.GetAllUsersAsync(token);

            foreach (var userId in eventToCreate.GuestsIds)
            {
                var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                if (currentUser != null)
                {
                    var map = new EventsUsersMap
                    {
                        UserId = userId,
                        User = currentUser,
                        DecisionType = Models.Enums.DecisionType.Default,
                        EventId = eventId,
                        Event = @event
                    };

                    await _eventsUsersMapRepository.AddAsync(map, token);

                    listGuestsMaps.Add(map);
                }
            }

            _eventsUsersMapRepository.SaveChanges();
        }

        var managerMap = new EventsUsersMap
        {
            UserId = manager.Id,
            DecisionType = Models.Enums.DecisionType.Apply,
            EventId = eventId,
        };

        await _eventsUsersMapRepository.AddAsync(managerMap, token);

        _eventsUsersMapRepository.SaveChanges();

        listGuestsMaps.Add(managerMap);

        @event.GuestsMap = listGuestsMaps;

        await _eventsRepository.UpdateAsync(@event, token);

        _eventsRepository.SaveChanges();

        var response = new Response();
        response.Result = true;
        response.OutInfo = group != null
            ? $"New event with id = {eventId} related to group {groupId}" +
                $" with name {group.GroupName} has been created"
            : $"New event with id = {eventId} personal" +
                $" for manager with id {managerId} has been created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [HttpPut]
    [Route("update_event_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateEventParams(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var updateEventParams = JsonConvert.DeserializeObject<EventInputWithIdDTO>(body);

        Debug.Assert(updateEventParams != null);

        var eventId = updateEventParams.EventId;

        var existedEvent = await _eventsRepository.GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            var userId = updateEventParams.UserId;

            var user = await _usersRepository.GetUserByIdAsync(userId, token);

            if (user == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Event has not been modified cause current user was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var managerId = existedEvent.ManagerId;

            var manager = await _usersRepository.GetUserByIdAsync(userId, token);

            existedEvent.Manager = manager!;

            if (existedEvent.Manager.Id != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Event has not been modified cause current user with id {userId}" +
                    $" is not manager: 'managerId = {existedEvent.Manager.Id}' of this event";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (updateEventParams.GuestsIds != null)
            {
                var listGuestsMap = new List<EventsUsersMap>();

                var existedUsers = await _usersRepository.GetAllUsersAsync(token);

                foreach (var guestId in updateEventParams.GuestsIds)
                {
                    var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                    if (currentUser != null)
                    {
                        var map = new EventsUsersMap
                        {
                            UserId = guestId,
                            EventId = eventId,
                            DecisionType = Models.Enums.DecisionType.Default
                        };

                        listGuestsMap.Add(map);
                    }
                }

                if (existedEvent.EventType != Models.Enums.EventType.Personal)
                {
                    foreach(var map in listGuestsMap)
                    {
                        await _eventsUsersMapRepository.AddAsync(map, token);
                    }

                    _eventsUsersMapRepository.SaveChanges();

                    if (existedEvent.GuestsMap == null)
                    {
                        existedEvent.GuestsMap = listGuestsMap;
                    }
                    else
                    {
                        existedEvent.GuestsMap.AddRange(listGuestsMap);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(updateEventParams.Caption))
            {
                existedEvent.Caption = updateEventParams.Caption;
            }

            if (!string.IsNullOrWhiteSpace(updateEventParams.Description))
            {
                existedEvent.Description = updateEventParams.Description;
            }

            if (updateEventParams.ScheduledStart != DateTimeOffset.MinValue)
            {
                existedEvent.ScheduledStart = updateEventParams.ScheduledStart;
            }

            if (updateEventParams.Duration != TimeSpan.Zero)
            {
                existedEvent.Duration = updateEventParams.Duration;
            }

            if (updateEventParams.EventType != Models.Enums.EventType.None)
            {
                existedEvent.EventType = updateEventParams.EventType;
            }

            if (updateEventParams.EventStatus != Models.Enums.EventStatus.None)
            {
                existedEvent.Status = updateEventParams.EventStatus;
            }

            await _eventsRepository.UpdateAsync(existedEvent, token);

            _eventsRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = existedEvent.RelatedGroup != null
                ? $"Existed event with id = {eventId} related" +
                    $" to group {existedEvent.RelatedGroup.Id} has been modified"
                : $"Existed event with id = {eventId} personal for manager " +
                    $"with id {existedEvent.Manager.Id} has been modified";

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such event with id {eventId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpPut]
    [Route("change_user_decision_for_event")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> ChangeUserDecisionForEvent(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var updateEventParams = JsonConvert.DeserializeObject<EventUserDecisionDTO>(body);

        Debug.Assert(updateEventParams != null);

        var eventId = updateEventParams.EventId;

        var existedEvent = await _eventsRepository.GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            var userId = updateEventParams.UserId;

            var user = await _usersRepository.GetUserByIdAsync(userId, token);

            if (user == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Event has not been modified cause current user was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var existedUserEventMap = await _eventsUsersMapRepository
                .GetEventUserMapByIdsAsync(eventId, userId, token);

            if (existedUserEventMap == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"User decision for event {eventId} has not been modified" +
                    $" cause user with id {userId} not relate to that";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var decisionType = updateEventParams.DecisionType;

            if (decisionType != Models.Enums.DecisionType.None)
            {
                await _eventsUsersMapRepository
                    .UpdateDecisionAsync(eventId, userId, decisionType, token);

                _eventsUsersMapRepository.SaveChanges();
            }

            var response = new Response();
            response.Result = true;
            response.OutInfo = existedEvent.RelatedGroup != null
                ? $"For event with id = {eventId} related" +
                    $" to group {existedEvent.RelatedGroupId}" +
                    $" for user {user.UserName}" +
                    $" has been changed decision to {decisionType}"
                : $"For event with id = {eventId} personal for manager " +
                    $"with id {existedEvent.ManagerId}" +
                    $" has been changed decision to {decisionType}";

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such event with id {eventId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpDelete]
    [Route("delete_event")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteEvent(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var eventDeleteParams = JsonConvert.DeserializeObject<EventIdDTO>(body);

        Debug.Assert(eventDeleteParams != null);

        var eventId = eventDeleteParams.EventId;

        var existedEvent = await _eventsRepository.GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            var userId = eventDeleteParams.UserId;
            var managerId = existedEvent.Manager.Id;

            var user = await _usersRepository.GetUserByIdAsync(userId, token);

            if (user == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Event has not been modified cause current user was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (managerId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event has not been modified cause current user with id {userId}" +
                    $" is not manager: 'managerId = {managerId}' of this event";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            await _eventsRepository.DeleteAsync(eventId, token);

            _eventsRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Event with id = {eventId} has been deleted";

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such event with id {eventId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpGet]
    [Route("get_event_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetEventInfo(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var getEventInfoRequest = JsonConvert.DeserializeObject<EventIdDTO>(body);

        Debug.Assert(getEventInfoRequest != null);

        var eventId = getEventInfoRequest.EventId;

        var existedEvent = await _eventsRepository.GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            var userId = getEventInfoRequest.UserId;

            var user = await _usersRepository.GetUserByIdAsync(userId, token);

            if (user == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Event info has not been received cause current user was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var isAccessedToEventInfo = existedEvent.ManagerId == userId;

            var manager = await _usersRepository.GetUserByIdAsync(existedEvent.ManagerId, token);

            existedEvent.Manager = manager!;

            var relatedGroupId = existedEvent.RelatedGroupId;

            var relatedGroup = await _groupsRepository.GetGroupByIdAsync(relatedGroupId, token);

            existedEvent.RelatedGroup = relatedGroup!;

            var groupingUsersMaps = await _groupingUsersMapRepository
                .GetAllMapsAsync(token);

            var groupUsers = groupingUsersMaps.Where(x => x.GroupId == relatedGroupId);

            var existedUserMap = groupUsers.FirstOrDefault(x => x.UserId == userId);

            if (existedUserMap != null) 
            {
                isAccessedToEventInfo = true;
            }

            if (!isAccessedToEventInfo)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event info has not been received cause current user with id {userId}" +
                    $" is not manager: 'managerId = {existedEvent.Manager.Id}'" +
                    $" or he is not participant of related group for this event";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var listGuests = new List<ShortUserInfo>();

            if (existedEvent.GuestsMap != null)
            {
                foreach(var guestMap in existedEvent.GuestsMap)
                {
                    var guest = guestMap.User;

                    var userInfo = new ShortUserInfo
                    {
                        UserEmail = guest.Email,
                        UserName = guest.UserName,
                        UserPhone = guest.PhoneNumber
                    };

                    listGuests.Add(userInfo);
                }
            }

            var managerInfo = new ShortUserInfo
            {
                UserEmail = existedEvent.Manager.Email,
                UserName = existedEvent.Manager.UserName,
                UserPhone = existedEvent.Manager.PhoneNumber
            };

            GroupInfoResponse groupInfoResponse = null!;

            if (existedEvent.RelatedGroup != null)
            {
                var participants = new List<ShortUserInfo>();

                if (existedEvent.RelatedGroup.ParticipantsMap != null)
                {
                    foreach (var participantMap in existedEvent.RelatedGroup.ParticipantsMap)
                    {
                        var participant = participantMap.User;

                        var userInfo = new ShortUserInfo
                        {
                            UserEmail = participant.Email,
                            UserName = participant.UserName,
                            UserPhone = participant.PhoneNumber
                        };

                        participants.Add(userInfo);
                    }
                }

                groupInfoResponse = new GroupInfoResponse
                {
                    GroupName = existedEvent.RelatedGroup.GroupName,
                    Type = existedEvent.RelatedGroup.Type,
                    Participants = participants
                };
            }

            var eventInfo = new EventInfoResponse
            {
                Caption = existedEvent.Caption,
                Description = existedEvent.Description,
                ScheduledStart = existedEvent.ScheduledStart,
                Duration = existedEvent.Duration,
                EventType = existedEvent.EventType,
                EventStatus = existedEvent.Status,
                Manager = managerInfo,
                Group = groupInfoResponse,
                Guests = listGuests
            };

            var response = new GetResponse();
            response.Result = true;
            response.OutInfo =
                $"Info about event with id = {eventId} has been received";
            response.RequestedInfo = eventInfo;

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such event with id {eventId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }

    private readonly IEventsRepository _eventsRepository;
    private readonly IGroupsRepository _groupsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IEventsUsersMapRepository _eventsUsersMapRepository;
    private readonly IGroupingUsersMapRepository _groupingUsersMapRepository;
}

