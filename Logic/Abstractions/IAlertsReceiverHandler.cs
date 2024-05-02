using Models.BusinessModels;

namespace Logic.Abstractions;

public interface IAlertsReceiverHandler
{
    public Task<GetResponse> GetAllAlerts(
        string allAlertsDTO,
        CancellationToken token);
}
