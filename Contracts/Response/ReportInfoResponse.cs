using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class ReportInfoResponse
{
    [JsonProperty("begin_moment", Required = Required.Always)]
    public required DateTimeOffset BeginMoment { get; init; }

    [JsonProperty("end_moment", Required = Required.Always)]
    public required DateTimeOffset EndMoment { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("report_type", Required = Required.Always)]
    public required ReportType ReportType { get; init; }

    [JsonProperty("content", Required = Required.Default)]
    public string Content { get; set; } = default!;

    [JsonProperty("creation_time", Required = Required.Always)]
    public required DateTimeOffset CreationTime { get; init; }
}
