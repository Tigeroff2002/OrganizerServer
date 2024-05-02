using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IGroupsHandler
{
    public Task<Response> TryCreateGroup(
        string creationData,
        CancellationToken token);

    public Task<Response> TryUpdateGroup(
        string updateData,
        CancellationToken token);

    public Task<Response> TryDeleteParticipant(
        string deleteParticipantData,
        CancellationToken token);

    public Task<Response> TryDeleteGroup(
        string deleteGroupData,
        CancellationToken token);

    public Task<GetResponse> GetGroupInfo(
        string groupInfoById,
        CancellationToken token);

    public Task<GetResponse> GetGroupParticipantCalendar(
        string groupParticipantCalendarById,
        CancellationToken token);
}
