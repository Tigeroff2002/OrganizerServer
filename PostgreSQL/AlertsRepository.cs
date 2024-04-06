using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Models;

using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class AlertsRepository : IAlertsRepository
{
    public AlertsRepository(
        IServiceProvider provider,
        ILogger<IssuesRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Alerts repository was created just now");
    }

    public async Task AddAsync(Alert alert, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(alert);

        token.ThrowIfCancellationRequested();

        await _repositoryContext.Alerts.AddAsync(alert, token);
    }

    public async Task<Alert?> GetAlertByIdAsync(int alertId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Alerts
            .FirstOrDefaultAsync(x => x.Id == alertId);
    }

    public async Task<List<Alert>> GetAllAlertsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Alerts.ToListAsync(token);
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of alerts were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
