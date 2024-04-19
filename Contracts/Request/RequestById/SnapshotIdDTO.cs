using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public class SnapshotIdDTO : RequestWithToken
{
    [JsonProperty("snapshot_id", Required = Required.Always)]
    public required int SnapshotId { get; init; }
}
