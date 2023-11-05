using Newtonsoft.Json;

namespace Models.BusinessModels;

public sealed class ResponseWithToken : Response
{
    [JsonProperty("token", Required = Required.Default)]
    public string Token { get; set; }
}
