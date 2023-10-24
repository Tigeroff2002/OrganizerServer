using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Request;

public sealed class UpdateGroupParams : RequestWithToken
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }

    [JsonProperty("group_name", Required = Required.Default)]
    public string GroupName { get; set; } = default!;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("group_type", Required = Required.Default)]
    public GroupType Type { get; set; } = GroupType.None;

    [JsonProperty("participants", Required = Required.Default)]
    public List<int> Participants { get; set; } = default!;
}
