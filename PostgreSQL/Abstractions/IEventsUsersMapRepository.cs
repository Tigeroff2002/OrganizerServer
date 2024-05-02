using Models;
using Models.Enums;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface IEventsUsersMapRepository : IRepository
{
    Task AddAsync(EventsUsersMap map, CancellationToken token);

    Task<EventsUsersMap?> GetEventUserMapByIdsAsync(
        int eventId,
        int userId,
        CancellationToken token);

    Task<List<EventsUsersMap>> GetAllMapsAsync(CancellationToken token);

    Task UpdateDecisionAsync(
        int eventId, 
        int userId, 
        DecisionType decisionType, 
        CancellationToken token);

    Task DeleteAsync(int eventId, int userId, CancellationToken token);
}
