using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class ChatMessagesResponse
{
    [JsonProperty("chat_id", Required = Required.Always)]
    public required int ChatId { get; init; }

    [JsonProperty("caption", Required = Required.Always)]
    public required string Caption { get; init; }

    [JsonProperty("create_time", Required = Required.Always)]
    public required DateTimeOffset CreateTime { get; init; }

    [JsonProperty("user_home", NullValueHandling = NullValueHandling.Ignore)]
    public ShortUserInfo UserHome { get; set; }

    [JsonProperty("user_away", NullValueHandling = NullValueHandling.Ignore)]
    public ShortUserInfo UserAway { get; set; }

    [JsonProperty("messages", Required = Required.Always)]
    public List<MessageInfoResponse> Messages { get; set; }
}
