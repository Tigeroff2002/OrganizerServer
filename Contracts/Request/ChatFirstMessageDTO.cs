using Newtonsoft.Json;

namespace Contracts.Request;

public sealed class ChatFirstMessageDTO : RequestWithToken
{
    [JsonProperty("text", Required = Required.Always)]
    public required string Text { get; init; }


    [JsonProperty("receiver_id", Required = Required.Always)]
    public required int ReceiverId { get; init; }
}
