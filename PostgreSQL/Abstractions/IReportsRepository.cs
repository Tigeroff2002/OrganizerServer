using Models;

namespace PostgreSQL.Abstractions; 

public interface IReportsRepository : IRepository
{
    Task AddAsync(Report report, CancellationToken token);

    Task<Report?> GetReportByIdAsync(int reporterId, CancellationToken token);

    Task<List<Report>> GetAllReportsAsync(CancellationToken token);

    Task DeleteAsync(int reporterId, CancellationToken token);

    Task UpdateAsync(Report report, CancellationToken token);
}
