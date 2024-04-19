using Newtonsoft.Json;

namespace Contracts.RedisContracts;

public abstract class UserRelatedEventDTO
    : BaseEventDTO
{
    [JsonProperty("user_id", Required = Required.Always)]
    public required int UserId { get; init; }
}
