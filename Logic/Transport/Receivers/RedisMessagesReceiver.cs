using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.RedisEventModels;

namespace Logic.Transport.Receivers;

public sealed class RedisMessagesReceiver : IRedisMessagesReceiver
{
    public RedisMessagesReceiver(
        IRedisRepository redisRepository,
        ILogger<RedisMessagesReceiver> logger)
    {
        _redisRepository = redisRepository 
            ?? throw new ArgumentNullException(nameof(redisRepository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<UserRelatedEvent>> GetMessages(CancellationToken token)
    {
        var messages = await _redisRepository.GetEventsAsync(false, token);

        _logger.LogInformation("Received {Count} new messages from redis", messages.Count);

        await _redisRepository.DeleteEvents(messages.Select(x => x.Id).ToList(), token);

        _logger.LogInformation("These redis messages were removed");

        return messages;
    }

    private readonly ILogger<RedisMessagesReceiver> _logger;
    private readonly IRedisRepository _redisRepository;
}
