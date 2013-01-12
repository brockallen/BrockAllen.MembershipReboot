using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
