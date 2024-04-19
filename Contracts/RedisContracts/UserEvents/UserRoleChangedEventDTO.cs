using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.RedisContracts.UserEvents;

public sealed class UserRoleChangedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("update_moment", Required = Required.Always)]
    public required DateTimeOffset UpdateMoment { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("new_role", Required = Required.Always)]
    public required UserRole NewRole { get; init; }
}
