using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UserBadRequest
{
    [JsonProperty("user_id", Required = Required.Always)]
    public required int UserId { get; init; }

    [JsonProperty("message", Required = Required.Always)]
    public required string Message { get; init; }
}
