using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Models.StorageModels;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class SnapshotsRepository
    : ISnapshotsRepository
{
    public SnapshotsRepository(
        IServiceProvider provider,
        ILogger<SnapshotsRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Snapshots repository was created just now");
    }

    public async Task AddAsync(Snapshot snapshot, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        token.ThrowIfCancellationRequested();

        await _repositoryContext.Snapshots.AddAsync(snapshot, token);
    }

    public async Task<Snapshot?> GetSnapshotByIdAsync(int snapshotId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Snapshots
            .FirstOrDefaultAsync(x => x.Id == snapshotId);
    }

    public async Task<List<Snapshot>> GetAllSnapshotsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Snapshots.ToListAsync(token);
    }

    public async Task DeleteAsync(int snapshotId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var snapshot = await _repositoryContext.Snapshots
            .FirstOrDefaultAsync(x => x.Id == snapshotId);

        if (snapshot != null)
        {
            _repositoryContext.Snapshots.Entry(snapshot).State = EntityState.Deleted;
        }
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of snapshots were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
