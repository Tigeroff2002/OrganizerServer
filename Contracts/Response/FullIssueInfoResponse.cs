using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class FullIssueInfoResponse : IssueInfoResponse
{
    [JsonProperty("user_name", Required = Required.Always)]
    public required string UserName { get; init; }
}
