using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class GroupContent
{
    [JsonProperty("participants", NullValueHandling = NullValueHandling.Ignore)]
    public List<ShortUserInfo> Participants { get; set; } = default!;
}
