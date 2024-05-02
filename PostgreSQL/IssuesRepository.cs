using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Models.StorageModels;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class IssuesRepository
    : IIssuesRepository
{
    public IssuesRepository(
        IServiceProvider provider,
        ILogger<IssuesRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Issues repository was created just now");
    }

    public async Task AddAsync(Issue issue, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(issue);

        token.ThrowIfCancellationRequested();

        await _repositoryContext.Issues.AddAsync(issue, token);
    }

    public async Task<Issue?> GetIssueByIdAsync(int issueId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Issues
            .FirstOrDefaultAsync(x => x.Id == issueId);
    }

    public async Task<List<Issue>> GetAllIssuesAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Issues.ToListAsync(token);
    }

    public async Task UpdateAsync(Issue issue, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(issue);

        token.ThrowIfCancellationRequested();

        var localIssue = await _repositoryContext.Issues
            .FirstOrDefaultAsync(x => x.Id == issue.Id);

        if (localIssue != null)
        {
            _repositoryContext.Issues.Entry(localIssue).CurrentValues.SetValues(issue);
        }

        localIssue = issue.Map<Issue>();
    }

    public async Task DeleteAsync(int issueId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var issue = await _repositoryContext.Issues
            .FirstOrDefaultAsync(x => x.Id == issueId);

        if (issue != null)
        {
            _repositoryContext.Issues.Entry(issue).State = EntityState.Deleted;
        }
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of issues were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
