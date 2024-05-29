namespace Logic.Abstractions;

public interface ICodeConfirmationsActualizer
{
    public Task ActualizeEmailConfirmationsAsync(CancellationToken token);
}
