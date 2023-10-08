using Newtonsoft.Json;

namespace Contracts.Request;

public abstract class RequestWithToken
{
    [JsonProperty("token", Required = Required.Always)]
    public required string Token { get; init; }
}
