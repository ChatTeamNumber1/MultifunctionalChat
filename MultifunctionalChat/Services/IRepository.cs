using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultifunctionalChat.Services
{
    public interface IRepository<T> : IDisposable
    {
        List<T> GetList();
        T Get(int id);
        void Create(T item);
        void Update(T item);
        void Delete(int id);
    }
}
