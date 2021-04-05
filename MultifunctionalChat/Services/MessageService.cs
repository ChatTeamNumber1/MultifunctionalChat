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
        private readonly ApplicationContext context;

        public MessageService(ApplicationContext context)
        {
            this.context = context;
            messagesList = context.Messages.ToList();

            foreach (var message in messagesList)
            {
                User user = context.Users.Where(user => user.Id == message.UserId).FirstOrDefault();
                message.UserName = user.Name;
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
            //throw new NotImplementedException();
        }
    }
}
