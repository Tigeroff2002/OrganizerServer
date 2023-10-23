using Newtonsoft.Json;

namespace Contracts.Request;

public class UserUpdateInfoDTO : RequestWithToken
{
    [JsonProperty("email", Required = Required.Always)]
    public required string Email { get; init; }

    [JsonProperty("name", Required = Required.Always)]
    public required string UserName { get; init; }

    [JsonProperty("password", Required = Required.Always)]
    public required string Password { get; init; }

    [JsonProperty("phone_number", Required = Required.Always)]
    public required string PhoneNumber { get; init; }
}
