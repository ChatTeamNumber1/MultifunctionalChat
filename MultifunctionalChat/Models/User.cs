﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MultifunctionalChat.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        [ForeignKey("Role")]
        public int RoleId { get; set; }

        [NotMapped]
        public Role UserRole { get; set; }
        //ToDo Avatar (или аватар роли)
    }
}
