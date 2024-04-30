namespace Logic.Abstractions;

public interface IRedisProcessor
{
    public Task ProcessAsync(CancellationToken token);
}
