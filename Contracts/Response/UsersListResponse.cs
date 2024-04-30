using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UsersListResponse
{
    [JsonProperty("users", Required = Required.Always)]
    public required List<ShortUserInfo> Users { get; init; }
}
