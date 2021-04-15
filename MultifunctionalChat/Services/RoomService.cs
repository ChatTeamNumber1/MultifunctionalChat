using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MultifunctionalChat.Models;

namespace MultifunctionalChat.Services
{
    public class RoomService : IRepository<Room>
    {
        private readonly ApplicationContext _context;
        private readonly List<Room> _roomsList;
        public RoomService(ApplicationContext context)
        {
            _context = context;
            _roomsList = _context.Rooms.ToList();

            foreach (var room in _roomsList)
            {
                if (room.Users == null)
                {
                    room.Users = context.Users.Where(rm => rm.Rooms.Contains(room)).ToList();
                }
                if (room.RoomUsers == null)
                {
                    room.RoomUsers = context.RoomUsers.Where(rm => rm.Room == room).ToList();
                }
            }
        }

        public List<Room> GetList()
        {
            return _roomsList;
        }       
        
        public Room Get(int id)
        {
            return _roomsList.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Create(Room newRoom)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                _context.Rooms.Add(newRoom);
                _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public void Update(Room updatedRoom)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                ///FIXME - аналогично с UserService
                using var newContext = new ApplicationContext();
                newContext.Entry(updatedRoom).State = EntityState.Modified;
                newContext.SaveChanges();

                transaction.Commit();
                //int roomIndex = _roomsList.IndexOf(Get(updatedRoom.Id));
                //_roomsList[roomIndex] = updatedRoom;
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        public void Delete(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                Room roomToDelete = _roomsList.Find(x => x.Id == id);
                _context.Rooms.Remove(roomToDelete);
                _context.SaveChanges();
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
                    _context.Dispose();
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
