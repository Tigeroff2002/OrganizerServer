using Models.Enums;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Request; 

public class GroupInputDTO
{
    [JsonProperty("group_name", Required = Required.Always)]
    public required string GroupName { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("group_type", Required = Required.Always)]
    public required GroupType Type { get; init; }

    [JsonProperty("participants", Required = Required.Default)]
    public List<User> Participants { get; set; } = default!;
}
