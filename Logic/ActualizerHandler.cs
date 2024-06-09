using Logic.Abstractions;
using Logic.Notifications;
using Microsoft.Extensions.Logging;

namespace Logic;

public sealed class ActualizerHandler : IActualizerHandler
{
    public ActualizerHandler(
        IUserDevicesTokensActualizer tokensActualizer,
        ICodeConfirmationsActualizer codesActualizer,
        IExceptionHandler exceptionHandler,
        ILogger<ActualizerHandler> logger) 
    {
        _tokensActualizer = tokensActualizer
            ?? throw new ArgumentNullException(nameof(tokensActualizer));

        _codesActualizer = codesActualizer
            ?? throw new ArgumentNullException(nameof(codesActualizer));

        _exceptionHandler = exceptionHandler
            ?? throw new ArgumentNullException(nameof(exceptionHandler));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ActualizeAsync(CancellationToken token)
    {
        using var scope = _logger.BeginScope(
            "Beginning handling notifications");

        var notificationsTasks = new List<Task>
        {
            _tokensActualizer.ActualizeUserDevicesAsync(token),
            _codesActualizer.ActualizeEmailConfirmationsAsync(token)
        };

        try
        {

            await Task.WhenAll(
                notificationsTasks.Select(
                    task => Task.Run(async () => await task)));
        }
        catch (Exception ex)
        {
            await _exceptionHandler.HandleExceptionsAsync(ex, token);

            throw;
        }
    }

    private readonly IUserDevicesTokensActualizer _tokensActualizer;
    private readonly ICodeConfirmationsActualizer _codesActualizer;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly ILogger _logger;
}
