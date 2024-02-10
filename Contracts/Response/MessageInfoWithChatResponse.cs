using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class MessageInfoWithChatResponse
    : MessageInfoResponse
{
    [JsonProperty("chat_id", Required = Required.Always)]
    public required int ChatId { get; init; }

    [JsonProperty("caption", Required = Required.Always)]
    public required string ChatCaption { get; init; }

    [JsonProperty("receiver_id", Required = Required.Always)]
    public required int ReceiverId { get; init; }

    [JsonProperty("receiver_name", Required = Required.Always)]
    public required string ReceiverName { get; init; }
}
