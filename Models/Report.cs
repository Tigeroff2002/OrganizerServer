namespace Models;

public class Report
{
    public int Id { get; set; }

    public string Caption { get; }

    public string Description { get; }

    public DateTimeOffset BeginMoment { get;}

    public DateTimeOffset EndMoment { get; }

    public virtual User User { get; set; }

    public int UserId { get; }

    public virtual List<Event> Events { get; set; }

    public Report(
        int id,
        string caption,
        string description, 
        DateTimeOffset beginMoment,
        DateTimeOffset endMoment)
    {

    }
}
