using Newtonsoft.Json;

namespace Contracts.RedisContracts.UserEvents;

public sealed class UserRegistrationEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("firebase_token", Required = Required.Always)]
    public required string FirebaseToken { get; init; }

    [JsonProperty("account_creation_moment", Required = Required.Always)]
    public required DateTimeOffset AccountCreationMoment { get; init; }
}
