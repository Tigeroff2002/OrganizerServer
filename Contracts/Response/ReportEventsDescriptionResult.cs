using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class ReportEventsDescriptionResult : ReportDescriptionResult
{
    [JsonProperty("user_events", Required = Required.Default)]
    public IReadOnlyCollection<EventInfoResponse> EventsInformation { get; set; } = default!;
}
