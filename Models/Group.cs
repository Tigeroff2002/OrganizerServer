using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class Group
{
    [Key]
    public int Id { get; set; }

    public string GroupName { get; set; }

    public GroupType Type { get; set; }

    public virtual List<User> Participants { get; set; }

    public Group() { }

    public Group(
        int groupId,
        string groupName,
        GroupType groupType,
        List<User> participants)
    {
        Id = groupId;
        GroupName = groupName;
        Type = groupType;
        Participants = participants;
    }
}
