using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class AnotherUserCalendarRequest : RequestWithToken
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }

    [JsonProperty("participant_id", Required = Required.Always)]
    public required int ParticipantId { get; init; }
}
