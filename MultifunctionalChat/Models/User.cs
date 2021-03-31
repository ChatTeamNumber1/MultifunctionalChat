using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultifunctionalChat.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }

        //ToDo Role, Password, Avatar (или аватар роли)
    }
}
