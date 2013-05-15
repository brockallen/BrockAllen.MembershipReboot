using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public interface IRepository<T> : IDisposable
         where T : class
    {
        IQueryable<T> GetAll();
        T Get(params object[] keys);
        T Create();
        void Add(T item);
        void Remove(T item);
        void Update(T item);
    }
}
