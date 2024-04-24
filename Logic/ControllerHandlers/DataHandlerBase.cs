using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using PostgreSQL.Abstractions;

namespace Logic.ControllerHandlers;

public abstract class DataHandlerBase
{
    protected ICommonUsersUnitOfWork CommonUnitOfWork { get; }

    protected IRedisRepository RedisRepository { get; }

    protected ILogger Logger { get; }

    public DataHandlerBase(
        ICommonUsersUnitOfWork commonUnitOfWork, 
        IRedisRepository redisRepository,
        ILogger logger)
    {
        CommonUnitOfWork = commonUnitOfWork 
            ?? throw new ArgumentNullException(nameof(commonUnitOfWork));

        RedisRepository = redisRepository 
            ?? throw new ArgumentNullException(nameof(redisRepository));

        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
