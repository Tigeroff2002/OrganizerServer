using Microsoft.EntityFrameworkCore;

using Models;
using Models.Enums;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace PostgreSQL;

public class CalendarDataContext : DbContext
{
    public virtual DbSet<User> Users { get; set; } = default!;

    public virtual DbSet<Group> Groups { get; set; } = default!;

    public virtual DbSet<Event> Events { get; set; } = default!;

    public virtual DbSet<UserTask> Tasks { get; set; } = default!;

    public virtual DbSet<Report> Reports { get; set; } = default!;

    [Obsolete]
    static CalendarDataContext()
        => NpgsqlConnection.GlobalTypeMapper
            .MapEnum<EventType>()
            .MapEnum<ActivityKind>()
            .MapEnum<GroupType>()
            .MapEnum<TaskType>();

    public CalendarDataContext(DbContextOptions<CalendarDataContext> options)
        : base(options)
    {
        //Database.EnsureCreated();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSnakeCaseNamingConvention();

    protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
    {
        _ = modelBuilder
            .HasPostgresEnum<EventType>()
            .HasPostgresEnum<ActivityKind>()
            .HasPostgresEnum<GroupType>()
            .HasPostgresEnum<TaskType>();

        CreateUsersModels(modelBuilder);
        CreateGroupsModels(modelBuilder);
        CreateEventsModels(modelBuilder);
        CreateTasksModels(modelBuilder);
        CreateReportsModels(modelBuilder);

        CreateUserGroupMapsModels(modelBuilder);
        CreateUserEventsMapsModels(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void CreateUsersModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<User>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<User>()
            .Property(x => x.UserName);

        _ = modelBuilder.Entity<User>()
            .Property(x => x.Email);

        _ = modelBuilder.Entity<User>()
            .Property(x => x.Password);

        _ = modelBuilder.Entity<User>()
            .Property(x => x.PhoneNumber);

        _ = modelBuilder.Entity<User>()
            .Property(x => x.AuthToken);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.TasksForImplementation)
            .WithOne(x => x.Implementer);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.Reports)
            .WithOne(x => x.User);
    }

    public static void CreateGroupsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Group>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Group>()
            .Property(x => x.GroupName);

        _ = modelBuilder.Entity<Group>()
            .Property(x => x.Type);
    }

    public static void CreateEventsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Event>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Event>()
            .Property(x => x.Caption);

        _ = modelBuilder.Entity<Event>()
            .Property(x => x.Description);

        _ = modelBuilder.Entity<Event>()
            .Property(x => x.ScheduledStart);

        _ = modelBuilder.Entity<Event>()
            .Property(x => x.Duration);

        _ = modelBuilder.Entity<Event>()
            .Property(x => x.EventType);

        _ = modelBuilder.Entity<Event>()
            .Property(x => x.ActivityKind);

        _ = modelBuilder.Entity<Event>()
            .HasOne(x => x.Manager)
            .WithMany(x => x.ManagedEvents);
    }

    public static void CreateTasksModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<UserTask>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<UserTask>()
            .Property(x => x.Caption);

        _ = modelBuilder.Entity<UserTask>()
            .Property(x => x.Description);

        _ = modelBuilder.Entity<UserTask>()
            .Property(x => x.TaskType);
    }

    private static void CreateReportsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Report>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.Caption);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.Description);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.BeginMoment);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.EndMoment);
    }

    private static void CreateUserGroupMapsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<UserGroupMap>()
            .HasKey(
                map => new { map.UserId, map.GroupId });

        _ = modelBuilder.Entity<UserGroupMap>()
            .HasOne<User>(x => x.User)
            .WithMany(x => x.UserGroupMaps)
            .HasForeignKey(x => x.UserId);

        _ = modelBuilder.Entity<UserGroupMap>()
            .HasOne<Group>(x => x.Group)
            .WithMany(x => x.UserGroupMaps)
            .HasForeignKey(x => x.GroupId);
    }

    private static void CreateUserEventsMapsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<UserEventMap>()
            .HasKey(
                map => new { map.UserId, map.EventId });

        _ = modelBuilder.Entity<UserEventMap>()
            .HasOne<User>(x => x.User)
            .WithMany(x => x.UserEventMaps)
            .HasForeignKey(x => x.UserId);

        _ = modelBuilder.Entity<UserEventMap>()
            .HasOne<Event>(x => x.Event)
            .WithMany(x => x.UserEventMaps)
            .HasForeignKey(x => x.EventId);
    }
}
