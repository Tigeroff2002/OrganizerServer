using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.RedisContracts.TaskEvents;

public sealed class TaskTerminalStatusReceivedEventDTO
    : TaskBasedEventDTO
{
    [JsonProperty("terminal_moment", Required = Required.Always)]
    public required DateTimeOffset TerminalMoment { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("terminal_status", Required = Required.Always)]
    public required TaskCurrentStatus TerminalStatus { get; init; }
}
