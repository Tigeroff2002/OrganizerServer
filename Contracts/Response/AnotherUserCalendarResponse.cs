using Newtonsoft.Json;

namespace Contracts.Response;

public sealed class AnotherUserCalendarResponse
{
    [JsonProperty("participant_events", NullValueHandling = NullValueHandling.Ignore)]
    public List<EventInfoResponse> UserEvents { get; set; } = default!;
}
