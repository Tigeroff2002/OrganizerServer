﻿using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class ReportInfoResponse
{
    [JsonProperty("user_name", Required = Required.Always)]
    public required string UserName { get; init; }

    [JsonProperty("user_email", Required = Required.Always)]
    public required string UserEmail { get; init; }

    [JsonProperty("description", Required = Required.Always)]
    public required string ReportDescription { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("report_type", Required = Required.Always)]
    public required ReportType ReportType { get; init; }

    [JsonProperty("begin_moment", Required = Required.Always)]
    public required DateTimeOffset BeginMoment { get; init; }

    [JsonProperty("end_moment", Required = Required.Always)]
    public required DateTimeOffset EndMoment { get; init; }
}
