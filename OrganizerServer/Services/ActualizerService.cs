using Logic.Abstractions;
using Logic.Notifications;
using Microsoft.Extensions.Options;
using ToDoCalendarServer.Services;

namespace OrganizerServer.Services;

public sealed class ActualizerService : BackgroundService
{
    public ActualizerService(
        ILogger<ActualizerService> logger,
        IActualizerHandler actualizerHandler,
        IOptions<StartDelayConfiguration> options) 
    {
        ArgumentNullException.ThrowIfNull(options);

        _configuration = options.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _actualizerHandler = actualizerHandler
            ?? throw new ArgumentNullException(nameof(actualizerHandler));

        _logger.LogInformation("Actualizer user info service has been created");
    }

    public override async Task StartAsync(CancellationToken stoppingToken)
    {
        Task.Delay(_configuration.StartDelayMs).GetAwaiter().GetResult();

        await base.StartAsync(stoppingToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _actualizerHandler.ActualizeAsync(stoppingToken);
    }

    private readonly ILogger _logger;
    private readonly IActualizerHandler _actualizerHandler;
    private readonly StartDelayConfiguration _configuration;
}
