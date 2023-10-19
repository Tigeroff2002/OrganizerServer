using System.ComponentModel.DataAnnotations;

namespace Models;

public class Report
{
    [Key]
    public int Id { get; set; }

    public string Caption { get; }

    public string Description { get; }

    public DateTimeOffset BeginMoment { get;}

    public DateTimeOffset EndMoment { get; }

    public virtual User User { get; set; }

    public virtual List<Event> Events { get; set; }

    public Report() { }

    public Report(
        int id,
        string caption,
        string description, 
        DateTimeOffset beginMoment,
        DateTimeOffset endMoment)
    {

    }
}
