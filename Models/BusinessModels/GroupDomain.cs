using Models.Enums;

namespace Models.BusinessModels;

public sealed class GroupDomain
{
    public int GroupId { get; }

    public string GroupName { get; }

    public GroupType Type { get; }

    public List<User> Participants { get; }

    public GroupDomain(
        int groupId,
        string groupName,
        GroupType groupType,
        List<User> participants)
    {
        GroupId = groupId;
        GroupName = groupName;
        Type = groupType;
        Participants = participants;
    }
}
