using Models.BusinessModels;
using Newtonsoft.Json;

namespace Contracts;

public sealed class UserLoginDataDTO
{
    [JsonProperty("email", Required = Required.Always)]
    public required string Email { get; init; }

    [JsonProperty("firebase_token", Required = Required.Always)]
    public required string FirebaseToken { get; init; }

    [JsonProperty("password", Required = Required.Always)]
    public required string Password { get; init; }


    public UserLoginData ToModel()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            throw new ArgumentException(nameof(Email));
        }

        if (string.IsNullOrWhiteSpace(FirebaseToken))
        {
            throw new ArgumentException(nameof(FirebaseToken));
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            throw new ArgumentException(nameof(Password));
        }

        return new(
            Email,
            FirebaseToken,
            Password);
    }
}
