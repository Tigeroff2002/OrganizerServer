using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.BusinessModels;
using Models.StorageModels;
using PostgreSQL.Abstractions;
using System.Net;
using System.Text.Json;

namespace Logic;

public sealed class CustomExceptionsHandler
{
    public CustomExceptionsHandler(
        RequestDelegate next,
        ILogger<CustomExceptionsHandler> logger,
        IAlertsRepository alertsRepository)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _alertsRepository = alertsRepository
            ?? throw new ArgumentNullException(nameof(alertsRepository));
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = new Response
        {
            Result = false,
            OutInfo = "Server error occured. "
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        errorResponse.OutInfo += $"Exception: {exception.Message}";

        await CreateBagIssue(errorResponse, CancellationToken.None);

        _logger.LogError(errorResponse.OutInfo);

        var result = JsonSerializer.Serialize(errorResponse);

        await context.Response.WriteAsync(result);
    }

    private async Task CreateBagIssue(Response message, CancellationToken token)
    {
        var issueMoment = DateTimeOffset.UtcNow;

        var title = "Server internal error 500";

        var alert = new Alert
        {
            Title = title,
            Description = message.OutInfo!,
            Moment = issueMoment,
            IsAlerted = false
        };

        await _alertsRepository.AddAsync(alert, token);

        _alertsRepository.SaveChanges();
    }

    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionsHandler> _logger;
    private readonly IAlertsRepository _alertsRepository;
}