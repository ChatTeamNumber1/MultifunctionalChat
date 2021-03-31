using System;
using System.Collections.Generic;
using System.Linq;
using MultifunctionalChat.Models;

namespace MultifunctionalChat.Services
{
    public class MessageService : IRepository<Message>
    {
        private readonly List<Message> messagesList;

        public MessageService()
        {
            messagesList = new List<Message> {
                new Message { Id = 1, Text = "1", UserName = "Главный" },
                new Message { Id = 2, Text = "Всем привет", UserName = "Сергей" },
                new Message { Id = 3, Text = "Скучно", UserName = "Наталья" }
            };
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
            var messageType = messagesList.Where(x => x.Id == id).FirstOrDefault();

            try
            {
                messagesList.Remove(messageType);
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