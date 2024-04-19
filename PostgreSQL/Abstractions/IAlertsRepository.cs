using Models;
using Models.StorageModels;

namespace PostgreSQL.Abstractions;

public interface IAlertsRepository : IRepository
{
    Task AddAsync(Alert alert, CancellationToken token);

    Task<Alert?> GetAlertByIdAsync(int alertId, CancellationToken token);

    Task<List<Alert>> GetAllAlertsAsync(CancellationToken token);

    Task MarkAsAlertedAsync(int alertId, CancellationToken token);
}
