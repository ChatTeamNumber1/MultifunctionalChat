using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;
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
                Role role = context.Roles.Where(role => role.Id == user.RoleId).FirstOrDefault();
                user.UserRole = role;
                message.Author = user;
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
            using var transaction = context.Database.BeginTransaction();

            try
            {
                //TODO Айдишник может и должен прилетать с клиентской стороны (и вообще это нарушение Single Responsibility)
                //User user = context.Users.Where(user => user.Name == newMessage.Author.Name).FirstOrDefault();
                //newMessage.UserId = user.Id;
                newMessage.MessageDate = DateTime.Now;

                context.Messages.Add(newMessage);
                context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public void Update(Message updatedMessage)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                ///FIXME Так удалось обойти проблему открытого соединения в прошлом проекте
                using var newContext = new ApplicationContext();
                newContext.Entry(updatedMessage).State = EntityState.Modified;
                newContext.SaveChanges();

                transaction.Commit();
                int messageIndex = messagesList.IndexOf(Get(updatedMessage.Id));
                messagesList[messageIndex] = updatedMessage;
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public void Delete(int id)
        {
            var messageToDelete = messagesList.Where(x => x.Id == id).FirstOrDefault();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                context.Messages.Remove(messageToDelete);
                context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
