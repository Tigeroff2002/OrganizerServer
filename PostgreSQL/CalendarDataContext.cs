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
            .MapEnum<EventStatus>()
            .MapEnum<GroupType>()
            .MapEnum<TaskType>()
            .MapEnum<TaskCurrentStatus>()
            .MapEnum<ReportType>()
            .MapEnum<DecisionType>();

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
            .HasPostgresEnum<EventStatus>()
            .HasPostgresEnum<GroupType>()
            .HasPostgresEnum<TaskType>()
            .HasPostgresEnum<TaskCurrentStatus>()
            .HasPostgresEnum<ReportType>()
            .HasPostgresEnum<DecisionType>();

        CreateUsersModels(modelBuilder);
        CreateGroupsModels(modelBuilder);
        CreateEventsModels(modelBuilder);
        CreateTasksModels(modelBuilder);
        CreateReportsModels(modelBuilder);

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
            .HasMany(x => x.ReportedTasks)
            .WithOne(x => x.Reporter);

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
            .Property(x => x.Status);

        _ = modelBuilder.Entity<Event>()
            .HasOne(x => x.RelatedGroup)
            .WithMany(x => x.RelatedEvents);

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

        _ = modelBuilder.Entity<UserTask>()
            .Property(x => x.TaskStatus);
    }

    private static void CreateReportsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Report>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.Description);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.ReportType);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.BeginMoment);

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.EndMoment);
    }

    private static void CreateGroupingUsersMapModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<GroupingUsersMap>()
            .HasKey(map => new { map.UserId, map.GroupId });

        _ = modelBuilder.Entity<GroupingUsersMap>()
            .HasOne(map => map.User)
            .WithMany(gm => gm.GroupingMaps)
            .HasForeignKey(map => map.UserId);

        _ = modelBuilder.Entity<GroupingUsersMap>()
            .HasOne(map => map.Group)
            .WithMany(pm => pm.ParticipantsMap)
            .HasForeignKey(map => map.GroupId);

        _ = modelBuilder.Entity<GroupingUsersMap>()
            .ToTable("groups_users_map");
    }

    private static void CreateEventUsersMapModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<EventsUsersMap>()
            .HasKey(map => new { map.UserId, map.EventId });

        _ = modelBuilder.Entity<EventsUsersMap>()
            .HasOne(map => map.User)
            .WithMany(em => em.EventMaps)
            .HasForeignKey(map => map.UserId);

        _ = modelBuilder.Entity<EventsUsersMap>()
            .HasOne(map => map.Event)
            .WithMany(gm => gm.GuestsMap)
            .HasForeignKey(map => map.EventId);

        _ = modelBuilder.Entity<EventsUsersMap>()
            .Property(x => x.DecisionType);

        _ = modelBuilder.Entity<GroupingUsersMap>()
            .ToTable("events_users_map");
    }
}
