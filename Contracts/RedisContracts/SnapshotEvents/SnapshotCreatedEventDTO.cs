using Newtonsoft.Json;

namespace Contracts.RedisContracts.SnapshotEvents;

public sealed class SnapshotCreatedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("snapshot_id", Required = Required.Always)]
    public required string SnapshotId { get; init; }
}
