/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface IFactory
    {
        T Create<T>(Type type);
    }

    public class DefaultFactory : IFactory
    {
        public T Create<T>(Type type)
        {
            return (T)Activator.CreateInstance(type);
        }
    }

    public class DelegateFactory : IFactory
    {
        Func<Type, object> factoryFunc;
        public DelegateFactory(Func<Type, object> factoryFunc)
        {
            this.factoryFunc = factoryFunc;
        }
        public T Create<T>(Type type)
        {
            return (T)factoryFunc(type);
        }
    }
}
