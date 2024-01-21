using Microsoft.EntityFrameworkCore;

using Logic;
using Logic.Abstractions;
using ToDoCalendarServer.Services;

using PostgreSQL;
using PostgreSQL.Abstractions;
using Logic.Transport.Abstractions;
using Models.BusinessModels;
using Logic.Transport;
using Models;
using Logic.Authentification;
using Microsoft.AspNetCore.Authentication;
using Contracts.Response;

namespace ToDoCalendarServer;

public static class Registrations
{
    public static IServiceCollection AddHostedServices(
        this IServiceCollection services)
        => services.AddHostedService<EventNotifyService>();

    public static IServiceCollection AddLogic(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddSingleton<IEventNotificationsHandler, EventNotificationsHandler>()
            .AddSingleton<IUsersDataHandler, UsersDataHandler>()
            .AddSingleton<IUserEmailConfirmer, UserEmailConfirmer>()
            .AddConfigurations(configuration)
            .AddSingleton<ISnapshotsHandler, SnapshotsHandler>();

    public static IServiceCollection AddSerialization(this IServiceCollection services)
        => services
            .AddSingleton<IDeserializer<UserLoginData>, UsersLoginDataDeserializer>()
            .AddSingleton<IDeserializer<UserRegistrationData>, UsersRegistrationDataDeserializer>()
            .AddSingleton<IDeserializer<UserInfoById>, UserInfoByIdDeserializer>()
            .AddSingleton<ISerializer<UserInfoContent>, UserInfoSerializer>();

    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddDbContext<CalendarDataContext>((sp, dbOpt) =>
                dbOpt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
            .AddScoped<IRepositoryContext, RepositoryContext>()
            .AddSingleton<IUsersRepository, UsersRepository>()
            .AddSingleton<IGroupsRepository, GroupsRepository>()
            .AddSingleton<IEventsRepository, EventsRepository>()
            .AddSingleton<ITasksRepository, TasksRepository>()
            .AddSingleton<ISnapshotsRepository, SnapshotsRepository>()
            .AddSingleton<IGroupingUsersMapRepository, GroupingUsersMapRepository>()
            .AddSingleton<IEventsUsersMapRepository, EventsUsersMapRepository>()
            .AddSingleton<IIssuesRepository, IssuesRepository>();

    public static IServiceCollection AddConfigurations(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .Configure<SmtpConfiguration>(
                configuration.GetSection(nameof(SmtpConfiguration)))
            .Configure<NotificationConfiguration>(
                configuration.GetSection(nameof(NotificationConfiguration)))
            .Configure<RootConfiguration>(
                configuration.GetSection(nameof(RootConfiguration)));

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
