using Google.Apis;
using Logic.Abstractions;
using Logic.Transport.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.RedisEventModels;
using Models.StorageModels;
using StackExchange.Redis;
using System.Diagnostics;

namespace Logic;

public sealed class RedisRepository : IRedisRepository
{
    public RedisRepository(
        ISerializer<BaseEvent> serializer,
        IDeserializer<BaseEvent> deserializer,
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<RedisConfiguration> redisOptions,
        ILogger<RedisRepository> logger)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));

        _connectionMultiplexer = connectionMultiplexer
            ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

        ArgumentNullException.ThrowIfNull(redisOptions);

        _redisConfiguration = redisOptions.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InsertEventAsync(
        BaseEvent @event,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var json = _serializer.Serialize(@event);

        var database = _connectionMultiplexer.GetDatabase();

        var transaction = database.CreateTransaction();

        var transactionLog = transaction.SetAddAsync(
            new RedisKey(@event.Id), new RedisValue(json));

        await transaction.ExecuteAsync();

        await transactionLog;

        _logger.LogInformation(
            "Event with key {EventId} was sent to redis cache",
            @event.Id);
    }

    public async Task MarkAsCommitedAsync(
        List<string> eventsIds,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(eventsIds);

        var database = _connectionMultiplexer.GetDatabase();

        var transaction = database.CreateTransaction();

        foreach (var key in eventsIds)
        {
            var value = database.StringGetAsync(key).GetAwaiter().GetResult();

            var json = value.ToString();

            var @event = _deserializer.Deserialize(json);

            Debug.Assert(@event is not null);

            if (!@event.IsCommited)
            {
                @event.IsCommited = true;

                _logger.LogInformation(
                    "Event with key {EventId} has been commited",
                @event.Id);

                var redisValue = new RedisValue(_serializer.Serialize(@event));

                var transactionLog = 
                    transaction.StringSetAsync(new RedisKey(key), redisValue);

                await transaction.ExecuteAsync();

                await transactionLog;
            }
        }
    }

    public async Task DeleteEvents(
        List<string> eventsIds, 
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(eventsIds);

        var database = _connectionMultiplexer.GetDatabase(); 

        var transaction = database.CreateTransaction();

        var transactionLog =
            transaction.KeyDeleteAsync(
                eventsIds.Select(x => new RedisKey(x)).ToArray());

        await transaction.ExecuteAsync();

        await transactionLog;
    }

    public async Task<List<BaseEvent>> GetEventsAsync(
        bool takeOnlyNotCommited = false,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var database = _connectionMultiplexer.GetDatabase();

        var keys = _connectionMultiplexer
            .GetServer(_redisConfiguration.HostAndPort)
            .KeysAsync();

        var listOfEvents = new List<BaseEvent>();

        await foreach (var key in keys)
        {
            var value = await database.StringGetAsync(key);

            _logger.LogInformation(
                "Event with key {EventId} was requested from cache",
                key.ToString());

            var json = value.ToString();

            var @event = _deserializer.Deserialize(json);

            Debug.Assert(@event is not null);

            listOfEvents.Add(@event);
        }

        if (takeOnlyNotCommited)
        {
            listOfEvents = listOfEvents.Where(x => !x.IsCommited).ToList();
        }

        return listOfEvents;
    }

    private readonly ISerializer<BaseEvent> _serializer;
    private readonly IDeserializer<BaseEvent> _deserializer;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly RedisConfiguration _redisConfiguration;
    private readonly ILogger _logger;
}
