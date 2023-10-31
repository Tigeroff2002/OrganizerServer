using Newtonsoft.Json;

namespace Contracts.Response;

public abstract class ReportDescriptionResult
{
    [JsonProperty("creation_time", Required = Required.Always)]
    public required DateTimeOffset CreationTime { get; init; }
}
