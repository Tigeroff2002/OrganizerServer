using Logic.Abstractions;
using Logic.Notifications;
using Microsoft.Extensions.Options;
using ToDoCalendarServer.Services;

namespace OrganizerServer.Services;

public sealed class RedisProcessorService : BackgroundService
{
    public RedisProcessorService(
        ILogger<RedisProcessorService> logger,
        IOptions<StartDelayConfiguration> options,
        IRedisProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration = options.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _processor = processor ?? throw new ArgumentNullException(nameof(processor));

        _logger.LogInformation("Redis processor service has been created");
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        Task.Delay(_configuration.StartDelayMs).GetAwaiter().GetResult();

        await base.StartAsync(stoppingToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _processor.ProcessAsync(stoppingToken);
    }

    private readonly ILogger<RedisProcessorService> _logger;
    private readonly IRedisProcessor _processor;
    private readonly StartDelayConfiguration _configuration;
}
