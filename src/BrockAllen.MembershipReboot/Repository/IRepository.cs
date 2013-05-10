using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public interface IRepository<T, Key> : IDisposable
         where T : class
    {
        IQueryable<T> GetAll();
        T Get(Key key);
        void Add(T item);
        void Remove(T item);
        void SaveChanges();
        void SaveChanges(T item);
    }
}
