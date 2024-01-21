using Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models
{
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

        public virtual List<GroupingUsersMap> GroupingMaps { get; set; }

        public virtual List<UserTask> ReportedTasks { get; set; }

        public virtual List<UserTask> TasksForImplementation { get; set; }

        public virtual List<EventsUsersMap> EventMaps { get; set; }

        public virtual List<Event> ManagedEvents { get; set; }

        public virtual List<Snapshot> Snapshots { get; set; }

        public virtual List<Issue> Issues { get; set; }

        public User() { }
    }
}