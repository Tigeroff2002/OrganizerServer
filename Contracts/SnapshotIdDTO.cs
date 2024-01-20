using Contracts.Request;
using Newtonsoft.Json;

namespace Contracts;

public class SnapshotIdDTO : RequestWithToken
{
    [JsonProperty("snapshot_id", Required = Required.Always)]
    public required int ReportId { get; init; }
}
