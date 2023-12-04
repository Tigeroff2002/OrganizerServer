using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Response;

public sealed class EventContent
{
    [JsonProperty("guests", NullValueHandling = NullValueHandling.Ignore)]
    public List<ShortUserInfo> Guests { get; set; } = default!;

    [JsonProperty("group_content", NullValueHandling = NullValueHandling.Ignore)]
    public object GroupContent { get; set; } = default!;

    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; set; }

    [JsonProperty("caption", Required = Required.Always)]
    public required string Caption { get; init; }

    [JsonProperty("description", Required = Required.Always)]
    public required string Description { get; init; }

    [JsonProperty("scheduled_start", Required = Required.Always)]
    public required DateTimeOffset ScheduledStart { get; init; }

    [JsonProperty("duration", Required = Required.Always)]
    public required TimeSpan Duration { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("event_type", Required = Required.Always)]
    public required EventType EventType { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("event_status", Required = Required.Always)]
    public required EventStatus EventStatus { get; init; }
}
