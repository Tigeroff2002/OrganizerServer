﻿using Models.Enums;

namespace Models.RedisEventModels.UserEvents;

public sealed record class UserRoleChangedEvent(
    int Id,
    bool IsCommited,
    int UserId,
    DateTime UpdateMoment,
    UserRole NewRole)
    : UserRelatedEvent(Id, IsCommited, UserId);
