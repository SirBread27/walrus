using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Text.Json.Serialization;

namespace WalrusMessenger
{
    public class Chat
    {
        public long Id { get; }
        public List<long> Users { get; } = new();
        public virtual string Name { get; internal set; } = null!;
        public virtual string? Description { get; internal set; }
        public virtual byte[]? ProfilePicture { get; internal set; }
        public List<IncludedFile> Files { get; } = new();
        public string LastMessage { get; internal set; } = "";
        [JsonIgnore]
        public ICollection<Message> Messages { get; } = new List<Message>();
        public virtual List<long> Admins { get; } = new();

        public Chat(List<long> users, string name, string? description, byte[]? profilePicture)
        {
            Users = users;
            Name = name;
            Description = description;
            ProfilePicture = profilePicture;
        }

        //internal void Delete()

        internal void AddMessage(Message m)
        {
            Messages.Add(m);
            LastMessage = m.ToString();
        }

        internal IEnumerable<Message> GetMessages(AppContext db) 
        {
            return db.Messages.Where(x => x.ChatId == Id).OrderByDescending(x => x.DateTime);
        }
    }
}
