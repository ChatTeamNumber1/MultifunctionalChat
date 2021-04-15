using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MultifunctionalChat.Models;

namespace MultifunctionalChat.Services
{
    public class RoomUserService : IRepository<RoomUser>
    {
        private readonly List<RoomUser> RoomUsersList;
        private readonly ApplicationContext context;

        public RoomUserService(ApplicationContext context)
        {
            this.context = context;
            RoomUsersList = context.RoomUsers.ToList();
        }
        public List<RoomUser> GetList()
        {
            return RoomUsersList;
        }
        public RoomUser Get(int id)
        {
            return null;
        }
        public void Create(RoomUser newRoomUser)
        {
            using var transaction = context.Database.BeginTransaction();

            try
            {
                context.RoomUsers.Add(newRoomUser);
                context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public void Update(RoomUser updatedRoomUser)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                ///FIXME Так удалось обойти проблему открытого соединения в прошлом проекте
                using var newContext = new ApplicationContext();
                newContext.Entry(updatedRoomUser).State = EntityState.Modified;
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
            var RoomUserToDelete = RoomUsersList.Where(x => x.Id == id).FirstOrDefault();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                context.RoomUsers.Remove(RoomUserToDelete);
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
