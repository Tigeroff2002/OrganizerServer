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

namespace ToDoCalendarServer;

public static class Registrations
{
    public static IServiceCollection AddHostedServices(
        this IServiceCollection services)
        => services.AddHostedService<NotifyService>();

    public static IServiceCollection AddLogic(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .AddSingleton<INotificationsHandler, NotificationsHandler>()
            .AddSingleton<IUsersDataHandler, UsersDataHandler>()
            .AddSingleton<IUsersCodeConfirmer, UsersCodeConfirmer>()
            .AddConfiguration(configuration)
            .AddSingleton<IGroupsHandler, GroupsHandler>()
            .AddSingleton<IEventsHandler, EventsHandler>()
            .AddSingleton<ITasksHandler, TasksHandler>()
            .AddSingleton<IReportsHandler, ReportsHandler>();

    public static IServiceCollection AddSerialization(this IServiceCollection services)
        => services
            .AddSingleton<IDeserializer<UserLoginData>, UsersLoginDataDeserializer>()
            .AddSingleton<IDeserializer<UserRegistrationData>, UsersRegistrationDataDeserializer>()
            .AddSingleton<IDeserializer<UserInfoById>, UserInfoByIdDeserializer>()
            .AddSingleton<ISerializer<User>, UserInfoSerializer>();

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
            .AddSingleton<IReportsRepository, ReportsRepository>();

    public static IServiceCollection AddConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .Configure<SmtpConfiguration>(
                configuration.GetSection(nameof(SmtpConfiguration)));

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
