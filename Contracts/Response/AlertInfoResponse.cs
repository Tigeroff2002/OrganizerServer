using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class AlertInfoResponse
{
    [JsonProperty("alert_id", Required = Required.Always)]
    public required int Id { get; init; }

    [JsonProperty("title", Required = Required.Always)]
    public required string Title { get; init; }

    [JsonProperty("description", Required = Required.Always)]
    public required string Description { get; init; }

    [JsonProperty("moment", Required = Required.Always)]
    public required DateTimeOffset Moment { get; init; }

    [JsonProperty("is_alerted", Required = Required.Always)]
    public required bool IsAlerted { get; init; }
}
