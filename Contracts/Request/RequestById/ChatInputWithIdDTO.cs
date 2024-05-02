using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class ChatInputWithIdDTO : RequestWithToken
{
    [JsonProperty("chat_id", Required = Required.Always)]
    public required int ChatId { get; init; }

    [JsonProperty("caption", Required = Required.Always)]
    public required string Caption { get; init; }
}
