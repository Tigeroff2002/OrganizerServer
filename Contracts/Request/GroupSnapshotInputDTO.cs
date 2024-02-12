﻿using Newtonsoft.Json;

namespace Contracts.Request;

public sealed class GroupSnapshotInputDTO : SnapshotInputDTO
{
    [JsonProperty("group_id", Required = Required.Always)]
    public required int GroupId { get; init; }
}
