namespace MultifunctionalChat.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        //ToDo Role, Avatar (или аватар роли)
    }
}
