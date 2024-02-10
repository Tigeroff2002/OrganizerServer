using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UsersChatsResponse
{
    [JsonProperty("chats", NullValueHandling = NullValueHandling.Ignore)]
    public List<ShortChatInfoResponse> ChatsWithIds { get; set; }
}
