namespace Logic.Abstractions;

public interface IExceptionHandler
{
    public Task<string> HandleExceptionsAsync(
        Exception exception,
        CancellationToken token);
}
