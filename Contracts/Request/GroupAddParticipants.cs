using Newtonsoft.Json;

namespace Contracts.Request;

public sealed class GroupAddParticipants : RequestWithToken
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }

    [JsonProperty("participants", Required = Required.Always)]
    public List<int> Participants { get; set; } = default!;
}
