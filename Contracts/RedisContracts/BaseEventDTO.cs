using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.RedisContracts;

public abstract class BaseEventDTO
{
    [JsonProperty("event_id", Required = Required.Always)]
    public required string Id { get; init; }

    [JsonProperty("is_commited", Required = Required.Always)]
    public required bool IsCommited { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("event_type", Required = Required.Always)]
    public required RawEventType EventType { get; init; }
}
