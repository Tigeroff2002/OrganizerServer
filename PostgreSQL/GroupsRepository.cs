﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Models.StorageModels;
using PostgreSQL.Abstractions;

namespace PostgreSQL;

public sealed class GroupsRepository
    : IGroupsRepository
{
    public GroupsRepository(
        IServiceProvider provider,
        ILogger<GroupsRepository> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _repositoryContext = scope.ServiceProvider.GetRequiredService<IRepositoryContext>();

        _logger.LogInformation("Groups repository was created just now");
    }

    public async Task AddAsync(Group group, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(group);

        token.ThrowIfCancellationRequested();

        await _repositoryContext.Groups.AddAsync(group, token);
    }

    public async Task<Group?> GetGroupByIdAsync(int groupId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Groups.FirstOrDefaultAsync(x => x.Id == groupId);
    }

    public async Task<List<Group>> GetAllGroupsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        return await _repositoryContext.Groups.ToListAsync(token);
    }

    public async Task DeleteAsync(int groupId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var group = await _repositoryContext.Groups.FirstOrDefaultAsync(x => x.Id == groupId);

        if (group != null)
        {
            _repositoryContext.Groups.Entry(group).State = EntityState.Deleted;
        }
    }

    public async Task UpdateAsync(Group group, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(group);

        token.ThrowIfCancellationRequested();

        var localGroup = await _repositoryContext.Groups.FirstOrDefaultAsync(x => x.Id == group.Id);

        if (localGroup != null)
        {
            _repositoryContext.Groups.Entry(localGroup).CurrentValues.SetValues(group);
        }

        localGroup = group.Map<Group>();

        _logger.LogDebug($"Properties of group with id {localGroup!.Id} were modified");
    }

    public void SaveChanges()
    {
        _repositoryContext.SaveChanges();

        _logger.LogInformation("The changes of groups were sent to DB");
    }

    private readonly IServiceProvider _provider;
    private readonly IRepositoryContext _repositoryContext;
    private readonly ILogger _logger;
}
