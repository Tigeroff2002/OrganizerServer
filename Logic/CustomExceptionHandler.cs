using Logic.Abstractions;
using Logic.ControllerHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.BusinessModels;
using Models.Enums;
using Models.RedisEventModels.AlertEvents;
using Models.StorageModels;
using PostgreSQL.Abstractions;
using System.Net;
using System.Text.Json;

namespace Logic;

public sealed class CustomExceptionsHandler
{
    public CustomExceptionsHandler(
        RequestDelegate next,
        IExceptionHandler exceptionHandler)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _exceptionHandler = exceptionHandler 
            ?? throw new ArgumentNullException(nameof(exceptionHandler));
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

            throw;
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var resultJson = 
            await _exceptionHandler.HandleExceptionsAsync(
                exception, 
                CancellationToken.None);

        await context.Response.WriteAsync(resultJson);
    }

    private readonly IExceptionHandler _exceptionHandler;
    private readonly RequestDelegate _next;
}