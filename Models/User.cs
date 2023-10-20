using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string AuthToken { get; set; }

        public virtual List<UserGroupMap> UserGroupMaps { get; set; }

        public virtual List<UserTask> TasksForImplementation { get; set; }

        public virtual List<UserEventMap> UserEventMaps { get; set; }

        public virtual List<Event> ManagedEvents { get; set; }

        public virtual List<Report> Reports { get; set; }

        public User() { }

        public User(
            int id,
            string userName,
            string email,
            string password,
            string phoneNumber)
        {
            Id = id;
            UserName = userName;
            Email = email;
            Password = password;
            PhoneNumber = phoneNumber;
        }
    }
}