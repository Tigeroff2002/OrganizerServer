using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using PostgreSQL.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSQL;

public sealed class RepositoryContext
    : IRepositoryContext
{
    /// <inheritdoc/>
    public DbSet<User> Users => _context.Users;

    /// <inheritdoc/>
    public DbSet<UserDeviceMap> UserDevices => _context.UserDevices;

    /// <inheritdoc/>
    public DbSet<Group> Groups => _context.Groups;

    /// <inheritdoc/>
    public DbSet<Event> Events => _context.Events;

    /// <inheritdoc/>
    public DbSet<UserTask> Tasks => _context.Tasks;

    /// <inheritdoc/>
    public DbSet<Snapshot> Snapshots => _context.Snapshots;

    /// <inheritdoc/>
    public DbSet<Issue> Issues => _context.Issues;

    /// <inheritdoc/>
    public DbSet<Alert> Alerts => _context.Alerts;

    /// <inheritdoc/>
    public DbSet<DirectChat> DirectChats => _context.DirectChats;

    /// <inheritdoc/>
    public DbSet<DirectMessage> Messages => _context.Messages;

    /// <inheritdoc/>
    public DbSet<GroupingUsersMap> GroupingUsersMaps => _context.GroupingUsersMaps;

    /// <inheritdoc/>
    public DbSet<EventsUsersMap> EventsUsersMaps => _context.EventsUsersMaps;

    /// <summary>
    /// Создаёт новый экземпляр <see cref="RepositoryContext"/>
    /// </summary>
    /// <param name="context">
    /// Контекст БД.
    /// </param>
    /// <param name="logger">
    /// Логгер.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Если один из обязательных входных
    /// параметров равен <see langword="null"/>
    /// </exception>
    public RepositoryContext(
        CalendarDataContext context,
        ILogger<RepositoryContext> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation(
            "A new instance of RepositoryContext was taken from scope just now");
    }

    /// <inheritdoc/>
    public void SaveChanges()
    {
        _logger.LogDebug("Save intermediate changes");

        _ = _context.SaveChanges();

        _logger.LogDebug("Intermediate changes sent to DB");
    }

    private readonly ILogger<RepositoryContext> _logger;
    private readonly CalendarDataContext _context;
}

