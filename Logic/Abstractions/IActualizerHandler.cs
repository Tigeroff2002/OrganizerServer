namespace Logic.Abstractions;

public interface IActualizerHandler
{
    public Task ActualizeAsync(CancellationToken token);
}
