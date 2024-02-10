using Newtonsoft.Json;

namespace Contracts.Request;

public class NewMessageInputDTO : RequestWithToken
{
    [JsonProperty("text", Required = Required.Always)]
    public required string Text { get; init; }

    [JsonProperty("chat_id", Required = Required.Always)]
    public required int ChatId { get; init; }
}
