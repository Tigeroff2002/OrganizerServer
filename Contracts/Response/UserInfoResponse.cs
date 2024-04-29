using Models;
using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("user_role", Required = Required.Always)]
    public required UserRole Role { get; init; }

    [JsonProperty("account_creation", Required = Required.Default)]
    public DateTimeOffset AccountCreationTime { get; set; }

    [JsonProperty("user_groups", NullValueHandling = NullValueHandling.Ignore)]
    public List<GroupInfoResponse> UserGroups { get; set; } = default!;

    [JsonProperty("user_tasks", NullValueHandling = NullValueHandling.Ignore)]
    public List<TaskInfoResponse> UserTasks { get; set; } = default!;

    [JsonProperty("user_events", NullValueHandling = NullValueHandling.Ignore)]
    public List<EventInfoResponse> UserEvents { get; set; } = default!;

    [JsonProperty("user_snapshots", NullValueHandling = NullValueHandling.Ignore)]
    public required List<PersonalSnapshotInfoResponse> UserSnapshots { get; set; } = default!;

    [JsonProperty("user_issues", NullValueHandling = NullValueHandling.Ignore)]
    public required List<IssueInfoResponse> UserIssues { get; set; } = default!;
}
