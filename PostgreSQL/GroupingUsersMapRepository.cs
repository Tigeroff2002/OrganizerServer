using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class GroupingUsersMapRepository
    : IGroupingUsersMapRepository
{
    public GroupingUsersMapRepository(
        IServiceProvider provider,
        ILogger<GroupingUsersMapRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Grouping users map repository was created just now");
    }

    public async Task AddAsync(GroupingUsersMap map, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(map);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var existedMap = await _repositoryContext.GroupingUsersMaps
            .FirstOrDefaultAsync(x => x.UserId == map.UserId && x.GroupId == map.GroupId);

        if (existedMap == null)
        {
            await _repositoryContext.GroupingUsersMaps.AddAsync(map, token);
        }

        SaveChanges();
    }

    public async Task<GroupingUsersMap?> GetGroupingUserMapByIdsAsync(
        int groupId,
        int userId, 
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return await _repositoryContext.GroupingUsersMaps
            .FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == groupId);
    }

    public async Task<List<GroupingUsersMap>> GetAllMapsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return await _repositoryContext.GroupingUsersMaps.ToListAsync();
    }

    public async Task DeleteAsync(int groupId, int userId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var map = await _repositoryContext.GroupingUsersMaps
            .FirstOrDefaultAsync(
                x => x.GroupId == groupId
                    && x.UserId == userId);

        if (map != null)
        {
            _repositoryContext.GroupingUsersMaps.Entry(map).State = EntityState.Deleted;
        }

        SaveChanges();
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of grouping users maps were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
