using Newtonsoft.Json;

namespace Contracts.RedisContracts.MeetEvents;

public sealed class MeetSoonBeginEventDTO
    : MeetBasedEventDTO
{
    [JsonProperty("remaining_time", Required = Required.Always)]
    public required TimeSpan RemainingTime { get; init; }

    [JsonProperty("scheduled_start", Required = Required.Always)]
    public required DateTimeOffset ScheduledStart { get; init; }
}
