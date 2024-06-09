namespace Logic.Abstractions;

public interface IUserDevicesTokensActualizer
{
    public Task ActualizeUserDevicesAsync(CancellationToken token);
}
