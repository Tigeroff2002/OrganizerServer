using Models;
using Models.Enums;

namespace PostgreSQL.Abstractions;

public interface IEventsUsersMapRepository : IRepository
{
    Task AddAsync(EventsUsersMap map, CancellationToken token);

    Task UpdateDecisionAsync(
        int eventId, 
        int userId, 
        DecisionType decisionType, 
        CancellationToken token);

    Task DeleteAsync(int eventId, int userId, CancellationToken token);
}
