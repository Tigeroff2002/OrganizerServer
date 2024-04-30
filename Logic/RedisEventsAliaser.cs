using Logic.Abstractions;
using Models.RedisEventModels;

namespace Logic;

public sealed class RedisEventsAliaser : IRedisEventsAliaser
{
    public string GetAliasForEvent(UserRelatedEvent redisEvent)
    {
        return "New event";
    }
}
