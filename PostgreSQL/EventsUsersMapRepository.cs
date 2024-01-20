using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Models.Enums;
using PostgreSQL.Abstractions;
using System.Text.RegularExpressions;

namespace PostgreSQL;

public sealed class EventsUsersMapRepository
    : IEventsUsersMapRepository
{
    public EventsUsersMapRepository(
        IServiceProvider provider,
        ILogger<EventsUsersMapRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Events users map repository was created just now");
    }

    public async Task AddAsync(EventsUsersMap map, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(map);

        token.ThrowIfCancellationRequested();

        var existedMap = await _repositoryContext.EventsUsersMaps
            .FirstOrDefaultAsync(x => x.UserId == map.UserId && x.EventId == map.EventId);

        if (existedMap == null)
        {
            await _repositoryContext.EventsUsersMaps.AddAsync(map, token);
        }
    }

    public async Task DeleteAsync(int eventId, int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var map = await _repositoryContext.EventsUsersMaps
            .FirstOrDefaultAsync(
                x => x.EventId == eventId 
                    && x.UserId == userId);

        if (map != null)
        {
            _repositoryContext.EventsUsersMaps.Entry(map).State = EntityState.Deleted;
        }
    }

    public async Task<EventsUsersMap?> GetEventUserMapByIdsAsync(int eventId, int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.EventsUsersMaps
            .FirstOrDefaultAsync(x => x.UserId == userId && x.EventId == eventId);
    }

    public async Task<List<EventsUsersMap>> GetAllMapsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.EventsUsersMaps.ToListAsync();
    }

    public async Task UpdateDecisionAsync(
        int eventId, 
        int userId,
        DecisionType decisionType, 
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var localMap = await _repositoryContext.EventsUsersMaps
            .FirstOrDefaultAsync(
                x => x.EventId == eventId
                    && x.UserId == userId);

        if (localMap != null)
        {
            localMap.DecisionType = decisionType;
        }
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of events users maps were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
