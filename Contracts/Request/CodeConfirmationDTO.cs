using Newtonsoft.Json;

namespace Contracts;

public sealed class CodeConfirmationDTO
{
    [JsonProperty("code", Required = Required.Always)]
    public required string Code { get; init; }
}
