using Newtonsoft.Json;

namespace Contracts.RedisContracts.MeetEvents;

public sealed class MeetParamsChangedEventDTO
    : MeetBasedEventDTO
{
    [JsonProperty("update_moment", Required = Required.Always)]
    public required DateTimeOffset UpdateMoment { get; init; }

    [JsonProperty("json", Required = Required.Always)]
    public required string Json { get; init; }
}
