using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Request;

public sealed class UserUpdateRoleDTO : RequestWithToken
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("user_role", Required = Required.Always)]
    public required UserRole NewRole { get; set; }

    [JsonProperty("root_password", Required = Required.Always)]
    public string RootPassword { get; set; }
}
