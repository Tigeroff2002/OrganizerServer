using Newtonsoft.Json;

namespace Contracts.RedisContracts.TaskEvents;

public abstract class TaskBasedEventDTO
    : UserRelatedEventDTO
{
    [JsonProperty("task_id", Required = Required.Always)]
    public required int TaskId { get; init; }
}
