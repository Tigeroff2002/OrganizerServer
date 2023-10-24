using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Models.Enums;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class EventsUsersMapRepository
    : IEventsUsersMapRepository
{
    public EventsUsersMapRepository(
        IServiceProvider provider,
        ILogger<EventsRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Events users map repository was created just now");
    }

    public async Task AddAsync(EventsUsersMap map, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(map);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var existedMap = await _repositoryContext.EventsUsersMaps
            .FirstOrDefaultAsync(x => x.UserId == map.UserId && x.EventId == map.EventId);

        if (existedMap == null)
        {
            await _repositoryContext.EventsUsersMaps.AddAsync(map, token);
        }

        SaveChanges();
    }

    public async Task DeleteAsync(int eventId, int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var map = await _repositoryContext.EventsUsersMaps
            .FirstOrDefaultAsync(
                x => x.EventId == eventId 
                    && x.UserId == userId);

        if (map != null)
        {
            _repositoryContext.EventsUsersMaps.Entry(map).State = EntityState.Deleted;
        }

        SaveChanges();
    }

    public async Task UpdateDecisionAsync(
        int eventId, 
        int userId,
        DecisionType decisionType, 
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var localMap = await _repositoryContext.EventsUsersMaps
            .FirstOrDefaultAsync(
                x => x.EventId == eventId
                    && x.UserId == userId);

        if (localMap != null)
        {
            localMap.DecisionType = decisionType;
        }

        SaveChanges();
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of events users maps were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
