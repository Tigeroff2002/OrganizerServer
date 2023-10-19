using System.ComponentModel.DataAnnotations;

namespace Models;

public class Report
{
    [Key]
    public int Id { get; set; }

    public string Caption { get; set; }

    public string Description { get; set; }

    public DateTimeOffset BeginMoment { get; set; }

    public DateTimeOffset EndMoment { get; set; }

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
