using Models.Enums;

namespace Models.RedisEventModels.SnapshotEvents;

public sealed record class SnapshotCreatedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int SnapshotId,
    SnapshotAuditType AuditType,
    DateTimeOffset CreateMoment)
    : UserRelatedEvent(Id, IsCommited, UserId);
