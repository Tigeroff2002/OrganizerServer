using Models.Enums;

namespace Models;

public class Group
{
    public int Id { get; }

    public string GroupName { get; }

    public GroupType Type { get; }

    public virtual List<User> Participants { get; set; }

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
