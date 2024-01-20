﻿using Contracts.Request;
using Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Contracts;

public sealed class IssueInputWithIdDTO : RequestWithToken
{
    [JsonProperty("issue_id", Required = Required.Always)]
    public required int IssueId { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("issue_type", Required = Required.Always)]
    public required IssueType IssueType { get; init; }

    [JsonProperty("title", Required = Required.Always)]
    public required string Title { get; init; }

    [JsonProperty("description", Required = Required.Always)]
    public required string Description { get; init; }

    [JsonProperty("img_link", Required = Required.Always)]
    public required string ImgLink { get; init; }
}
