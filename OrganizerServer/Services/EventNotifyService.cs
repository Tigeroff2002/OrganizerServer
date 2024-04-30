using Logic.Abstractions;
using Microsoft.Extensions.Options;

namespace ToDoCalendarServer.Services;

public class EventNotifyService : BackgroundService
{
    public EventNotifyService(
        ILogger<EventNotifyService> logger,
        IOptions<StartDelayConfiguration> options,
        INotificationsHandler notificationsHandler)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration = options.Value;

        _notificationsHandler = notificationsHandler
            ?? throw new ArgumentNullException(nameof(notificationsHandler));

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
        return _notificationsHandler.HandleSystemNotifications(stoppingToken);
    }

    private readonly ILogger<EventNotifyService> _logger;
    private readonly INotificationsHandler _notificationsHandler;
    private readonly StartDelayConfiguration _configuration;
}
