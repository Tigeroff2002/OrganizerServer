using Contracts.Request;
using Newtonsoft.Json;

namespace Contracts;

public sealed class ChatIdDTO : RequestWithToken
{
    [JsonProperty("chat_id", Required = Required.Always)]
    public required int ChatId { get; init; }
}
