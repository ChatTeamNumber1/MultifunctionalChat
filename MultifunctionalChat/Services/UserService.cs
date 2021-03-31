using System;
using System.Collections.Generic;
using System.Linq;
using MultifunctionalChat.Models;

namespace MultifunctionalChat.Services
{
    //ToDo : IRepository<User>
    public class UserService
    {
        private readonly List<User> usersList;

        public UserService()
        {
            usersList = new List<User> { 
                new User { Id = 1, Login = "1", Name = "Главный" },
                new User { Id = 2, Login = "Сергей", Name = "Сергей" },
                new User { Id = 3, Login = "Наталья", Name = "Наталья" }
            };
        }
        public List<User> GetList()
        {
            return usersList;
        }
        public User Get(int id)
        {
            return usersList.Where(x => x.Id == id).FirstOrDefault();
        }
        public void Create(User newUser)
        {
            try
            {
                usersList.Add(newUser);
            }
            catch (Exception)
            {
            }
        }

        public void Update(User updatedUser)
        {
            try
            {
                int userIndex = usersList.IndexOf(Get(updatedUser.Id));
                usersList[userIndex] = updatedUser;
            }
            catch (Exception)
            {
            }
        }

        public void Delete(int id)
        {
            var userType = usersList.Where(x => x.Id == id).FirstOrDefault();

            try
            {
                usersList.Remove(userType);
            }
            catch (Exception)
            {
            }
        }
    }
}