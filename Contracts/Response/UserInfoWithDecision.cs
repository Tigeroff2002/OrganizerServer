using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class UserInfoWithDecision : ShortUserInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("decision_type", Required = Required.Default)]
    public DecisionType DecisionType { get; set; } = DecisionType.Default;
}
