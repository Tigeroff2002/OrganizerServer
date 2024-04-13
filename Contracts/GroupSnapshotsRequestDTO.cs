using Contracts.Request;
using Newtonsoft.Json;

namespace Contracts;

public sealed class GroupSnapshotsRequestDTO : RequestWithToken
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }
}
