using Models.RedisEventModels;

namespace Logic.Abstractions;

public interface IRedisEventsAliaser
{
    public string GetAliasForEvent(UserRelatedEvent redisEvent);
}
