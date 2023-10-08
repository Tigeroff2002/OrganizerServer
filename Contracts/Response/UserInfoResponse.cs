using Models;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UserInfoResponse
{
    [JsonProperty("user_id", Required = Required.Always)]
    public required int UserId { get; init; }

    [JsonProperty("user_name", Required = Required.Always)]
    public required string UserName { get; init; }

    [JsonProperty("user_email", Required = Required.Always)]
    public required string UserEmail { get; init; }

    [JsonProperty("user_token", Required = Required.Always)]
    public required string UserToken { get; init; }

    [JsonProperty("user_groups", Required = Required.Default)]
    public List<GroupInfoResponse> UserGroups { get; set; } = default!;

    [JsonProperty("user_tasks", Required = Required.Default)]
    public List<TaskInfoResponse> UserTasks { get; set; } = default!;

    [JsonProperty("user_events", Required = Required.Default)]
    public List<EventInfoResponse> UserEvents { get; set; } = default!;

    [JsonProperty("user_reports", Required = Required.Default)]
    public required List<ReportInfoResponse> UserReports { get; set; } = default!;
}
