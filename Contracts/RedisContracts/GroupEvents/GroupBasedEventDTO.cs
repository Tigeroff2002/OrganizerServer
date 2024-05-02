using Newtonsoft.Json;

namespace Contracts.RedisContracts.GroupEvents;

public abstract class GroupBasedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }
}
