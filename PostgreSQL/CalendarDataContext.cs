using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

using Models;
using Models.Enums;
using Models.StorageModels;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace PostgreSQL;

public class CalendarDataContext : DbContext
{
    public virtual DbSet<User> Users { get; set; } = default!;

    public virtual DbSet<UserDeviceMap> UserDevices { get; set; }

    public virtual DbSet<Group> Groups { get; set; } = default!;

    public virtual DbSet<Event> Events { get; set; } = default!;

    public virtual DbSet<UserTask> Tasks { get; set; } = default!;

    public virtual DbSet<Snapshot> Snapshots { get; set; } = default!;

    public virtual DbSet<Issue> Issues { get; set; } = default!;

    public virtual DbSet<Alert> Alerts { get; set; } = default!;

    public virtual DbSet<DirectChat> DirectChats { get; set; }

    public virtual DbSet<DirectMessage> Messages { get; set; }

    // дополнительные сущности для отношений many-to-many
    public virtual DbSet<GroupingUsersMap> GroupingUsersMaps { get; set; } = default!;

    public virtual DbSet<EventsUsersMap> EventsUsersMaps { get; set; } = default!;

    [Obsolete]
    static CalendarDataContext()
        => NpgsqlConnection.GlobalTypeMapper
            .MapEnum<EventType>()
            .MapEnum<EventStatus>()
            .MapEnum<GroupType>()
            .MapEnum<TaskType>()
            .MapEnum<TaskCurrentStatus>()
            .MapEnum<SnapshotType>()
            .MapEnum<SnapshotAuditType>()
            .MapEnum<DecisionType>()
            .MapEnum<IssueType>()
            .MapEnum<UserRole>()
            .MapEnum<IssueStatus>();

    public CalendarDataContext(DbContextOptions<CalendarDataContext> options)
        : base(options)
    {
        // Database.EnsureCreated();

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
            .HasPostgresEnum<SnapshotType>()
            .HasPostgresEnum<SnapshotAuditType>()
            .HasPostgresEnum<DecisionType>()
            .HasPostgresEnum<IssueType>()
            .HasPostgresEnum<UserRole>()
            .HasPostgresEnum<IssueStatus>();

        CreateUsersModels(modelBuilder);
        CreateUserDevicesMap(modelBuilder);
        CreateGroupsModels(modelBuilder);
        CreateEventsModels(modelBuilder);
        CreateTasksModels(modelBuilder);
        CreateSnapshotsModels(modelBuilder);
        CreateIssuesModels(modelBuilder);
        CreateAlertsModels(modelBuilder);
        CreateDirectChatsModels(modelBuilder);
        CreateMessagesModels(modelBuilder);
        CreateGroupingUsersMapModels(modelBuilder);
        CreateEventUsersMapModels(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void CreateUsersModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<User>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<User>()
            .Property(x => x.Role);

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
            .Property(x => x.AccountCreation);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.ReportedTasks)
            .WithOne(x => x.Reporter)
            .HasForeignKey(x => x.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.TasksForImplementation)
            .WithOne(x => x.Implementer)
            .HasForeignKey(x => x.ImplementerId);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.Snapshots)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.Issues)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.Devices)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.DirectChatsForUserHome)
            .WithOne(x => x.User1)
            .HasForeignKey(x => x.User1Id)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.DirectChatsForUserAway)
            .WithOne(x => x.User2)
            .HasForeignKey(x => x.User2Id)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public static void CreateGroupsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Group>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Group>()
            .Property(x => x.GroupName);

        _ = modelBuilder.Entity<Group>()
            .Property(x => x.Type);

        _ = modelBuilder.Entity<Group>()
            .HasOne(x => x.Manager)
            .WithMany(x => x.ManagedGroups)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);
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
            .WithMany(x => x.RelatedEvents)
            .HasForeignKey(x => x.RelatedGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<Event>()
            .HasOne(x => x.Manager)
            .WithMany(x => x.ManagedEvents)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);
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

    private static void CreateSnapshotsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Snapshot>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Snapshot>()
            .Property(x => x.Description);

        _ = modelBuilder.Entity<Snapshot>()
            .Property(x => x.SnapshotType);

        _ = modelBuilder.Entity<Snapshot>()
            .Property(x => x.SnapshotAuditType);

        _ = modelBuilder.Entity<Snapshot>()
            .Property(x => x.CreateMoment);

        _ = modelBuilder.Entity<Snapshot>()
            .Property(x => x.BeginMoment);

        _ = modelBuilder.Entity<Snapshot>()
            .Property(x => x.EndMoment);
    }

    private static void CreateUserDevicesMap(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<UserDeviceMap>()
            .HasKey(map => new { map.UserId, map.FirebaseToken });

        _ = modelBuilder.Entity<UserDeviceMap>()
            .Property(map => map.UserId);

        _ = modelBuilder.Entity<UserDeviceMap>()
            .Property(map => map.FirebaseToken);

        _ = modelBuilder.Entity<UserDeviceMap>()
            .Property(map => map.TokenSetMoment);

        _ = modelBuilder.Entity<UserDeviceMap>()
            .Property(map => map.IsActive);
    }

    private static void CreateIssuesModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Issue>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Issue>()
            .Property(x => x.Title);

        _ = modelBuilder.Entity<Issue>()
            .Property(x => x.IssueType);

        _ = modelBuilder.Entity<Issue>()
            .Property(x => x.Status);

        _ = modelBuilder.Entity<Issue>()
            .Property(x => x.Description);

        _ = modelBuilder.Entity<Issue>()
            .Property(x => x.ImgLink);

        _ = modelBuilder.Entity<Issue>()
            .Property(x => x.IssueMoment);
    }

    private static void CreateAlertsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Alert>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Alert>()
            .Property(x => x.Title);

        _ = modelBuilder.Entity<Alert>()
            .Property(x => x.Description);

        _ = modelBuilder.Entity<Alert>()
            .Property(x => x.Moment);

        _ = modelBuilder.Entity<Alert>()
            .Property(x => x.IsAlerted);
    }

    private static void CreateDirectChatsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<DirectChat>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<DirectChat>()
            .Property(x => x.Caption);

        _ = modelBuilder.Entity<DirectChat>()
            .Property(x => x.CreateTime);

        _ = modelBuilder.Entity<DirectChat>()
            .HasMany(x => x.DirectMessages)
            .WithOne(x => x.Chat)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void CreateMessagesModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<DirectMessage>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<DirectMessage>()
            .Property(x => x.SendTime);

        _ = modelBuilder.Entity<DirectMessage>()
            .Property(x => x.Text);

        _ = modelBuilder.Entity<DirectMessage>()
            .Property(x => x.isEdited);
    }

    private static void CreateGroupingUsersMapModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<GroupingUsersMap>()
            .HasKey(map => new { map.UserId, map.GroupId });

        _ = modelBuilder.Entity<GroupingUsersMap>()
            .HasOne(map => map.User)
            .WithMany(gm => gm.GroupingMaps)
            .HasForeignKey(map => map.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<GroupingUsersMap>()
            .HasOne(map => map.Group)
            .WithMany(pm => pm.ParticipantsMap)
            .HasForeignKey(map => map.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

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
            .HasForeignKey(map => map.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<EventsUsersMap>()
            .HasOne(map => map.Event)
            .WithMany(gm => gm.GuestsMap)
            .HasForeignKey(map => map.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<EventsUsersMap>()
            .Property(x => x.DecisionType);

        _ = modelBuilder.Entity<EventsUsersMap>()
            .ToTable("events_users_map");
    }
}
