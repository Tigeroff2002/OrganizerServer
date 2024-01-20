﻿using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class IssueInfoResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("issue_type", Required = Required.Always)]
    public required IssueType IssueType { get; init; }

    [JsonProperty("title", Required = Required.Always)]
    public required string Title { get; init; }

    [JsonProperty("description", Required = Required.Always)]
    public required string Description { get; init; }

    [JsonProperty("img_link", Required = Required.Always)]
    public required string ImgLink { get; init; }

    [JsonProperty("issue_moment", Required = Required.Always)]
    public required DateTimeOffset CreateMoment { get; init; }
}
