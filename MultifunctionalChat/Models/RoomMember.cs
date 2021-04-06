using System.ComponentModel.DataAnnotations.Schema;

namespace MultifunctionalChat.Models
{
    public class RoomMember
    {
        public int Id { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }
        [NotMapped]
        public string RoomName { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        [NotMapped]
        public string UserName { get; set; }
    }
}
