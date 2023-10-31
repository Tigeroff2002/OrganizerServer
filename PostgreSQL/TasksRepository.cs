using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class TasksRepository
    : ITasksRepository
{
    public TasksRepository(
        IServiceProvider provider,
        ILogger<TasksRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Tasks repository was created just now");
    }

    public async Task AddAsync(UserTask task, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(task);

        token.ThrowIfCancellationRequested();

        await _repositoryContext.Tasks.AddAsync(task, token);
    }

    public async Task<UserTask?> GetTaskByIdAsync(int taskId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);
    }

    public Task<List<UserTask>> GetAllTasksAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return _repositoryContext.Tasks.ToListAsync(token);
    }

    public async Task DeleteAsync(int taskId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var task = await _repositoryContext.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);

        if (task != null)
        {
            _repositoryContext.Tasks.Entry(task).State = EntityState.Deleted;
        }
    }

    public async Task UpdateAsync(UserTask task, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(task);

        token.ThrowIfCancellationRequested();

        var localTask = await _repositoryContext.Tasks.FirstOrDefaultAsync(x => x.Id == task.Id);

        if (localTask != null)
        {
            _repositoryContext.Tasks.Entry(localTask).CurrentValues.SetValues(task);
        }

        localTask = task.Map<UserTask>();
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of tasks were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
