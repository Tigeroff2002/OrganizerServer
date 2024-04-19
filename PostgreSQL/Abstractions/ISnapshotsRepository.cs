using Models.StorageModels;

namespace PostgreSQL.Abstractions; 

public interface ISnapshotsRepository : IRepository
{
    Task AddAsync(Snapshot snapshot, CancellationToken token);

    Task<Snapshot?> GetSnapshotByIdAsync(int snapshotId, CancellationToken token);

    Task<List<Snapshot>> GetAllSnapshotsAsync(CancellationToken token);

    Task DeleteAsync(int snapshotId, CancellationToken token);
}
