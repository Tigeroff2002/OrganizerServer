using Newtonsoft.Json;

namespace Contracts.Response.SnapshotsDB;

public class CommonSnapshotContent
{
    [JsonProperty("content", Required = Required.Default)]
    public string Content { get; set; } = default!;
}
