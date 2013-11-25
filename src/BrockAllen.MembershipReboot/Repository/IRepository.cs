/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public interface IRepository<T> : IDisposable
         where T : class
    {
        IQueryable<T> GetAll();
        T Get(Guid key);
        T Create();
        void Add(T item);
        void Remove(T item);
        void Update(T item);
    }
}
