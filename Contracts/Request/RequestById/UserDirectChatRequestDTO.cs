using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class UserDirectChatRequestDTO : RequestWithToken
{
    [JsonProperty("receiver_id", Required = Required.Always)]
    public required int ReceiverId { get; init; }
}

