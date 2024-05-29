using Newtonsoft.Json;

namespace Contracts.Request;

public sealed class UserConfirmationEmailDTO
{
    [JsonProperty("email", Required = Required.Always)]
    public required string Email { get; init; }

    [JsonProperty("code", Required = Required.Always)]
    public required string Code { get; init; } 
}
