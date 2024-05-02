using Newtonsoft.Json;

namespace Contracts.RedisContracts.MeetEvents;

public sealed class MeetCreatedEventDTO
    : MeetBasedEventDTO
{
    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreatedMoment { get; init; }

    [JsonProperty("scheduled_start", Required = Required.Always)]
    public required DateTimeOffset ScheduledStart { get; init; }

    [JsonProperty("duration", Required = Required.Always)]
    public required TimeSpan Duration { get; init; }
}
