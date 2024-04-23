namespace Models.RedisEventModels.SnapshotEvents;

public sealed record class SnapshotCreatedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int SnapshotId,
    DateTimeOffset CreateMoment)
    : UserRelatedEvent(Id, IsCommited, UserId);
