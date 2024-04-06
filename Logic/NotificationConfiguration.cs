namespace Logic;

public sealed class NotificationConfiguration
{
    public required TimeSpan IterationDelay { get; init; }

    public required TimeSpan ReminderOffset { get; init; }

    public required int FutureStockDays { get; init; }
}
