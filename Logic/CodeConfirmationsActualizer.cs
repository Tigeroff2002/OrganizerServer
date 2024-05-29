using Logic.Abstractions;
using Microsoft.Extensions.Logging;

namespace Logic;

public sealed class CodeConfirmationsActualizer : 
    ICodeConfirmationsActualizer
{
    public CodeConfirmationsActualizer(
        IUserConfirmationCache cache,
        ILogger<CodeConfirmationsActualizer> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ActualizeEmailConfirmationsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Beginning next iteration of checking expired email confirmations");

            _cache.RemoveNotActual();

            await Task.Delay(DELAY);
        }
    }

    private static readonly TimeSpan DELAY = TimeSpan.FromSeconds(10);

    private readonly IUserConfirmationCache _cache;
    private readonly ILogger _logger;
}
