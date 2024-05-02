using Microsoft.EntityFrameworkCore;

using Logic;
using Logic.Abstractions;
using ToDoCalendarServer.Services;

using PostgreSQL;
using PostgreSQL.Abstractions;
using Logic.Transport.Abstractions;
using Models.BusinessModels;
using Logic.Transport;
using Logic.Authentification;
using Microsoft.AspNetCore.Authentication;
using Contracts.Response;
using Logic.Transport.Senders;
using Models.UserActionModels;
using StackExchange.Redis;
using Models.RedisEventModels;
using Microsoft.Extensions.DependencyInjection;
using Logic.Transport.Serialization;
using Logic.ControllerHandlers;
using Logic.Notifications;
using Logic.Transport.Receivers;
using OrganizerServer.Services;

namespace ToDoCalendarServer;

public static class Registrations
{
    public static IServiceCollection AddHostedServices(
        this IServiceCollection services)
        => services
            .AddHostedService<EventNotifyService>()
            .AddHostedService<RedisProcessorService>();

    public static IServiceCollection AddLogic(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddNotificationLogic()
            .AddNotificationsTransport()

            .AddSingleton<IUsersDataHandler, UsersDataHandler>()
            .AddSingleton<IGroupsHandler, GroupsHandler>()
            .AddSingleton<IEventsHandler, EventsHandler>()
            .AddSingleton<ITasksHandler, TasksHandler>()
            .AddSingleton<IIssuesHandler, IssuesHandler>()
            .AddSingleton<ISnapshotsHandler, SnapshotsHandler>()
            .AddSingleton<IAlertsReceiverHandler, AlertsReceiverHandler>()

            .AddSingleton<IRedisProcessor, RedisProcessor>()
            .AddSingleton<IRedisMessagesReceiver, RedisMessagesReceiver>()
            .AddSingleton<IRedisEventsAliaser, RedisEventsAliaser>()

            .AddRedisCache(configuration)
            .AddConfigurations(configuration);

    public static IServiceCollection AddNotificationLogic(
        this IServiceCollection services)
        => services
            .AddSingleton<INotificationsHandler, NotificationsHandler>()
            .AddSingleton<IEventNotificationsHandler, EventNotificationsHandler>()
            .AddSingleton<IAlertsNotificationsHandler, AlertsNotificationsHandler>();

    public static IServiceCollection AddNotificationsTransport(
        this IServiceCollection services)
        => services
            .AddSingleton<ISMTPSender, SMTPSender>()
            .AddSingleton<IPushNotificationsSender, PushNotificationsSender>();

    public static IServiceCollection AddSerialization(
        this IServiceCollection services)
        => services
            .AddSingleton<IDeserializer<UserLoginData>, UsersLoginDataDeserializer>()
            .AddSingleton<IDeserializer<UserRegistrationData>, UsersRegistrationDataDeserializer>()
            .AddSingleton<IDeserializer<UserInfoById>, UserInfoByIdDeserializer>()
            .AddSingleton<IDeserializer<UserLogoutDeviceById>, UserLogoutDeviceByIdDeserializer>()
            .AddSingleton<ISerializer<UserInfoContent>, UserInfoSerializer>()
            .AddSingleton<ISerializer<UserRelatedEvent>, RedisEventSerializer>()
            .AddSingleton<IDeserializer<UserRelatedEvent>, RedisEventDeserializer>();

    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDbContext<CalendarDataContext>((sp, dbOpt) =>
                dbOpt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
            .AddScoped<IRepositoryContext, RepositoryContext>()
            .AddSingleton<IUsersRepository, UsersRepository>()
            .AddSingleton<IUserDevicesRepository, UserDevicesRepository>()
            .AddSingleton<IGroupsRepository, GroupsRepository>()
            .AddSingleton<IEventsRepository, EventsRepository>()
            .AddSingleton<ITasksRepository, TasksRepository>()
            .AddSingleton<ISnapshotsRepository, SnapshotsRepository>()
            .AddSingleton<IGroupingUsersMapRepository, GroupingUsersMapRepository>()
            .AddSingleton<IEventsUsersMapRepository, EventsUsersMapRepository>()
            .AddSingleton<IIssuesRepository, IssuesRepository>()
            .AddSingleton<IAlertsRepository, AlertsRepository>()
            .AddSingleton<IChatRepository, ChatRepository>()
            .AddSingleton<IMessageRepository, MessageRepository>()

            .AddSingleton<ICommonUsersUnitOfWork, CommonUsersUnitOfWork>()
            .AddSingleton<IUsersMessagingUnitOfWork, UsersMessagingUnitOfWork>()

            .AddNpgsqlDataSource(configuration.GetConnectionString("DefaultConnection")!);
            

    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(
                _ =>
                {
                    var connectionString = configuration.GetConnectionString("RedisConnection")!;

                    return ConnectionMultiplexer.Connect(connectionString);
                    /*
                     * sudo apt install redis-server
                     * sudo systemctl restart redis.service
                     * sudo systemctl status redis
                     * redis-cli
                     */
                })
            .AddSingleton<IRedisRepository, RedisRepository>();

    public static IServiceCollection AddConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .Configure<SmtpConfiguration>(
                configuration.GetSection(nameof(SmtpConfiguration)))
            .Configure<RedisReadingConfiguration>(
                configuration.GetSection(nameof(RedisReadingConfiguration)))
            .Configure<NotificationConfiguration>(
                configuration.GetSection(nameof(NotificationConfiguration)))
            .Configure<RootConfiguration>(
                configuration.GetSection(nameof(RootConfiguration)))
            .Configure<StartDelayConfiguration>(
                configuration.GetSection(nameof(StartDelayConfiguration)))
            .Configure<AdsPushConfiguration>(
                configuration
                    .GetSection("AdsPush")
                    .GetSection("MyApp")
                    .GetSection("FirebaseCloudMessaging"));

    public static AuthenticationBuilder AddAuthBuilder(
        this IServiceCollection services)
    {
        return services.AddAuthentication(o =>
        {
            o.DefaultScheme = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme;
        })
        .AddScheme<TokenAuthentificationOptions, AuthentificationTokensHandler>(
            AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme, o => { });
    }
}
