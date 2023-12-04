using Models;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UserInfoResponse
{
    [JsonProperty("user_id", Required = Required.Default)]
    public required int UserId { get; init; }

    [JsonProperty("user_name", Required = Required.Always)]
    public required string UserName { get; init; }

    [JsonProperty("password", Required = Required.Always)]
    public required string Password { get; init; }

    [JsonProperty("user_email", Required = Required.Always)]
    public required string UserEmail { get; init; }

    [JsonProperty("phone_number", Required = Required.Always)]
    public required string PhoneNumber { get; init; }

    [JsonProperty("user_groups", NullValueHandling = NullValueHandling.Ignore)]
    public List<GroupInfoResponse> UserGroups { get; set; } = default!;

    [JsonProperty("user_tasks", NullValueHandling = NullValueHandling.Ignore)]
    public List<TaskInfoResponse> UserTasks { get; set; } = default!;

    [JsonProperty("user_events", NullValueHandling = NullValueHandling.Ignore)]
    public List<EventInfoResponse> UserEvents { get; set; } = default!;

    [JsonProperty("user_reports", NullValueHandling = NullValueHandling.Ignore)]
    public required List<ReportInfoResponse> UserReports { get; set; } = default!;
}
