using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class EventUserDecisionDTO : RequestWithToken
{
    [JsonProperty("event_id", Required = Required.Always)]
    public required int EventId { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("decision_type", Required = Required.Default)]
    public DecisionType DecisionType { get; set; } = DecisionType.None;
}
