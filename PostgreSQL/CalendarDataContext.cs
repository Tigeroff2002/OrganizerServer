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
        Database.EnsureCreated();

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
            .HasMany(x => x.Groups)
            .WithMany(x => x.Participants)
            .UsingEntity("users_groups_map");

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.TasksForImplementation)
            .WithOne(x => x.Implementer);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.Reports)
            .WithOne(x => x.User);

        _ = modelBuilder.Entity<User>()
            .HasMany(x => x.Events)
            .WithMany(x => x.Guests)
            .UsingEntity("users_events_calendar");
    }

    public static void CreateGroupsModels(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Group>()
            .HasKey(x => x.Id);

        _ = modelBuilder.Entity<Group>()
            .Property(x => x.GroupName);

        _ = modelBuilder.Entity<Group>()
            .Property(x => x.Type);

        /*
        _ = modelBuilder.Entity<Group>()
            .HasMany(x => x.Participants)
            .WithMany(x => x.Groups)
            .UsingEntity("users_groups_map");
         */
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
            .Property(x => x.ManagerId);

        _ = modelBuilder.Entity<Event>()
            .HasOne(x => x.Manager)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);

        /*
        _ = modelBuilder.Entity<Event>()
            .HasMany(x => x.Guests)
            .WithMany(x => x.Events)
            .UsingEntity("users_events_calendar");
        */
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
            .Property(x => x.ReporterId);

        _ = modelBuilder.Entity<UserTask>()
            .HasOne(x => x.Reporter)
            .WithMany()
            .HasForeignKey(x => x.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<UserTask>()
            .Property(x => x.ImplementerId);

        _ = modelBuilder.Entity<UserTask>()
            .HasOne(x => x.Implementer)
            .WithMany()
            .HasForeignKey(x => x.ImplementerId)
            .OnDelete(DeleteBehavior.Cascade);
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

        _ = modelBuilder.Entity<Report>()
            .Property(x => x.UserId);

        _ = modelBuilder.Entity<Report>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
