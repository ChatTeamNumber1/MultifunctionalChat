using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MultifunctionalChat.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string Text { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }

        [NotMapped]
        public string UserName { get; set; }
        
        //TODO: MessageDate        
    }
}