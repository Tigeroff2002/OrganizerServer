using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Request;

public class SnapshotInputDTO : RequestWithToken
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("snapshot_type", Required = Required.Always)]
    public required SnapshotType SnapshotType { get; init; }

    [JsonProperty("begin_moment", Required = Required.Always)]
    public required DateTimeOffset BeginMoment { get; init; }

    [JsonProperty("end_moment", Required = Required.Always)]
    public required DateTimeOffset EndMoment { get; init; }
}
