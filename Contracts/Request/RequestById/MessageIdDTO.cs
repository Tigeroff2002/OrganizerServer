using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class MessageIdDTO : RequestWithToken
{
    [JsonProperty("message_id", Required = Required.Always)]
    public required int MessageId { get; init; }
}
