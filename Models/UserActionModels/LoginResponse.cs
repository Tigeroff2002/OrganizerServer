using Models.BusinessModels;
using Newtonsoft.Json;

namespace Models.UserActionModels;

public sealed class LoginResponse : Response
{
    [JsonProperty("user_id", Required = Required.Default)]
    public int UserId { get; set; }

    [JsonProperty("token", Required = Required.Default)]
    public string? Token { get; set; }

    [JsonProperty("user_name", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserName { get; set; }

    [JsonProperty("firebase_token", Required = Required.Default)]
    public string FirebaseToken { get; set; }
}
