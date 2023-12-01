using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts.Response;

public sealed class ReportDescriptionResult
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("report_type", Required = Required.Always)]
    public required ReportType ReportType { get; init; }

    [JsonProperty("begin_moment", Required = Required.Always)]
    public required DateTimeOffset BeginMoment { get; init; }

    [JsonProperty("end_moment", Required = Required.Always)]
    public required DateTimeOffset EndMoment { get; init; }

    [JsonProperty("content", Required = Required.Default)]
    public string Content { get; set; } = default!;
}
