using Models;

namespace PostgreSQL.Abstractions;

public interface IEventsRepository : IRepository
{
    Task AddAsync(Event @event, CancellationToken token);

    Task<Event?> GetEventByIdAsync(int eventId, CancellationToken token);

    Task<List<Event>> GetAllEventsAsync(CancellationToken token);

    Task DeleteAsync(int eventId, CancellationToken token);

    Task UpdateAsync(Event @event, CancellationToken token);
}
