using Newtonsoft.Json;

namespace Contracts.RedisContracts.MeetEvents;

public abstract class MeetBasedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("meet_id", Required = Required.Always)]
    public required int MeetId { get; init; }
}
