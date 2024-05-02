using Models.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Contracts.Request.RequestById;

public class TaskInputWithIdDTO : RequestWithToken
{
    [JsonProperty("task_id", Required = Required.Always)]
    public required int TaskId { get; init; }

    [JsonProperty("caption", Required = Required.Default)]
    public string TaskCaption { get; set; } = default!;

    [JsonProperty("description", Required = Required.Default)]
    public string TaskDescription { get; set; } = default!;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("task_type", Required = Required.Default)]
    public TaskType TaskType { get; set; } = TaskType.None;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("task_status", Required = Required.Default)]
    public TaskCurrentStatus TaskStatus { get; init; } = TaskCurrentStatus.None;

    [JsonProperty("implementer_id", Required = Required.Default)]
    public int ImplementerId { get; set; } = -1;
}
