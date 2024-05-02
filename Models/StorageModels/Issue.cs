using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.StorageModels;

public class Issue
{
    [Key]
    public int Id { get; set; }

    public IssueType IssueType { get; set; }

    public IssueStatus Status { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string ImgLink { get; set; }

    public DateTimeOffset IssueMoment { get; set; }

    public virtual User User { get; set; }

    public int UserId { get; set; }

    public Issue() { }
}
