using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Response;

public class SnapshotDescriptionResult
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

    [JsonProperty("kpi", Required = Required.Default)]
    public float KPI { get; set; }    

    [JsonProperty("content", Required = Required.Default)]
    public string Content { get; set; } = default!;

    [JsonProperty("user_name", Required = Required.Always)]
    public string UserName {  get; set; }
}
