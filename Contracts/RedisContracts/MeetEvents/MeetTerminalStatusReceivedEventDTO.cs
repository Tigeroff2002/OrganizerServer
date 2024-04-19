using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.RedisContracts.MeetEvents;

public sealed class MeetTerminalStatusReceivedEventDTO
    : MeetBasedEventDTO
{
    [JsonProperty("terminal_moment", Required = Required.Always)]
    public required DateTimeOffset TerminalMoment { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("terminal_status", Required = Required.Always)]
    public required EventStatus TerminalStatus { get; init; }
}
