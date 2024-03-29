﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MultifunctionalChat.Models;

namespace MultifunctionalChat.Services
{
    public class UserService : IRepository<User>
    {
        private readonly List<User> usersList;
        private readonly ApplicationContext context;

        public UserService(ApplicationContext context)
        {
            this.context = context;
            usersList = context.Users.ToList();

            foreach (var user in usersList)
            {
                if (user.UserRole == null)
                {
                    Role role = context.Roles.Where(role => role.Id == user.RoleId).FirstOrDefault();
                    user.UserRole = role;
                }
                if (user.Rooms == null)
                {
                    user.Rooms = context.Rooms.Where(rm => rm.Users.Contains(user)).ToList();
                }
            }
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
            using var transaction = context.Database.BeginTransaction();

            try
            {
                context.Users.Add(newUser);
                context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public void Update(User updatedUser)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                ///FIXME Так удалось обойти проблему открытого соединения в прошлом проекте
                using var newContext = new ApplicationContext();
                newContext.Entry(updatedUser).State = EntityState.Modified;
                newContext.SaveChanges();

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public void Delete(int id)
        {
            var userToDelete = usersList.Where(x => x.Id == id).FirstOrDefault();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                context.Users.Remove(userToDelete);
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
