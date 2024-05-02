using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public class SnapshotInfoResponse
{
    [JsonProperty("snapshot_id", Required = Required.Always)]
    public required int SnapshotId { get; init; }

    [JsonProperty("begin_moment", Required = Required.Always)]
    public required DateTimeOffset BeginMoment { get; init; }

    [JsonProperty("end_moment", Required = Required.Always)]
    public required DateTimeOffset EndMoment { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("snapshot_type", Required = Required.Always)]
    public required SnapshotType SnapshotType { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("audit_type", Required = Required.Always)]
    public required SnapshotAuditType AuditType { get; init; }

    [JsonProperty("content", Required = Required.Default)]
    public string Content { get; set; } = default!;

    [JsonProperty("creation_time", Required = Required.Always)]
    public required DateTimeOffset CreationTime { get; init; }
}
