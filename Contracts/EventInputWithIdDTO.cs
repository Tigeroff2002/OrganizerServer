using Newtonsoft.Json;

namespace Contracts;

public class EventInputWithIdDTO : EventInputDTO
{
    [JsonProperty("event_id", Required = Required.Always)]
    public required int EventId { get; init; }
}
