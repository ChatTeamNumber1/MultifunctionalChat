using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultifunctionalChat.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string Text { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }

        [NotMapped]
        public string UserName { get; set; }

        public DateTime MessageDate { get; set; }
    }
}