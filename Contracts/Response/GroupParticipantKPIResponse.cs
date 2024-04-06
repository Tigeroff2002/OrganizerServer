using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class GroupParticipantKPIResponse
{
    [JsonProperty("participant_id", Required = Required.Always)]
    public required int ParticipantId { get; init; }

    [JsonProperty("participant_name", Required = Required.Always)]
    public required string ParticipantName { get; init; }

    [JsonProperty("participant_kpi", Required = Required.Always)]
    public required float ParticipantKPI { get; init; }
}
