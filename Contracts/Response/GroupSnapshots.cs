using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class GroupSnapshots
{
    [JsonProperty("group_snapshots", NullValueHandling = NullValueHandling.Ignore)]
    public List<GroupSnapshotInfoResponse> Snapshots { get; set; }
}
