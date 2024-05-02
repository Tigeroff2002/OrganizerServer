using Models.RedisEventModels;

namespace Logic.Abstractions;

public interface IRedisMessagesReceiver
{
    public Task<List<UserRelatedEvent>> GetMessages(CancellationToken token);
}
