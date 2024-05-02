using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public class EventIdDTO : RequestWithToken
{
    [JsonProperty("event_id", Required = Required.Always)]
    public required int EventId { get; init; }
}