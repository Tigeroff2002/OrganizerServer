using Newtonsoft.Json;

namespace Contracts.RedisContracts.UserEvents;

public sealed class UserInfoUpdateEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("update_moment", Required = Required.Always)]
    public required DateTimeOffset UpdateMoment { get; init; }

    [JsonProperty("json", Required = Required.Always)]
    public required string Json { get; init; }
}
