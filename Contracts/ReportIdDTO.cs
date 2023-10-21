using Contracts.Request;
using Newtonsoft.Json;

namespace Contracts;

public class ReportIdDTO : RequestWithToken
{
    [JsonProperty("report_id", Required = Required.Always)]
    public required int ReportId { get; init; }
}
