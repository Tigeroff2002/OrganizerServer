using Logic.Abstractions;
using Microsoft.Extensions.Options;

namespace ToDoCalendarServer.Services;

public class EventNotifyService : BackgroundService
{
    public EventNotifyService(
        ILogger<EventNotifyService> logger,
        IOptions<StartDelayConfiguration> options,
        IEventNotificationsHandler notifiesHandler)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration = options.Value;

        _notifiesHandler = notifiesHandler 
            ?? throw new ArgumentNullException(nameof(notifiesHandler));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Event notify service has been created");
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        Task.Delay(_configuration.StartDelayMs).GetAwaiter().GetResult();

        await base.StartAsync(stoppingToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _logger.BeginScope(
            "Beginning handling events notifications");

        return _notifiesHandler.HandleEventsAsync(stoppingToken);
    }

    private readonly ILogger<EventNotifyService> _logger;
    private readonly IEventNotificationsHandler _notifiesHandler;
    private readonly StartDelayConfiguration _configuration;
}
