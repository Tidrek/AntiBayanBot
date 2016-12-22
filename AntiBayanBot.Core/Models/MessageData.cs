using System;
namespace AntiBayanBot.Core.Models
{
    public class MessageData
    {
        public int MessageId { get; set; }
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public DateTime DateTimeAdded { get; set; }
        public string UserFullName { get; set; }
        public string UserName { get; set; }
    }
}