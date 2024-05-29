using Models.BusinessModels;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Models.UserActionModels;

public class PreRegistrationResponse : Response
{
    [JsonProperty("user_name", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserName { get; set; }

    [JsonProperty("firebase_token", Required = Required.Default)]
    public string FirebaseToken { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("case", Required = Required.Default)]
    public RegistrationCase RegistrationCase { get; set; }
}
