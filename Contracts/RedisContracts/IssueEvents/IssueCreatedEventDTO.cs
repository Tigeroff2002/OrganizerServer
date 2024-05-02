using Newtonsoft.Json;

namespace Contracts.RedisContracts.IssueEvents;

public sealed class IssueCreatedEventDTO
    : IssueBasedEventDTO
{
    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreatedMoment { get; init; }
}
