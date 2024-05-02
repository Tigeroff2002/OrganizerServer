using Newtonsoft.Json;

namespace Contracts.RedisContracts.IssueEvents;

public sealed class IssueParamsChangedEventDTO
    : IssueBasedEventDTO
{
    [JsonProperty("update_moment", Required = Required.Always)]
    public required DateTimeOffset UpdateMoment { get; init; }

    [JsonProperty("json", Required = Required.Always)]
    public required string Json { get; init; }
}
