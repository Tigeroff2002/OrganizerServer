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

        _logger.LogInformation("Tasks repository was created just now");
    }

    public async Task AddAsync(UserTask task, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(task);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        await _repositoryContext.Tasks.AddAsync(task, token);

        SaveChanges();
    }

    public async Task<UserTask?> GetTaskByIdAsync(int taskId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return await _repositoryContext.Tasks.FindAsync(
            new object?[]
            {
                taskId
            }, token);
    }

    public Task<List<UserTask>> GetAllTasksAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        return _repositoryContext.Tasks.ToListAsync(token);
    }

    public async Task DeleteAsync(int taskId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var task = await _repositoryContext.Tasks.FindAsync(
            new object?[]
            {
                taskId
            }, token);

        if (task != null)
        {
            _repositoryContext.Tasks.Entry(task).State = EntityState.Deleted;
        }

        SaveChanges();
    }

    public async Task UpdateAsync(Task task, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(task);

        token.ThrowIfCancellationRequested();

        using var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        var localTask = await _repositoryContext.Tasks.FindAsync(
            new object?[]
            {
                task.Id
            }, token);

        if (localTask != null)
        {
            _repositoryContext.Tasks.Entry(localTask).CurrentValues.SetValues(task);
        }

        SaveChanges();
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of tasks were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
