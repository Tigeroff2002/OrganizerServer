using Logic.Abstractions;
using Microsoft.Extensions.Options;

namespace ToDoCalendarServer.Services;

public class NotifyService : BackgroundService
{
    public NotifyService(
        ILogger<NotifyService> logger,
        IOptions<StartDelayConfiguration> options,
        INotificationsHandler notificationsHandler)
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration = options.Value;

        _notificationsHandler = notificationsHandler
            ?? throw new ArgumentNullException(nameof(notificationsHandler));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Notify service has been created");
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

    private readonly ILogger<NotifyService> _logger;
    private readonly INotificationsHandler _notificationsHandler;
    private readonly StartDelayConfiguration _configuration;
}
