namespace Models.BusinessModels;

public sealed class UserCalendarSnapshot
{
    public int UserId { get; }

    public DateTimeOffset BeginTime { get; }

    public DateTimeOffset EndTime { get; }

    public UserCalendarSnapshot(
        int userId, DateTimeOffset beginTime, DateTimeOffset endTime)
    {
        UserId = userId;
        BeginTime = beginTime;
        EndTime = endTime;
    }
}
