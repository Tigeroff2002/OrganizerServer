using AdsPush.Extensions;
using ToDoCalendarServer.Controllers;

namespace ToDoCalendarServer;

public class Startup
{
    /// <summary>
    /// Создает новый экземпляр <see cref="Startup"/>.
    /// </summary>
    /// <param name="configuration">
    /// Конфигурация сервиса.
    /// </param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Конфигурация.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Этот метод вызывается средой выполнения.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddControllers();

        _ = services.AddHttpClient();
        _ = services
            .AddLogging()
            .AddHostedServices()
            .AddLogic(Configuration)
            .AddSerialization()
            .AddStorage(Configuration)
            .AddHealthChecks();

        _ = services.AddAuthBuilder();

        _ = services.AddAdsPush(Configuration);

        _ = services.AddHostedServices();
    }

    /// <summary>
    /// Этот метод вызывается средой выполнения.
    /// </summary>
    public void Configure(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        _ = app.UseCustomExceptionsHandling();

        _ = app
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                _ = endpoints.MapHealthChecks("/hc");
                _ = endpoints.MapControllers();
            });
    }
}