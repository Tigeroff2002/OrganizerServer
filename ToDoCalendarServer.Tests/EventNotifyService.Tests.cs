using FluentAssertions;
using Logic.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ToDoCalendarServer.Services;
using Xunit;

namespace ToDoCalendarServer.Tests;

public sealed class EventNotifyServiceTests
    : IClassFixture<WebApplicationFactory<Startup>>
{
    public EventNotifyServiceTests(
        WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = $"{nameof(EventNotifyService)} can be created.")]
    [Trait("Category", "Unit")]
    public void CanBeCreated()
    {
        // Arrange
        var logger = Mock.Of<ILogger<EventNotifyService>>();
        var configuration = Options.Create<StartDelayConfiguration>(new(){ StartDelayMs = 5000 });
        var notifyHandler = Mock.Of<IEventNotificationsHandler>(
            MockBehavior.Strict);

        // Act
        var exception = Record.Exception(() =>
            new EventNotifyService(
                logger,
                configuration,
                notifyHandler));

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = $"{nameof(EventNotifyService)}" +
        $" can not be created without logger.")]
    [Trait("Category", "Unit")]
    public void CanNotBeCreatedWithoutLogger()
    {
        // Arrange
        var notifyHandler = Mock.Of<IEventNotificationsHandler>(
            MockBehavior.Strict);
        var configuration = Options.Create<StartDelayConfiguration>(new() { StartDelayMs = 5000 });

        // Act
        var exception = Record.Exception(() =>
            new EventNotifyService(null!, configuration, notifyHandler));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentNullException>();
    }

    [Fact(DisplayName = $"{nameof(EventNotifyService)}" +
        $" can not be created without configuration.")]
    [Trait("Category", "Unit")]
    public void CanNotBeCreatedWithoutConfiguration()
    {
        // Arrange
        var notifyHandler = Mock.Of<IEventNotificationsHandler>(
            MockBehavior.Strict);
        var logger = Mock.Of<ILogger<EventNotifyService>>();

        // Act
        var exception = Record.Exception(() =>
            new EventNotifyService(logger, null!, notifyHandler));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentNullException>();
    }

    [Fact(DisplayName = $"{nameof(EventNotifyService)}" +
        $" can not be created without notifications handler.")]
    [Trait("Category", "Unit")]
    public void CanNotBeCreatedWithoutNotificationsHandler()
    {
        // Arrange
        var logger = Mock.Of<ILogger<EventNotifyService>>();
        var configuration = Options.Create<StartDelayConfiguration>(new() { StartDelayMs = 5000 });

        // Act
        var exception = Record.Exception(() =>
            new EventNotifyService(
                logger,
                configuration,
                null!));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<ArgumentNullException>();
    }

    [Fact(DisplayName = $"{nameof(EventNotifyService)}" +
        $" can successfully build.")]
    [Trait("Category", "Unit")]
    public void CanBuildService()
    {
        // Arrange
        var hostBuilder =
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(builder =>
                    builder.UseStartup<Startup>())
                .UseDefaultServiceProvider((c, o) => o.ValidateOnBuild = true);

        // Act
        var exception = Record.Exception(hostBuilder.Build);

        // Assert
        exception.Should().BeNull();
    }

    [Fact(DisplayName = $"{nameof(EventNotifyService)}" +
        $" can health check.")]
    [Trait("Category", "Integration")]
    public async Task CanHealthCheckAsync()
    {
        // Arrange
        using var client = _factory.CreateClient();

        string? response = null;

        // Act
        var exception =
            await Record.ExceptionAsync(async () =>
                response = await client.GetStringAsync("/hc"));

        // Assert
        exception.Should().BeNull();
        response.Should().NotBeNullOrWhiteSpace()
            .And.Be("Healthy");
    }

    private readonly WebApplicationFactory<Startup> _factory;
}
