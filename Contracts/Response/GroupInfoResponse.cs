using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class GroupInfoResponse
{
    [JsonProperty("group_name", Required = Required.Always)]
    public required string GroupName { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("group_type", Required = Required.Always)]
    public required GroupType Type { get; init; }

    [JsonProperty("participants", Required = Required.Default)]
    public List<ShortUserInfo> Participants { get; set; } = default!;
}
