namespace Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string AuthToken { get; set; }

        public virtual List<Group> Groups { get; set; }

        public virtual List<UserTask> TasksForImplementation { get; set; }

        public virtual List<Event> Events { get; set; }

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