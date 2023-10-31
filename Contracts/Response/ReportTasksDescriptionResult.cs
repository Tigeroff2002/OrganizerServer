using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class ReportTasksDescriptionResult : ReportDescriptionResult
{
    [JsonProperty("user_tasks", Required = Required.Default)]
    public IReadOnlyCollection<TaskInfoResponse> TasksInformation { get; set; } = default!;
}
