using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Models.RedisEventModels;
using PostgreSQL.Abstractions;

namespace Logic.ControllerHandlers;

public abstract class DataHandlerBase
{
    protected ICommonUsersUnitOfWork CommonUnitOfWork { get; }

    protected ILogger Logger { get; }

    public DataHandlerBase(
        ICommonUsersUnitOfWork commonUnitOfWork, 
        IRedisRepository redisRepository,
        ILogger logger)
    {
        CommonUnitOfWork = commonUnitOfWork 
            ?? throw new ArgumentNullException(nameof(commonUnitOfWork));

        _redisRepository = redisRepository 
            ?? throw new ArgumentNullException(nameof(redisRepository));

        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEventForCacheAsync(BaseEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        Logger.LogDebug("Preparing to send event {EventId} to cache", @event.Id);

        await _redisRepository.InsertEventAsync(@event);
    }

    private readonly IRedisRepository _redisRepository;
}
