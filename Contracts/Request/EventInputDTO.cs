using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Contracts.Request;

namespace Contracts;

public sealed class EventInputDTO : RequestWithToken
{
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
    [JsonProperty("activity_kind", Required = Required.Always)]
    public required ActivityKind ActivityKind { get; init; }

    [JsonProperty("manager_id", Required = Required.Always, Order = 1)]
    public required int ManagerId { get; init; }

    [JsonProperty("guests_ids", Required = Required.Default)]
    public List<int> GuestsIds { get; set; } = default!;
}
