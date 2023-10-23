using Contracts;
using Contracts.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Models.BusinessModels;
using Newtonsoft.Json;
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
        IUsersRepository usersRepository)
    {
        _eventsRepository = eventsRepository
            ?? throw new ArgumentNullException(nameof(eventsRepository));

        _groupsRepository = groupsRepository 
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));
    }

    [HttpPost]
    [Route("schedule_new")]
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

        var listUsers = new List<User>();

        if (eventToCreate.GuestsIds != null)
        {
            var existedUsers = await _usersRepository.GetAllUsersAsync(token);

            foreach (var userId in eventToCreate.GuestsIds)
            {
                var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                if (currentUser != null)
                {
                    listUsers.Add(currentUser);
                }
            }
        }

        listUsers.Add(manager);

        var @event = new Event
        {
            Caption = eventToCreate.Caption,
            Description = eventToCreate.Description,
            ScheduledStart = eventToCreate.ScheduledStart,
            Duration = eventToCreate.Duration,
            EventType = eventToCreate.EventType,
            Status = eventToCreate.EventStatus
        };

        await _eventsRepository.AddAsync(@event, token);

        var eventId = @event.Id;

        @event.Manager = manager;
        @event.RelatedGroup = group!;
        @event.Guests = listUsers;

        await _eventsRepository.UpdateAsync(@event, token);

        var response = new Response();
        response.Result = true;
        response.OutInfo = group != null
            ? $"New event with id = {eventId} related to group {groupId} with name {group.GroupName} has been created"
            : $"New event with id = {eventId} personal for manager with id {managerId} has been created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(response);
    }

    [HttpPut]
    [Route("update_event_params")]
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

            if (existedEvent.Manager.Id != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Event has not been modified cause current user with id {userId}" +
                    $" is not manager: 'managerId = {existedEvent.Manager.Id}' of this event";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var listGuests = new List<User>();

            if (updateEventParams.GuestsIds != null)
            {
                var existedUsers = await _usersRepository.GetAllUsersAsync(token);

                foreach (var guestId in updateEventParams.GuestsIds)
                {
                    var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                    if (currentUser != null)
                    {
                        listGuests.Add(currentUser);
                    }
                }
            }

            existedEvent.Caption = updateEventParams.Caption;
            existedEvent.Description = updateEventParams.Description;
            existedEvent.ScheduledStart = updateEventParams.ScheduledStart;
            existedEvent.Duration = updateEventParams.Duration;
            existedEvent.EventType = updateEventParams.EventType;
            existedEvent.Status = updateEventParams.EventStatus;
            existedEvent.Manager = user;
            existedEvent.Guests.AddRange(listGuests);

            await _eventsRepository.UpdateAsync(existedEvent, token);

            var response = new Response();
            response.Result = true;
            response.OutInfo = existedEvent.RelatedGroup != null
                ? $"New event with id = {eventId} related" +
                    $" to group {existedEvent.RelatedGroup.Id}" +
                    $" with name {existedEvent.RelatedGroup.GroupName} has been created"
                : $"New event with id = {eventId} personal for manager " +
                    $"with id {existedEvent.Manager.Id} has been created";

            var json = JsonConvert.SerializeObject(response);

            return Ok(response);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such event with id {eventId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpDelete]
    [Route("delete_event")]
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

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Event with id = {eventId} has been deleted";

            var json = JsonConvert.SerializeObject(response);

            return Ok(response);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such event with id {eventId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpGet]
    [Route("get_event_info")]
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

            var isAccessedToInfo = existedEvent.Manager.Id == userId;

            var groupUsers = existedEvent.RelatedGroup.Participants;

            if (groupUsers.FirstOrDefault(x => x.Id == userId) != null) 
            {
                isAccessedToInfo = true;
            }

            if (isAccessedToInfo)
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

            if (existedEvent.Guests != null)
            {
                foreach(var guest in existedEvent.Guests)
                {
                    var userInfo = new ShortUserInfo
                    {
                        UserEmail = guest.Email,
                        UserName = guest.UserName
                    };

                    listGuests.Add(userInfo);
                }
            }

            var manager = new ShortUserInfo
            {
                UserEmail = existedEvent.Manager.Email,
                UserName = existedEvent.Manager.UserName
            };

            GroupInfoResponse groupInfoResponse = null!;

            if (existedEvent.RelatedGroup != null)
            {
                var participants = new List<ShortUserInfo>();

                if (existedEvent.RelatedGroup.Participants != null)
                {
                    foreach (var participant in existedEvent.RelatedGroup.Participants)
                    {
                        var userInfo = new ShortUserInfo
                        {
                            UserEmail = participant.Email,
                            UserName = participant.UserName
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
                Manager = manager,
                Group = groupInfoResponse,
                Guests = listGuests
            };

            var response = new GetResponse();
            response.Result = true;
            response.OutInfo =
                $"Info about event with id = {eventId} has been received";
            response.RequestedInfo = eventInfo;

            var json = JsonConvert.SerializeObject(response);

            return Ok(response);
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
}

