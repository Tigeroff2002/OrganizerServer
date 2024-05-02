using Newtonsoft.Json;

namespace Contracts.RedisContracts.TaskEvents;

public sealed class TaskCreatedEventDTO
    : TaskBasedEventDTO
{
    [JsonProperty("create_moment", Required = Required.Always)]
    public required DateTimeOffset CreatedMoment { get; init; }
}
