using Models.Enums;

namespace Models.RedisEventModels.MeetEvents;

public sealed record class MeetGuestChangedDecisionEvent(
    string Id, 
    bool IsCommited,
    int UserId,
    int MeetId,
    DecisionType NewDecision,
    DateTimeOffset ScheduledStart) 
    : MeetBasedEvent(Id, IsCommited, UserId, MeetId);
