using Models.RedisEventModels.IssueEvents;

namespace Models.RedisEventModels.MeetEvents;

public sealed record class MeetParamsChangedEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int MeetId,
    DateTimeOffset UpdateMoment,
    string Json)
    : MeetBasedEvent(Id, IsCommited, UserId, MeetId);
