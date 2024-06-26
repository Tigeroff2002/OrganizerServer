﻿using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.StorageModels;

public class User
{
    [Key]
    public int Id { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    public string Password { get; set; }

    public string PhoneNumber { get; set; }

    public string AuthToken { get; set; }

    public DateTimeOffset AccountCreation { get; set; }

    public virtual List<UserDeviceMap> Devices { get; set; }

    public virtual List<GroupingUsersMap> GroupingMaps { get; set; }

    public virtual List<UserTask> ReportedTasks { get; set; }

    public virtual List<UserTask> TasksForImplementation { get; set; }

    public virtual List<EventsUsersMap> EventMaps { get; set; }

    public virtual List<Event> ManagedEvents { get; set; }

    public virtual List<Group> ManagedGroups { get; set; }

    public virtual List<Snapshot> Snapshots { get; set; }

    public virtual List<Issue> Issues { get; set; }

    public virtual List<DirectChat> DirectChatsForUserHome { get; set; }

    public virtual List<DirectChat> DirectChatsForUserAway { get; set; }

    public virtual List<DirectChat> DirectChats { get; set; }

    public virtual List<DirectMessage> Messages { get; set; }

    public User()
    {
        if (DirectChatsForUserHome != null
            && DirectChatsForUserAway != null)
        {
            DirectChats = DirectChatsForUserAway
                .Concat(DirectChatsForUserAway)
                .ToList();
        }
    }
}
