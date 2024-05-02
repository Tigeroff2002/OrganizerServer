using Logic.Abstractions;
using Models.RedisEventModels;
using Models.RedisEventModels.AlertEvents;
using Models.RedisEventModels.GroupEvents;
using Models.RedisEventModels.IssueEvents;
using Models.RedisEventModels.MeetEvents;
using Models.RedisEventModels.SnapshotEvents;
using Models.RedisEventModels.TaskEvents;
using Models.RedisEventModels.UserEvents;

namespace Logic;

public sealed class RedisEventsAliaser : IRedisEventsAliaser
{
    public string GetAliasForEvent(UserRelatedEvent redisEvent)
    {
        ArgumentNullException.ThrowIfNull(redisEvent);

        return redisEvent switch
        {
            _ when redisEvent is AlertCreatedEvent alertCreated
                => $"Alert with id = {alertCreated.AlertId}," +
                $" text = '{alertCreated.Json}'" +
                $" was created in server at moment: {alertCreated.CreateMoment}",

            _ when redisEvent is GroupCreatedEvent groupCreated
                => $"Group with id = {groupCreated.Id} was created by you" +
                $" at moment: {groupCreated.CreatedMoment}",

            _ when redisEvent is GroupParamsChangedEvent groupChanged
                => $"Group with id = {groupChanged.Id} was modified by you" +
                $" at moment: {groupChanged.UpdateMoment}." +
                $" New group fixture: '{groupChanged.Json}'",

            _ when redisEvent is GroupParticipantDeletedEvent groupParticipantDeleted
                => $"You were deleted from group" +
                $" {groupParticipantDeleted.Id} by manager.",

            _ when redisEvent is GroupParticipantInvitedEvent groupParticipantInvited
                => $"You were invited to group" +
                $" {groupParticipantInvited.GroupId} by manager.",

            _ when redisEvent is IssueCreatedEvent issueCreated
                => $"Issue with id = {issueCreated.Id} was created by you" +
                $"at moment: {issueCreated.CreatedMoment}",

            _ when redisEvent is IssueParamsChangedEvent issueChanged
                => $"Issue with id = {issueChanged.IssueId} was modified by you" +
                $" at moment: {issueChanged.UpdateMoment}." +
                $" New issue fixture: '{issueChanged.Json}'",

            _ when redisEvent is IssueTerminalStatusReceivedEvent issueTermnated
                => $"Issue with id = {issueTermnated.IssueId} was terminated" +
                $" to status {issueTermnated.TerminalStatus}" +
                $" at moment: {issueTermnated.TerminalMoment}",

            _ when redisEvent is MeetCreatedEvent meetCreated
                => $"Meet with id = {meetCreated.MeetId} was created" +
                $" at moment: {meetCreated.CreatedMoment}." +
                $"Scheduled start: {meetCreated.ScheduledStart}," +
                $" duration: {meetCreated.Duration}",

            _ when redisEvent is MeetGuestDeletedEvent meetGuestDeleted
                => $"You were deleted from guests of" +
                $" event with id {meetGuestDeleted.MeetId}",

            _ when redisEvent is MeetGuestInvitedEvent meetGuestInvited
                => $"You were invited to guests of" +
                $" event with id {meetGuestInvited.MeetId}",

            _ when redisEvent is MeetParamsChangedEvent meetChanged
                => $"Event with id = {meetChanged.MeetId} was modified by manager" +
                $" at moment: {meetChanged.UpdateMoment}." +
                $" New event fixture: '{meetChanged.Json}'",

            _ when redisEvent is MeetTerminalStatusReceivedEvent meetTerminated
                => $"Meet with id = {meetTerminated.MeetId} was terminated" +
                $" to status {meetTerminated.TerminalStatus}" +
                $" at moment: {meetTerminated.TerminalMoment}",

            _ when redisEvent is MeetSoonBeginEvent meetSoonBegin
                => $"Meet with id = {meetSoonBegin.MeetId} is soon begin." +
                $" {meetSoonBegin.RemainingTime} remaining to its" +
                $" scheduled start at: {meetSoonBegin.ScheduledStart}",

            _ when redisEvent is TaskAssignedEvent taskAssigned
                => $"Task with id = {taskAssigned.TaskId}" +
                $" was assigned for implementation to you",

            _ when redisEvent is TaskUnassignedEvent taskUnassigned
                => $"Task with id = {taskUnassigned.TaskId} " +
                $"was unassigned from your implementation",

            _ when redisEvent is TaskCreatedEvent taskCreated
                => $"Task with id = {taskCreated.TaskId} was created by you" +
                $" at moment: {taskCreated.CreatedMoment}",

            _ when redisEvent is TaskParamsChangedEvent taskChanged
                => $"Task with id = {taskChanged.TaskId} was modified" +
                $" at moment: {taskChanged.UpdateMoment}." +
                $"New task fixture: '{taskChanged.Json}'",

            _ when redisEvent is TaskTerminalStatusReceivedEvent taskTerminated
                => $"Task with id = {taskTerminated.TaskId} was terminated" +
                $" to status {taskTerminated.TerminalStatus}" +
                $" at moment: {taskTerminated.TerminalMoment}",

            _ when redisEvent is UserInfoUpdateEvent userInfoUpdated
                => $"Your profile info was updated at moment: {userInfoUpdated.UpdateMoment}." +
                $"New profile info fixture: '{userInfoUpdated.Json}'",

            _ when redisEvent is UserRegistrationEvent userRegistered
                => $"You were sucessfully registered" +
                $" at moment: {userRegistered.AccountCreationMoment}." +
                $" Your app token: {userRegistered.FirebaseToken}",

            _ when redisEvent is UserLoginEvent userLogin
                => $"You were succesfully loged in" +
                $" at moment: {userLogin.TokenSetMoment}." +
                $" Your app token: {userLogin.FirebaseToken}",

            _ when redisEvent is UserLogoutEvent userLogout
                => $"You were sucesfully logged out at moment: {DateTimeOffset.UtcNow}",

            _ when redisEvent is UserRoleChangedEvent userRoleChanged
                => $"You successfully received new role: {userRoleChanged.NewRole}" +
                $" at moment: {userRoleChanged.UpdateMoment}",

            _ when redisEvent is SnapshotCreatedEvent snapshotCreated
                => $"Snapshot with id = {snapshotCreated.SnapshotId}" +
                $" with audit type: {snapshotCreated.AuditType}" +
                $" was created by you at moment: {snapshotCreated.CreateMoment}",

            _ => throw new InvalidOperationException(
                $"Unsupported type for event: '{redisEvent.GetType().Name}'")
        };
    }
}
