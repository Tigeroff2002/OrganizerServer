using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Response;

public class ShortUserInfo
{
    [JsonProperty("user_id", Required = Required.Default)]
    public int UserId { get; init; }

    [JsonProperty("user_name", Required = Required.Always)]
    public required string UserName { get; init; }

    [JsonProperty("user_email", Required = Required.Always)]
    public required string UserEmail { get; init; }

    [JsonProperty("phone_number", Required = Required.Always)]
    public required string UserPhone { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("user_role", Required = Required.Default)]
    public UserRole Role { get; init; }
}
