using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class GroupInfoResponse
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }

    [JsonProperty("manager_id", Required = Required.Always)]
    public required int ManagerId { get; init; }    

    [JsonProperty("group_name", Required = Required.Always)]
    public required string GroupName { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("group_type", Required = Required.Always)]
    public required GroupType Type { get; init; }

    [JsonProperty("participants", NullValueHandling = NullValueHandling.Ignore)]
    public List<ShortUserInfo> Participants { get; set; } = default!;
}
