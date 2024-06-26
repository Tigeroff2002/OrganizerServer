﻿using Models.RedisEventModels.GroupEvents;

namespace Models.RedisEventModels.MeetEvents;

public sealed record class MeetSoonBeginEvent(
    string Id,
    bool IsCommited,
    int UserId,
    int MeetId,
    TimeSpan RemainingTime,
    DateTimeOffset ScheduledStart)
    : MeetBasedEvent(Id, IsCommited, UserId, MeetId);
