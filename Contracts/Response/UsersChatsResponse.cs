using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UsersChatsResponse
{
    [JsonProperty("chats", NullValueHandling = NullValueHandling.Ignore)]
    public List<ChatMessagesResponse> ChatsWithMessages { get; set; }
}
