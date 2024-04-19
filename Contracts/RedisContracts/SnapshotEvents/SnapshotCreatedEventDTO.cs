using Newtonsoft.Json;

namespace Contracts.RedisContracts.SnapshotEvents;

public sealed class SnapshotCreatedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("snapshot_id", Required = Required.Always)]
    public required int SnapshotId { get; init; }

    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreatedMoment { get; init; }
}
