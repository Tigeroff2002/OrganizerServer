using Newtonsoft.Json;

namespace Contracts.RedisContracts.IssueEvents;

public abstract class IssueBasedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("issue_id", Required = Required.Always)]
    public required int IssueId { get; init; }
}
