namespace Logic;

public sealed class RedisReadingConfiguration
{
    public required string HostAndPort { get; init; }

    public required int DelaySeconds { get; init; }

    public required bool IsDeleteCommited { get; init; } = true;
}
