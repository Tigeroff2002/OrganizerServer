using Newtonsoft.Json;

namespace Contracts.RedisContracts.AlertEvents;

public sealed class AlertCreatedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("alert_id", Required = Required.Always)]
    public required int AlertId { get; init; }

    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreatedMoment { get; init; }
}
