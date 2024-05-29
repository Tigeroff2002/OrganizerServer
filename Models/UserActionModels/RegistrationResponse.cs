using Models.UserActionModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Models.BusinessModels;

public sealed class RegistrationResponse : PreRegistrationResponse
{
    [JsonProperty("user_id", Required = Required.Default)]
    public int UserId { get; set; }

    [JsonProperty("token", Required = Required.Default)]
    public string? Token { get; set; }
}
