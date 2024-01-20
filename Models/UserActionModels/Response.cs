using Newtonsoft.Json;

namespace Models.BusinessModels;

public class Response
{
    [JsonProperty("result", Required = Required.Always)]
    public bool Result { get; set; }

    [JsonProperty("out_info", Required = Required.Always)]
    public string? OutInfo { get; set; }
}
