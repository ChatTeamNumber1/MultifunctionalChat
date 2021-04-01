using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MultifunctionalChat.Models;

namespace MultifunctionalChat.Services
{
    public class MessageService : IRepository<Message>
    {
        private readonly List<Message> messagesList;

        public MessageService()
        {
            messagesList = new List<Message> {
                new Message { Id = 1, UserId = 1, Text = "Решаем задачу" },
                new Message { Id = 2, UserId = 2, Text = "Принято" },
                new Message { Id = 3, UserId = 3, Text = "Хорошо" }
            };

            //Список пользователей пока получаем так (без ApplicationContext)
            UserService us = new UserService();
            foreach (var message in messagesList)
            {
                User user = us.Get(message.UserId);
                message.UserName = user.Name;
                message.MessageDate = DateTime.Now;
            }
        }

        public List<Message> GetList()
        {
            return messagesList;
        }
        public Message Get(int id)
        {
            return messagesList.Where(x => x.Id == id).FirstOrDefault();
        }
        public void Create(Message newMessage)
        {
            try
            {
                messagesList.Add(newMessage);
            }
            catch (Exception)
            {
            }
        }
        public void Update(Message updatedMessage)
        {
            try
            {
                int messageIndex = messagesList.IndexOf(Get(updatedMessage.Id));
                messagesList[messageIndex] = updatedMessage;
            }
            catch (Exception)
            {
            }
        }
        public void Delete(int id)
        {
            Message messageToDelete = messagesList.Find(x => x.Id == id);

            try
            {
                messagesList.Remove(messageToDelete);
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
