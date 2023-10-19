using Newtonsoft.Json;

namespace Contracts.Request;

public class RequestWithToken
{
    [JsonProperty("user_id", Required = Required.Default)]
    public required int UserId { get; init; }

    [JsonProperty("token", Required = Required.Default)]
    public required string Token { get; init; }
}
