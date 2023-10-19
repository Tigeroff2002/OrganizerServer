using Models.Enums;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public sealed class TasksInfoRequest : RequestWithToken
{
    [JsonProperty("task_type", Required = Required.Default)]
    public TaskType TaskType { get; init; }
}
