﻿using Models.Enums;

namespace Contracts.Response;

public sealed class UserInfoContent
{
    public required int UserId { get; init; }

    public required UserRole Role { get; init; }

    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string PhoneNumber { get; init; }

    public DateTimeOffset AccountCreationTime { get; set; }

    public List<GroupInfoResponse> Groups { get; set; } = null!;

    public List<EventInfoResponse> Events { get; set; } = null!;

    public List<TaskInfoResponse> Tasks { get; set; } = null!;

    public List<PersonalSnapshotInfoResponse> Snapshots { get; set; } = null!;

    public List<IssueInfoResponse> Issues { get; set; } = null!;
}
