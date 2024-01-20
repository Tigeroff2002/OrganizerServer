using Models;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UserInfoContent
{
    public required int UserId { get; init; }

    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string PhoneNumber { get; init; }

    public List<GroupInfoResponse> Groups { get; set; } = null!;

    public List<EventInfoResponse> Events { get; set; } = null!;

    public List<TaskInfoResponse> Tasks { get; set; } = null!;

    public List<SnapshotInfoResponse> Snapshots { get; set; } = null!;
}
