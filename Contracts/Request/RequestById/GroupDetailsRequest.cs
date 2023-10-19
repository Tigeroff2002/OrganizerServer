using Models.Enums;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById; 

public class GroupDetailsRequest
{
    [JsonProperty("group_id", Required = Required.Always, Order = 1)]
    public required int GroupId { get; init; }
}
