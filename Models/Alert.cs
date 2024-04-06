using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models;

public sealed class Alert
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public DateTimeOffset Moment { get; set; }

    public bool IsAlerted { get; set; }

    public Alert() { }
}
