namespace Models.RedisEventModels.SnapshotEvents;

public sealed record class SnapshotCreatedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int SnapshotId,
    DateTimeOffset CreateMoment)
    : UserRelatedEvent(Id, IsCommited, UserId);
