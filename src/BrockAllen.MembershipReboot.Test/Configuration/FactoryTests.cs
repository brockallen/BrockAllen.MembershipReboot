using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test.Configuration
{
    [TestClass]
    public class ReflectionFactoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_NullParam_Throws()
        {
            new ReflectionFactory(null);
        }

        [TestMethod]
        public void CreateUserAccountRepository_CreatesUserAccountRepository()
        {
            var sub = new ReflectionFactory(typeof(TestRepo));
            var instance = sub.CreateUserAccountRepository();
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestRepo));
        }
    }

    [TestClass]
    public class DelegateFactoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_NullParam_Throws()
        {   
            new DelegateFactory(null);
        }

        [TestMethod]
        public void CreateUserAccountRepository_CallsDelegate()
        {
            Func<IUserAccountRepository> func = () => new TestRepo();
            var sub = new DelegateFactory(func);
            var instance = sub.CreateUserAccountRepository();
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestRepo));
        }
    }

    public class TestRepo : IUserAccountRepository
    {
        public IQueryable<UserAccount> GetAll()
        {
            throw new NotImplementedException();
        }

        public UserAccount Get(params object[] keys)
        {
            throw new NotImplementedException();
        }

        public UserAccount Create()
        {
            throw new NotImplementedException();
        }

        public void Add(UserAccount item)
        {
            throw new NotImplementedException();
        }

        public void Remove(UserAccount item)
        {
            throw new NotImplementedException();
        }

        public void Update(UserAccount item)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
