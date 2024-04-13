using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class SystemAlerts
{
    [JsonProperty("alerts", NullValueHandling = NullValueHandling.Ignore)]
    public List<AlertInfoResponse> Alerts { get; set; }
}
