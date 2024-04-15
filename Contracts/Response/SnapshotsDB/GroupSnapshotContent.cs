using Newtonsoft.Json;

namespace Contracts.Response.SnapshotsDB;

public sealed class GroupSnapshotContent : CommonSnapshotContent
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }

    [JsonProperty("participants_kpis", NullValueHandling = NullValueHandling.Ignore)]
    public List<GroupParticipantKPIResponse> ParticipantKPIs { get; set; }

    [JsonProperty("average_kpi", Required = Required.Always)]
    public required float AverageKPI { get; init; }
}
