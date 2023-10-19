using Newtonsoft.Json;

namespace Models.BusinessModels;

public sealed class GetResponse : Response
{
    [JsonProperty("requested_info", Required = Required.Always)]
    public object? RequestedInfo { get; set; }
}
