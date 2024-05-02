using Newtonsoft.Json;

namespace Contracts.RedisContracts.UserEvents;

public sealed class UserLoginEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("firebase_token", Required = Required.Always)]
    public required string FirebaseToken { get; init; }

    [JsonProperty("token_set_moment", Required = Required.Always)]
    public required DateTimeOffset TokenSetMoment { get; init; }
}
