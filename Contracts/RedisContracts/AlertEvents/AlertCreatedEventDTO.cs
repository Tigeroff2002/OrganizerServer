using Newtonsoft.Json;

namespace Contracts.RedisContracts.AlertEvents;

public sealed class AlertCreatedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("alert_id", Required = Required.Always)]
    public required string AlertId { get; init; }
}
