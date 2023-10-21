using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class EventsRepository
    : IEventsRepository
{
    public EventsRepository(
        IServiceProvider provider,
        ILogger<EventsRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Events repository was created just now");
    }

    public async Task AddAsync(Event @event, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(@event);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        await _repositoryContext.Events.AddAsync(@event, token);

        SaveChanges();
    }

    public async Task<Event?> GetEventByIdAsync(int eventId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return await _repositoryContext.Events.FirstOrDefaultAsync(x => x.Id == eventId);
    }

    public Task<List<Event>> GetAllEventsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return _repositoryContext.Events.ToListAsync(token);
    }

    public async Task DeleteAsync(int eventId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var @event = await _repositoryContext.Events.FirstOrDefaultAsync(x => x.Id == eventId);

        if (@event != null)
        {
            _repositoryContext.Events.Entry(@event).State = EntityState.Deleted;
        }

        SaveChanges();
    }

    public async Task UpdateAsync(Event @event, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(@event);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var localEvent = await _repositoryContext.Events.FirstOrDefaultAsync(x => x.Id == @event.Id);

        if (localEvent != null)
        {
            _repositoryContext.Events.Entry(localEvent).CurrentValues.SetValues(@event);
        }

        localEvent = @event.Map<Event>();

        SaveChanges();
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of events were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
