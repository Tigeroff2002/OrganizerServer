using Contracts.Request;
using Contracts.Request.RequestById;
using Contracts.Response;
using Logic.Abstractions;
using Models.BusinessModels;
using Models.Enums;
using Models.StorageModels;
using Newtonsoft.Json;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace Logic.ControllerHandlers;

public sealed class GroupsHandler : IGroupsHandler
{
    public GroupsHandler(ICommonUsersUnitOfWork commonUnitOfWork)
    {
        _commonUnitOfWork = commonUnitOfWork 
            ?? throw new ArgumentNullException(nameof(commonUnitOfWork));
    }

    public async Task<Response> TryCreateGroup(
        string creationData, 
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(creationData);

        token.ThrowIfCancellationRequested();

        var groupToCreate = JsonConvert.DeserializeObject<GroupInputDTO>(creationData);

        Debug.Assert(groupToCreate != null);

        var currentUserId = groupToCreate.UserId;

        var selfUser = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(currentUserId, token);

        if (selfUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Group has not been created" +
                $" cause current user with id {currentUserId} was not found";

            return await Task.FromResult(response1);
        }

        var group = new Group()
        {
            GroupName = groupToCreate.GroupName,
            Type = groupToCreate.Type
        };

        await _commonUnitOfWork.GroupsRepository.AddAsync(group, token);

        _commonUnitOfWork.SaveChanges();

        var groupId = group.Id;

        var listGroupingUsersMap = new List<GroupingUsersMap>();

        var existedUsers = await _commonUnitOfWork
            .UsersRepository
            .GetAllUsersAsync(token);

        if (groupToCreate.Type is GroupType.None)
        {
            foreach (var user in existedUsers)
            {
                if (user != null)
                {
                    var map = new GroupingUsersMap
                    {
                        UserId = user.Id,
                        GroupId = groupId,
                    };

                    listGroupingUsersMap.Add(map);
                }
            }
        }
        else
        {
            if (groupToCreate.Participants != null)
            {
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

                        listGroupingUsersMap.Add(map);
                    }
                }
            }
        }

        foreach (var map in listGroupingUsersMap)
        {
            await _commonUnitOfWork.GroupingUsersMapRepository.AddAsync(map, token);
        }

        _commonUnitOfWork.SaveChanges();

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New group with id = {groupId}" +
            $" and name {group.GroupName} was created";

        return await Task.FromResult(response);
    }

    public async Task<Response> TryUpdateGroup(
        string updateData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(updateData);

        token.ThrowIfCancellationRequested();

        var updateGroupParams = 
            JsonConvert.DeserializeObject<UpdateGroupParams>(updateData);

        Debug.Assert(updateGroupParams != null);

        var groupId = updateGroupParams.GroupId;
        var currentUserId = updateGroupParams.UserId;

        var currentUserFromRequest = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(currentUserId, token);

        if (currentUserFromRequest == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Group has not been modified cause" +
                $" current user with id {currentUserId} was not found";

            return await Task.FromResult(response1);
        }

        var existedGroup = await _commonUnitOfWork
            .GroupsRepository
            .GetGroupByIdAsync(groupId, token);

        if (existedGroup != null)
        {
            if (existedGroup.ParticipantsMap == null)
            {
                existedGroup.ParticipantsMap = new List<GroupingUsersMap>();
            }

            var existedMap = await _commonUnitOfWork
                .GroupingUsersMapRepository
                .GetGroupingUserMapByIdsAsync(groupId, currentUserId, token);

            if (existedMap == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Group has not been modified" +
                    $" cause user with id {currentUserId} not relate to that";

                return await Task.FromResult(response1);
            }

            var listGroupingUsersMap = new List<GroupingUsersMap>();

            if (updateGroupParams.Participants != null)
            {
                var existedUsers = await _commonUnitOfWork
                    .UsersRepository
                    .GetAllUsersAsync(token);

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

                        listGroupingUsersMap.Add(map);
                    }
                }

                foreach (var map in listGroupingUsersMap)
                {
                    await _commonUnitOfWork
                        .GroupingUsersMapRepository
                        .AddAsync(map, token);
                }
            }

            if (!string.IsNullOrWhiteSpace(updateGroupParams.GroupName))
            {
                existedGroup.GroupName = updateGroupParams.GroupName;
            }

            if (updateGroupParams.Type != GroupType.None)
            {
                existedGroup.Type = updateGroupParams.Type;
            }

            await _commonUnitOfWork
                .GroupsRepository
                .UpdateAsync(existedGroup, token);

            _commonUnitOfWork.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Group with id {groupId} has been modified";

            var json = JsonConvert.SerializeObject(response);

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {groupId}";

        return await Task.FromResult(response2);
    }

    public async Task<Response> TryDeleteParticipant(
        string deleteParticipantData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(deleteParticipantData);

        token.ThrowIfCancellationRequested();

        var groupDeleteParticipant = 
            JsonConvert.DeserializeObject<GroupDeleteParticipant>(
                deleteParticipantData);

        Debug.Assert(groupDeleteParticipant != null);

        var groupId = groupDeleteParticipant.GroupId;
        var participantId = groupDeleteParticipant.Participant_Id;

        var existedGroup = await _commonUnitOfWork
            .GroupsRepository
            .GetGroupByIdAsync(groupId, token);

        var requestParticipantId = groupDeleteParticipant.Participant_Id;

        var existedUser = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(requestParticipantId, token);

        if (existedUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Group has not been modified cause" +
                $" current participant with id {requestParticipantId} was not found";

            return await Task.FromResult(response1);
        }

        if (existedGroup != null)
        {
            if (existedUser.Id != existedGroup.ManagerId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Group has not been modified cause" +
                    $" delete participant request was made by user {existedUser.Id}" +
                    $" thats not manager of group {groupId} was not found";

                return await Task.FromResult(response1);
            }

            if (existedGroup.ParticipantsMap == null)
            {
                existedGroup.ParticipantsMap = new List<GroupingUsersMap>();
            }

            var existedUserMap = await _commonUnitOfWork
                .GroupingUsersMapRepository
                .GetGroupingUserMapByIdsAsync(groupId, existedUser.Id, token);

            if (existedUserMap == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Participants of group with id {groupId} " +
                    $"has not been modified cause user not relate to that";

                return await Task.FromResult(response1);
            }

            await _commonUnitOfWork
                .GroupingUsersMapRepository
                .DeleteAsync(groupId, participantId, token);

            _commonUnitOfWork.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo =
                $"Participant with id {groupDeleteParticipant.Participant_Id}" +
                $" has been deleted from group with id {groupDeleteParticipant.GroupId}";

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {groupDeleteParticipant.GroupId}";

        return await Task.FromResult(response2);
    }

    public async Task<GetResponse> GetGroupInfo(
        string groupInfoById,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupInfoById);

        token.ThrowIfCancellationRequested();

        var groupByIdRequest = 
            JsonConvert.DeserializeObject<GroupDetailsRequest>(
                groupInfoById);

        Debug.Assert(groupByIdRequest != null);

        var userId = groupByIdRequest.UserId;
        var groupId = groupByIdRequest.GroupId;

        var existedUser = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo =
                $"Group info has not been received cause" +
                $" current user with id {userId} was not found";

            return await Task.FromResult(response1);
        }

        var existedGroup = await _commonUnitOfWork
            .GroupsRepository
            .GetGroupByIdAsync(groupId, token);

        if (existedGroup != null)
        {
            if (existedGroup.ParticipantsMap == null)
            {
                existedGroup.ParticipantsMap = new List<GroupingUsersMap>();
            }

            var existedUserGroupMap = await _commonUnitOfWork
                .GroupingUsersMapRepository
                .GetGroupingUserMapByIdsAsync(groupId, userId, token);

            if (existedUserGroupMap == null)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Info about group with id {groupId}" +
                    $" was not accessed cause user with id {userId} not related to that";

                var json1 = JsonConvert.SerializeObject(response1);

                return await Task.FromResult(response1);
            }

            var existedMaps = await _commonUnitOfWork
                .GroupingUsersMapRepository
                .GetGroupingUsersMapByGroupIdsAsync(groupId, token);

            var listOfUsersInfo = new List<ShortUserInfo>();

            foreach (var userMap in existedMaps)
            {
                var participantId = userMap.UserId;

                var user = await _commonUnitOfWork
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

            return await Task.FromResult(getResponse);
        }

        var response2 = new GetResponse();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {groupByIdRequest.GroupId}";

        return await Task.FromResult(response2);

    }

    public async Task<GetResponse> GetGroupParticipantCalendar(
        string groupParticipantCalendarById,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupParticipantCalendarById);

        token.ThrowIfCancellationRequested();

        var participantCalendarRequest = 
            JsonConvert.DeserializeObject<AnotherUserCalendarRequest>(
                groupParticipantCalendarById);

        Debug.Assert(participantCalendarRequest != null);

        var userId = participantCalendarRequest.UserId;
        var groupId = participantCalendarRequest.GroupId;
        var participantId = participantCalendarRequest.ParticipantId;

        var existedGroup = await _commonUnitOfWork
            .GroupsRepository
            .GetGroupByIdAsync(groupId, token);

        if (existedGroup != null)
        {
            var existedUser = await _commonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

            if (existedUser != null)
            {
                var participant = await _commonUnitOfWork
                    .UsersRepository
                    .GetUserByIdAsync(participantId, token);

                if (participant != null)
                {
                    var allGroupMaps = await _commonUnitOfWork
                        .GroupingUsersMapRepository
                        .GetAllMapsAsync(token);

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

                            var allEventMaps = await _commonUnitOfWork
                                .EventsUsersMapRepository
                                .GetAllMapsAsync(token);

                            var participantEventsInGroup = allEventMaps
                                .Where(x => x.UserId == participantId)
                                .ToList();

                            foreach (var eventMap in participantEventsInGroup)
                            {
                                var currentEventId = eventMap.EventId;
                                var existedEvent = await _commonUnitOfWork
                                    .EventsRepository
                                    .GetEventByIdAsync(currentEventId, token);

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

                            return await Task.FromResult(getResponse);
                        }
                    }

                    var response5 = new GetResponse();
                    response5.Result = false;
                    response5.OutInfo = 
                        $"Info about participant {participantCalendarRequest.ParticipantId}" +
                        $" was forbidden to be received";

                    return await Task.FromResult(response5);
                }

                var response4 = new GetResponse();
                response4.Result = false;
                response4.OutInfo = 
                    $"No such participant with id" +
                    $" {participantCalendarRequest.ParticipantId}";

                return await Task.FromResult(response4);
            }

            var response3 = new GetResponse();
            response3.Result = false;
            response3.OutInfo = 
                $"No such user with id" +
                $" {participantCalendarRequest.UserId}";

            return await Task.FromResult(response3);
        }

        var response2 = new GetResponse();
        response2.Result = false;
        response2.OutInfo = $"No such group with id {participantCalendarRequest.GroupId}";

        return await Task.FromResult(response2);

    }

    private ICommonUsersUnitOfWork _commonUnitOfWork;
}
