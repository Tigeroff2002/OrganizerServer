using Models.Enums;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById; 

public class GroupDetailsRequest : RequestWithToken
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }
}
