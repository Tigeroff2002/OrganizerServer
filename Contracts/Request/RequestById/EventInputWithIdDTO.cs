using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public class EventInputWithIdDTO : RequestWithToken
{
    [JsonProperty("event_id", Required = Required.Always)]
    public required int EventId { get; init; }

    [JsonProperty("caption", Required = Required.Default)]
    public string Caption { get; set; } = default!;

    [JsonProperty("description", Required = Required.Default)]
    public string Description { get; set; } = default!;

    [JsonProperty("scheduled_start", Required = Required.Default)]
    public DateTimeOffset ScheduledStart { get; set; } = DateTimeOffset.MinValue;

    [JsonProperty("duration", Required = Required.Default)]
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("event_type", Required = Required.Default)]
    public EventType EventType { get; set; } = EventType.None;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("event_status", Required = Required.Default)]
    public EventStatus EventStatus { get; set; } = EventStatus.None;

    [JsonProperty("guests_ids", Required = Required.Default)]
    public List<int> GuestsIds { get; set; } = default!;
}
