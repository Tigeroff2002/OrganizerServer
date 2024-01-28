using Microsoft.EntityFrameworkCore;

using Models;

namespace PostgreSQL.Abstractions;

public interface IRepositoryContext
{
    DbSet<User> Users { get; }

    DbSet<UserDeviceMap> UserDevices { get; }

    DbSet<Group> Groups { get; }

    DbSet<Event> Events { get; }

    DbSet<UserTask> Tasks { get; }

    DbSet<Snapshot> Snapshots { get; }

    DbSet<Issue> Issues { get; }

    DbSet<GroupingUsersMap> GroupingUsersMaps { get; }

    DbSet<EventsUsersMap> EventsUsersMaps { get; }

    void SaveChanges();
}
