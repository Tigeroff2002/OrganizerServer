using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class Report
{
    [Key]
    public int Id { get; set; }

    public string Description { get; set; }

    public ReportType ReportType { get; set; }

    public DateTimeOffset BeginMoment { get; set; }

    public DateTimeOffset EndMoment { get; set; }

    public virtual User User { get; set; }

    public int UserId { get; set; }

    public Report() { }
}
