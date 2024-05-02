using Contracts.Request;
using Contracts.Request.RequestById;
using Contracts.Response;
using Models.Enums;

namespace Logic.Abstractions; 

public interface ISnapshotsHandler
{
    public Task<SnapshotDescriptionResult> CreatePersonalSnapshotDescriptionAsync(
        int userId,
        SnapshotInputDTO inputSnapshot,
        CancellationToken token);

    public Task<GroupSnapshotDescriptionResult> CreateGroupKPISnapshotDescriptionAsync(
        int managerId,
        GroupSnapshotInputDTO inputSnapshot,
        CancellationToken token);
}
