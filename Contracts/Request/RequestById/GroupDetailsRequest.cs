using Models.Enums;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById; 

public class GroupDetailsRequest : RequestWithToken
{
    [JsonProperty("group_id", Required = Required.Always, Order = 1)]
    public required int GroupId { get; init; }

    [JsonProperty("user_id", Required = Required.Always, Order = 2)]
    public required string UserId { get; init; }
}
