using Contracts;
using Contracts.Request.RequestById;
using Contracts.Response;
using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Models.BusinessModels;
using Models.Enums;
using Models.StorageModels;
using Newtonsoft.Json;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace Logic.ControllerHandlers;

public sealed class EventsHandler 
    : DataHandlerBase, IEventsHandler
{
    public EventsHandler(
        ICommonUsersUnitOfWork commonUnitOfWork,
        IRedisRepository redisRepository,
        ILogger<EventsHandler> logger)
        : base(commonUnitOfWork, redisRepository, logger)
    {
    }

    public async Task<Response> TryScheduleEvent(
        string schedulingData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(schedulingData);

        token.ThrowIfCancellationRequested();

        var eventToCreate = 
            JsonConvert.DeserializeObject<EventInputDTO>(schedulingData);
        Debug.Assert(eventToCreate != null);

        var managerId = eventToCreate.UserId;
        var groupId = eventToCreate.GroupId;

        var manager = await CommonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(managerId, token);

        if (manager == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Event has not been scheduled cause" +
                $" current user with id {managerId} was not found";

            return await Task.FromResult(response1);
        }

        var group = await CommonUnitOfWork
            .GroupsRepository
            .GetGroupByIdAsync(groupId, token);

        if (group == null)
        {
            if (eventToCreate.EventType != EventType.Personal)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event has not been scheduled cause related" +
                    $" group with id {groupId} was not found";

                return await Task.FromResult(response1);
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

        await CommonUnitOfWork.EventsRepository.AddAsync(@event, token);

        CommonUnitOfWork.SaveChanges();

        var eventId = @event.Id;
        @event.Manager = manager;
        @event.RelatedGroup = group!;

        var listGuestsMaps = new List<EventsUsersMap>();

        var existedUsers = await CommonUnitOfWork
            .UsersRepository
            .GetAllUsersAsync(token);

        var managerMap = new EventsUsersMap
        {
            UserId = manager.Id,
            DecisionType = DecisionType.Apply,
            EventId = eventId,
        };

        if (eventToCreate.EventType 
            is EventType.Meeting or EventType.StandUp)
        {
            var allMaps = await CommonUnitOfWork
                .GroupingUsersMapRepository
                .GetAllMapsAsync(token);

            var usersInGroup = allMaps.Where(x => x.GroupId == groupId);

            foreach (var userMap in usersInGroup)
            {
                var currentUserId = userMap.UserId;

                var currentUser = existedUsers.FirstOrDefault(x => x.Id == currentUserId);

                if (currentUser != null)
                {
                    var map = new EventsUsersMap
                    {
                        UserId = currentUserId,
                        DecisionType = DecisionType.Default,
                        EventId = eventId
                    };

                    listGuestsMaps.Add(map);
                }
            }
        }
        else if (eventToCreate.EventType == EventType.Personal)
        {
            listGuestsMaps.Add(managerMap);
        }
        else
        {
            if (eventToCreate.GuestsIds != null)
            {
                foreach (var userId in eventToCreate.GuestsIds)
                {
                    var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                    if (currentUser != null)
                    {
                        var map = new EventsUsersMap
                        {
                            UserId = userId,
                            DecisionType = DecisionType.Default,
                            EventId = eventId
                        };

                        listGuestsMaps.Add(map);
                    }
                }
            }
        }

        if (listGuestsMaps.FirstOrDefault(x => x.UserId == manager.Id) == null)
        {
            listGuestsMaps.Add(managerMap);
        }

        foreach (var map in listGuestsMaps)
        {
            await CommonUnitOfWork
                .EventsUsersMapRepository
                .AddAsync(map, token);
        }

        CommonUnitOfWork.SaveChanges();

        var response = new Response();
        response.Result = true;
        response.OutInfo = group != null
            ? $"New event with id = {eventId} related to group {groupId}" +
                $" with name {group.GroupName} has been created"
            : $"New event with id = {eventId} personal" +
                $" for manager with id {managerId} has been created";

        return await Task.FromResult(response);
    }

    public async Task<Response> TryUpdateEvent(
        string updateData, 
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(updateData);

        token.ThrowIfCancellationRequested();

        var updateEventParams = 
            JsonConvert.DeserializeObject<EventInputWithIdDTO>(updateData);

        Debug.Assert(updateEventParams != null);

        var eventId = updateEventParams.EventId;

        var existedEvent = await CommonUnitOfWork
            .EventsRepository
            .GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            var userId = updateEventParams.UserId;

            var user = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

            if (user == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event has not been modified cause" +
                    $" current user with id {userId} was not found";

                return await Task.FromResult(response1);
            }

            var managerId = existedEvent.ManagerId;

            var manager = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);


            if (existedEvent.Manager.Id != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event has not been modified cause current user with id {userId}" +
                    $" is not manager: 'managerId = {existedEvent.Manager.Id}' of this event";

                return await Task.FromResult(response1);
            }

            if (updateEventParams.GuestsIds != null)
            {
                var listGuestsMap = new List<EventsUsersMap>();

                var existedUsers = await CommonUnitOfWork
                    .UsersRepository
                    .GetAllUsersAsync(token);

                foreach (var guestId in updateEventParams.GuestsIds)
                {
                    var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                    if (currentUser != null)
                    {
                        var map = new EventsUsersMap
                        {
                            UserId = guestId,
                            EventId = eventId,
                            DecisionType = DecisionType.Default
                        };

                        listGuestsMap.Add(map);
                    }
                }

                if (existedEvent.EventType != EventType.Personal)
                {
                    foreach (var map in listGuestsMap)
                    {
                        await CommonUnitOfWork
                            .EventsUsersMapRepository
                            .AddAsync(map, token);
                    }
                }
            }

            var response = new Response();

            var numbers_of_new_params = 0;

            if (!string.IsNullOrWhiteSpace(updateEventParams.Caption))
            {
                existedEvent.Caption = updateEventParams.Caption;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(updateEventParams.Description))
            {
                existedEvent.Description = updateEventParams.Description;
                numbers_of_new_params++;
            }

            if (updateEventParams.ScheduledStart != DateTimeOffset.MinValue)
            {
                existedEvent.ScheduledStart = updateEventParams.ScheduledStart;
                numbers_of_new_params++;
            }

            if (updateEventParams.Duration != TimeSpan.Zero)
            {
                existedEvent.Duration = updateEventParams.Duration;
                numbers_of_new_params++;
            }

            if (updateEventParams.EventType != EventType.None)
            {
                existedEvent.EventType = updateEventParams.EventType;
                numbers_of_new_params++;
            }

            if (updateEventParams.EventStatus != EventStatus.None)
            {
                existedEvent.Status = updateEventParams.EventStatus;
                numbers_of_new_params++;
            }

            if (numbers_of_new_params > 0)
            {
                await CommonUnitOfWork
                    .EventsRepository
                    .UpdateAsync(existedEvent, token);

                response.OutInfo = existedEvent.RelatedGroup != null
                    ? $"Existed event with id = {eventId} related" +
                        $" to group {existedEvent.RelatedGroup.Id} has been modified"
                    : $"Existed event with id = {eventId} personal for manager " +
                        $"with id {existedEvent.Manager.Id} has been modified";
            }
            else
            {
                response.OutInfo =
                    $"Existed event with id {eventId} has all same parameters" +
                    $" so it has not been modified";
            }

            CommonUnitOfWork.SaveChanges();

            response.Result = true;

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such event with id {eventId}";

        return await Task.FromResult(response2);
    }

    public async Task<Response> TryUpdateUserDecision(
        string updateDecisionData, 
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(updateDecisionData);

        token.ThrowIfCancellationRequested();

        var updateEventParams = 
            JsonConvert.DeserializeObject<EventUserDecisionDTO>(
                updateDecisionData);

        Debug.Assert(updateEventParams != null);

        var eventId = updateEventParams.EventId;

        var existedEvent = await CommonUnitOfWork
            .EventsRepository
            .GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            var userId = updateEventParams.UserId;

            var user = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

            if (user == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event has not been modified cause" +
                    $" current user with id {userId} was not found";

                return await Task.FromResult(response1);
            }

            var existedUserEventMap = await CommonUnitOfWork
                .EventsUsersMapRepository
                .GetEventUserMapByIdsAsync(eventId, userId, token);

            if (existedUserEventMap == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"User decision for event {eventId} has not been modified" +
                    $" cause user with id {userId} not relate to that";

                return await Task.FromResult(response1);
            }

            var decisionType = updateEventParams.DecisionType;

            if (decisionType != DecisionType.None)
            {
                await CommonUnitOfWork
                    .EventsUsersMapRepository
                    .UpdateDecisionAsync(eventId, userId, decisionType, token);
            }

            CommonUnitOfWork.SaveChanges();

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

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such event with id {eventId}";

        return await Task.FromResult(response2);
    }

    public async Task<Response> TryDeleteEvent(
        string deleteData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(deleteData);

        token.ThrowIfCancellationRequested();

        var eventDeleteParams = 
            JsonConvert.DeserializeObject<EventIdDTO>(deleteData);

        Debug.Assert(eventDeleteParams != null);

        var eventId = eventDeleteParams.EventId;

        var existedEvent = await CommonUnitOfWork
            .EventsRepository
            .GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            var userId = eventDeleteParams.UserId;
            var managerId = existedEvent.Manager.Id;

            var user = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

            if (user == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event has not been modified cause" +
                    $" current user with id {userId} was not found";

                return await Task.FromResult(response1);
            }

            if (managerId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Event has not been modified" +
                    $" cause current user with id {userId}" +
                    $" is not manager: 'managerId = {managerId}' of this event";

                return await Task.FromResult(response1);
            }

            await CommonUnitOfWork.EventsRepository.DeleteAsync(eventId, token);

            CommonUnitOfWork.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Event with id = {eventId} has been deleted";

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such event with id {eventId}";

        return await Task.FromResult(response2);
    }

    public async Task<GetResponse> GetEventInfo(
        string eventInfoById,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(eventInfoById);

        token.ThrowIfCancellationRequested();

        var eventByIdRequest = 
            JsonConvert.DeserializeObject<EventIdDTO>(eventInfoById);

        Debug.Assert(eventByIdRequest != null);

        var userId = eventByIdRequest.UserId;
        var eventId = eventByIdRequest.EventId;

        var existedEvent = await CommonUnitOfWork
            .EventsRepository
            .GetEventByIdAsync(eventId, token);

        if (existedEvent != null)
        {
            if (existedEvent.GuestsMap == null)
            {
                existedEvent.GuestsMap = new List<EventsUsersMap>();
            }

            var existedUserEventMap = await CommonUnitOfWork
                .EventsUsersMapRepository
                .GetEventUserMapByIdsAsync(eventId, userId, token);

            if (existedUserEventMap == null)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Info about event with id {eventId}" +
                    $" was not accessed" +
                    $" cause user with id {userId} not related to that";

                return await Task.FromResult(response1);
            }

            var allEventMaps = await CommonUnitOfWork
                .EventsUsersMapRepository
                .GetAllMapsAsync(token);

            var existedEventMaps = allEventMaps.Where(x => x.EventId == eventId).ToList();

            var listOfUsersInfo = new List<ShortUserInfo>();

            foreach (var userMap in existedEventMaps)
            {
                var participantId = userMap.UserId;

                var user = await CommonUnitOfWork
                    .UsersRepository
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

            var relatedGroupId = existedEvent.RelatedGroupId;

            var relatedGroup = await CommonUnitOfWork
                .GroupsRepository
                .GetGroupByIdAsync(relatedGroupId, token);

            var groupParticipants = new List<ShortUserInfo>();

            if (relatedGroup != null)
            {
                var existedGroupMaps = await CommonUnitOfWork
                    .GroupingUsersMapRepository
                    .GetGroupingUsersMapByGroupIdsAsync(relatedGroupId, token);

                foreach (var userMap in existedGroupMaps)
                {
                    var participantId = userMap.UserId;

                    var user = await CommonUnitOfWork
                        .UsersRepository
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

                        groupParticipants.Add(shortUserInfo);
                    }
                }
            }

            var groupContent = new GroupContent
            {
                Participants = groupParticipants
            };

            var serializedGroupContent = groupContent;

            var eventContent = new EventContent
            {
                Guests = listOfUsersInfo,
                Caption = existedEvent.Caption,
                Description = existedEvent.Description,
                ScheduledStart = existedEvent.ScheduledStart,
                Duration = existedEvent.Duration,
                EventType = existedEvent.EventType,
                EventStatus = existedEvent.Status,
                GroupId = relatedGroupId,
                GroupContent = serializedGroupContent
            };

            var content = JsonConvert.SerializeObject(eventContent);

            var eventInfo = new EventInfoResponse
            {
                Caption = existedEvent.Caption,
                Description = existedEvent.Description,
                ScheduledStart = existedEvent.ScheduledStart,
                Duration = existedEvent.Duration,
                EventType = existedEvent.EventType,
                EventStatus = existedEvent.Status,
                EventId = existedEvent.Id,
                Content = content
            };

            var getResponse = new GetResponse();
            getResponse.Result = true;
            getResponse.OutInfo = 
                $"Info about event with id" +
                $" {existedEvent.Id} was found";
            getResponse.RequestedInfo = eventInfo;

            return await Task.FromResult(getResponse);
        }

        var response2 = new GetResponse();
        response2.Result = false;
        response2.OutInfo = $"No such event with id" +
            $" {eventByIdRequest.EventId}";

        return await Task.FromResult(response2);
    }
}
