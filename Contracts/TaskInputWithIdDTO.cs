using Newtonsoft.Json;

namespace Contracts;

public class TaskInputWithIdDTO : TaskInputDTO
{
    [JsonProperty("task_id", Required = Required.Always)]
    public required int TaskId { get; init; }
}
