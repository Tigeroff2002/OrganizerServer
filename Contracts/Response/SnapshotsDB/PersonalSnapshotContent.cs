using Newtonsoft.Json;

namespace Contracts.Response.SnapshotsDB;

public sealed class PersonalSnapshotContent : CommonSnapshotContent
{
    [JsonProperty("kpi", Required = Required.Always)]
    public required float KPI { get; init; }
}
