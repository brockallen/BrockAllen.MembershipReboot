/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Raven.Client;
using Raven.Client.Linq;

namespace BrockAllen.MembershipReboot.RavenDb
{
    public class RavenDbRepository<T> : IRepository<T>, IDisposable
        where T : class
    {
        private readonly IDocumentStore documentStore;
        private readonly IDocumentSession documentSession;
        private readonly IRavenQueryable<T> items;

        public RavenDbRepository(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            documentSession = documentStore.OpenSession();
            items = documentSession.Query<T>();
        }

        void CheckDisposed()
        {
            if (documentStore == null || documentSession == null)
            {
                throw new ObjectDisposedException("RavenDbRepository<T>");
            }
        }

        IQueryable<T> IRepository<T>.GetAll()
        {
            CheckDisposed();
            return items;
        }

        T IRepository<T>.Get(params object[] keys)
        {
            CheckDisposed();
            return items.SingleOrDefault(T => T.In(keys));
        }

        T IRepository<T>.Create()
        {
            CheckDisposed();
            return CreateObject<T>();
        }

        void IRepository<T>.Add(T item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        void IRepository<T>.Remove(T item)
        {
            CheckDisposed();
            documentSession.Delete(item);
            documentSession.SaveChanges();
        }
        
        void IRepository<T>.Update(T item)
        {
            CheckDisposed();
            documentSession.Store(item);
            documentSession.SaveChanges();
        }

        public void Dispose()
        {
            documentSession.TryDispose();
        }

        #region Create Proxies

        private static T CreateObject<T>()
        {
            var obj = CreateConstructor<T>() as Func<T>;
            return obj != null ? obj() : default(T);
        }

        private static ConstructorInfo GetConstructorForType(Type type)
        {
            ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, Type.EmptyTypes, null);
            if (null == constructor)
            {
                var message = string.Format("No constructor available for type '{0}'.", type);
                throw new Exception(message);
            }
            return constructor;
        }

        private static Delegate CreateConstructor<T>()
        {
            ConstructorInfo constructorForType = GetConstructorForType(typeof (T));
            DynamicMethod dynamicMethod = CreateDynamicMethod(constructorForType.DeclaringType.Name, typeof(T), Type.EmptyTypes);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Newobj, constructorForType);
            iLGenerator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<T>));
        }

        private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes)
        {
            return new DynamicMethod(name, returnType, parameterTypes, true);
        }

        #endregion Create Proxies
    }
}
