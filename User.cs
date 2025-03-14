using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WalrusMessenger
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; internal set; } = null!;
        public string Login { get; internal set; } = null!;
        public string Email { get; internal set; } = null!;

        public List<long> Chats { get; set; } = new();

        public List<long> BanList { get; set; } = new();

        public byte[]? ProfilePicture { get; internal set; } = null;
        public string? Description { get; internal set; } = null;

        [JsonIgnore]
        public string Password { get; internal set; } = null!;
        public DateTime LastLogin { get; internal set; } = DateTime.Now;
        public bool IsOnline { get; internal set; }

        public User(string name, string login, string email, List<long> chats, List<long> banList, byte[]? profilePicture, 
            string? description, string password, DateTime lastLogin, bool isOnline) 
        {
            Name = name;
            Login = login;
            Email = email;
            Password = password;
            Chats = chats;
            BanList = banList;
            ProfilePicture = profilePicture;
            Description = description;
            LastLogin = lastLogin;
            IsOnline = isOnline;
        }

//        public User() { }
        /*
        public User(string name, string login, string password, string email)
        {
            Name = name;
            Login = login;
            Email = email;
            Password = password;
            Console.WriteLine("\n\n\nAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n\n\n");
        } */
    }
}
