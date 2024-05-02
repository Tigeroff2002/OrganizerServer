using Newtonsoft.Json;

namespace Contracts.RedisContracts.TaskEvents;

public sealed class TaskParamsChangedEventDTO
    : TaskBasedEventDTO
{
    [JsonProperty("update_moment", Required = Required.Always)]
    public required DateTimeOffset UpdateMoment { get; init; }

    [JsonProperty("json", Required = Required.Always)]
    public required string Json { get; init; }
}
