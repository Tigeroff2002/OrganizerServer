using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.RedisContracts.MeetEvents;

public sealed class MeetGuestChangedDecisionEventDTO
    : MeetBasedEventDTO
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("new_decision", Required = Required.Always)]
    public required DecisionType NewDecision { get; init; }

    [JsonProperty("scheduled_start", Required = Required.Always)]
    public required DateTimeOffset ScheduledStart { get; init; }
}
