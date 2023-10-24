using Newtonsoft.Json;

namespace Contracts.Request;

public class UserUpdateInfoDTO : RequestWithToken
{
    [JsonProperty("email", Required = Required.Default)]
    public string Email { get; set; } = default!;

    [JsonProperty("name", Required = Required.Default)]
    public string UserName { get; set; } = default!;

    [JsonProperty("password", Required = Required.Default)]
    public string Password { get; set; } = default!;

    [JsonProperty("phone_number", Required = Required.Default)]
    public string PhoneNumber { get; set; } = default!;
}
