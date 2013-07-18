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
        IUserAccountRepository CreateUserAccountRepository();
    }

    public class ReflectionFactory : IFactory
    {
        Type type;
        public ReflectionFactory(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            this.type = type;
        }

        public IUserAccountRepository CreateUserAccountRepository()
        {
            return (IUserAccountRepository)Activator.CreateInstance(type);
        }
    }

    public class DelegateFactory : IFactory
    {
        Func<IUserAccountRepository> factoryFunc;
        public DelegateFactory(Func<IUserAccountRepository> factoryFunc)
        {
            if (factoryFunc == null) throw new ArgumentNullException("factoryFunc");
            this.factoryFunc = factoryFunc;
        }

        public IUserAccountRepository CreateUserAccountRepository()
        {
            return this.factoryFunc();
        }
    }
}
