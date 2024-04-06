namespace ToDoCalendarServer.Controllers;

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionsHandling(
        this IApplicationBuilder builder)
        => builder.UseMiddleware<CustomExceptionsHandler>();
}
