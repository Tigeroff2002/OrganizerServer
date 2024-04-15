using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class PersonalSnapshotInfoResponse : SnapshotInfoResponse
{
    [JsonProperty("kpi", Required = Required.Always)]
    public required float KPI { get; init; }
}
