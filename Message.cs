using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace WalrusMessenger
{
    public class Message
    {
        public long Id { get; }

        public long ChatId { get; set; }

        [JsonIgnore]
        public Chat Chat { get; set; } = null!;

        public DateTime DateTime { get; }

        public string Text { get; internal set; } = string.Empty;

        public long SenderId { get; }

        public long? ResentFrom { get; }

        public long? AnswerTo { get; }

        public List<long> DeletedFor { get; } = new();

        public List<long> ReadBy { get; } = new();

        public IncludedFile[] IncludedFiles { get; } = Array.Empty<IncludedFile>();

        public Message(DateTime dateTime, string text, long senderId, long chatId)
        {
            DateTime = dateTime;
            Text = text;
            SenderId = senderId;
            ChatId = chatId;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
