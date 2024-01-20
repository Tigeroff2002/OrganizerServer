using Models;

namespace PostgreSQL.Abstractions;

public interface IIssuesRepository : IRepository
{
    Task AddAsync(Issue issue, CancellationToken token);

    Task<Issue?> GetIssueByIdAsync(int issueId, CancellationToken token);

    Task<List<Issue>> GetAllIssuesAsync(CancellationToken token);

    Task UpdateAsync(Issue issue, CancellationToken token);

    Task DeleteAsync(int issueId, CancellationToken token);
}
