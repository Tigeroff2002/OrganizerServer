using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Response;

public sealed class GroupSnapshotDescriptionResult
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("snapshot_type", Required = Required.Always)]
    public required SnapshotType SnapshotType { get; init; }

    [JsonProperty("begin_moment", Required = Required.Always)]
    public required DateTimeOffset BeginMoment { get; init; }

    [JsonProperty("end_moment", Required = Required.Always)]
    public required DateTimeOffset EndMoment { get; init; }

    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreateMoment { get; init; }

    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }

    [JsonProperty("participants_kpis", NullValueHandling = NullValueHandling.Ignore)]
    public List<GroupParticipantKPIResponse> ParticipantKPIs { get; set; }

    [JsonProperty("average_kpi", Required = Required.Always)]
    public required float AverageKPI { get; init; }

    [JsonProperty("content", Required = Required.Default)]
    public string Content { get; set; } = default!;
}
