using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class SystemIssuesResponse
{
    [JsonProperty("issues", NullValueHandling = NullValueHandling.Ignore)]
    public List<FullIssueInfoResponse> Issues { get; set; }
}
