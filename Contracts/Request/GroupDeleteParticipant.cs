using Newtonsoft.Json;

namespace Contracts.Request;

public sealed class GroupDeleteParticipant : RequestWithToken
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }

    [JsonProperty("participant_id", Required = Required.Always)]
    public int Participant_Id { get; set; } = default!;
}
