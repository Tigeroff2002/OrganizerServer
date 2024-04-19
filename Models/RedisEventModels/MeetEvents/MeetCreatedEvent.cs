using Models.RedisEventModels.IssueEvents;

namespace Models.RedisEventModels.MeetEvents;

public sealed record class MeetCreatedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    int MeetId,
    DateTimeOffset CreatedMoment,
    DateTimeOffset ScheduledStart,
    TimeSpan Duration)
    : MeetBasedEvent(Id, IsCommited, UserId, MeetId);
