using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IEventsHandler
{
    public Task<Response> TryScheduleEvent(
        string schedulingData,
        CancellationToken token);

    public Task<Response> TryUpdateEvent(
        string updateData,
        CancellationToken token);

    public Task<Response> TryUpdateUserDecision(
        string updateDecisionData,
        CancellationToken token);

    public Task<Response> TryDeleteEvent(
        string deleteData,
        CancellationToken token);

    public Task<GetResponse> GetEventInfo(
        string eventInfoById,
        CancellationToken token);
}
