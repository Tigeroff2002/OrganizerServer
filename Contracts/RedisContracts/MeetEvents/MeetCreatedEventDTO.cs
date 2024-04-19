using Newtonsoft.Json;

namespace Contracts.RedisContracts.MeetEvents;

public sealed class MeetCreatedEventDTO
    : MeetBasedEventDTO
{
    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreatedMoment { get; init; }
}
