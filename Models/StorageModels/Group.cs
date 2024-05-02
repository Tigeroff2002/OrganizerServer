using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.StorageModels;

public class Group
{
    [Key]
    public int Id { get; set; }

    public string GroupName { get; set; }

    public GroupType Type { get; set; }

    public virtual List<GroupingUsersMap> ParticipantsMap { get; set; }

    public virtual List<Event> RelatedEvents { get; set; }

    public int ManagerId { get; set; }

    public virtual User Manager { get; set; }

    public Group() { }
}
