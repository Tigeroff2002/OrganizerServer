using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.RedisContracts.IssueEvents;

public sealed class IssueTerminalStatusReceivedEventDTO
    : IssueBasedEventDTO
{
    [JsonProperty("terminal_moment", Required = Required.Always)]
    public required DateTimeOffset TerminalMoment { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("terminal_status", Required = Required.Always)]
    public required IssueStatus TerminalStatus { get; init; }
}
