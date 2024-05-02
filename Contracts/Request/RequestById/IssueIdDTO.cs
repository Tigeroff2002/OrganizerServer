using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class IssueIdDTO : RequestWithToken
{
    [JsonProperty("issue_id", Required = Required.Always)]
    public required int IssueId { get; init; }
}
