using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public interface IRepository<T> : IDisposable
         where T : class
    {
        IQueryable<T> GetAll();
        T Get(params object[] keys);
        void Add(T item);
        void Remove(params object[] keys);
        void Remove(T item);
        void SaveChanges();
    }
}
