using Models.RedisEventModels;

namespace Logic.Abstractions;

public interface IRedisRepository
{
    public Task InsertEventAsync(BaseEvent @event, CancellationToken token = default);

    public Task MarkAsCommitedAsync(List<string> eventsIds, CancellationToken token = default);

    public Task DeleteEvents(List<string> eventsIds, CancellationToken token = default);

    public Task<List<BaseEvent>> GetEventsAsync(
        bool takeOnlyNotCommited = false,
        CancellationToken token = default);
}
