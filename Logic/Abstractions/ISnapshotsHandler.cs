using Contracts.Request;
using Contracts.Response;
using Models.Enums;

namespace Logic.Abstractions; 

public interface ISnapshotsHandler
{
    public Task<SnapshotDescriptionResult> CreateSnapshotDescriptionAsync(
        int userId,
        SnapshotInputDTO inputSnapshot,
        CancellationToken token);

    public Task<GroupSnapshotDescriptionResult> CreateGroupKPISnapshotDescriptionAsync(
        int managerId,
        GroupSnapshotInputDTO inputSnapshot,
        CancellationToken token);
}
