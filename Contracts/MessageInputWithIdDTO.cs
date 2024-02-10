using Contracts.Request;
using Newtonsoft.Json;

namespace Contracts;

public sealed class MessageInputWithIdDTO : RequestWithToken
{
    [JsonProperty("message_id", Required = Required.Always)]
    public required int MessageId { get; init; }

    [JsonProperty("text", Required = Required.Always)]
    public required string Text { get; init; }
}
