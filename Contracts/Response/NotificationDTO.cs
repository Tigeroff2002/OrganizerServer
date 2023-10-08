using Newtonsoft.Json;

namespace Contracts;

public sealed class NotificationDTO
{
    [JsonProperty("event_id", Required = Required.Always)]
    public required int EventId { get; init; }

    [JsonProperty("remaining_time", Required = Required.Always)]
    public required TimeSpan RemainingTime { get; init; }

    [JsonProperty("text", Required = Required.Always)]
    public required string Text { get; init; }
}
