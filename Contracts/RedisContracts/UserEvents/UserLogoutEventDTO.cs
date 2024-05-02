using Newtonsoft.Json;

namespace Contracts.RedisContracts.UserEvents;

public sealed class UserLogoutEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("firebase_token", Required = Required.Always)]
    public required string FirebaseToken { get; init; }
}
