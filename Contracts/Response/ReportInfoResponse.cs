using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class ReportInfoResponse
{
    [JsonProperty("description", Required = Required.Always)]
    public required string ReportDescription { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("report_type", Required = Required.Always)]
    public required ReportType ReportType { get; init; }

    [JsonProperty("begin_moment", Required = Required.Always)]
    public required DateTimeOffset BeginMoment { get; init; }

    [JsonProperty("end_moment", Required = Required.Always)]
    public required DateTimeOffset EndMoment { get; init; }

    [JsonProperty("reporter", Required = Required.Default)]
    public ShortUserInfo Reporter { get; set; } = default!;
}
