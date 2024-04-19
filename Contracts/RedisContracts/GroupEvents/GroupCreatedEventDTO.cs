using Newtonsoft.Json;

namespace Contracts.RedisContracts.GroupEvents;

public sealed class GroupCreatedEventDTO
    : GroupBasedEventDTO
{
    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreatedMoment { get; init; }
}
