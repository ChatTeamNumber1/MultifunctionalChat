using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MultifunctionalChat.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан Login")]
        public string Login { get; set; }

        //Вход без пароля
        //[Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
