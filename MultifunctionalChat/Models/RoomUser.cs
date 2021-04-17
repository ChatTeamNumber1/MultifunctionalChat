using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultifunctionalChat.Models
{
    public class RoomUser
    {
        public int Id { get; set; }
        public int RoomsId { get; set; }
        public Room Room { get; set; }
        public int UsersId { get; set; }
        public User User { get; set; }

        public char? Status { get; set; }
        public DateTime? BanStart { get; set; }
        public int? BanInterval { get; set; }
    }
}
