using Newtonsoft.Json;

namespace Models.BusinessModels;

public sealed class ResponseWithToken : Response
{
    [JsonProperty("user_id", Required = Required.Default)]
    public int UserId { get; set; }

    [JsonProperty("token", Required = Required.Default)]
    public string? Token { get; set; }

    [JsonProperty("user_name", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserName { get; set; }
}
