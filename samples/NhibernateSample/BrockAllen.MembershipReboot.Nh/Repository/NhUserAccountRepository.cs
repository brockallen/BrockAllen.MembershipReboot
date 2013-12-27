namespace BrockAllen.MembershipReboot.Nh.Repository
{
    using System;
    using System.Linq;

    using BrockAllen.MembershipReboot;
    using BrockAllen.MembershipReboot.Nh;

    public class NhUserAccountRepository<TAccount> : IUserAccountRepository<TAccount>
        where TAccount : NhUserAccount
    {
        private readonly IRepository<TAccount> accountRepository;

        public NhUserAccountRepository(IRepository<TAccount> accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        protected IQueryable<TAccount> Queryable
        {
            get
            {
                return this.accountRepository.FindAll();
            }
        }

        public TAccount Create()
        {
            var account = Activator.CreateInstance<TAccount>();
            return account;
        }

        public void Add(TAccount item)
        {
            this.accountRepository.Save(item);
        }

        public void Remove(TAccount item)
        {
            this.accountRepository.Delete(item);
        }

        public void Update(TAccount item)
        {
        }

        public TAccount GetByID(Guid id)
        {
            return this.accountRepository.Get(id);
        }

        public TAccount GetByUsername(string username)
        {
            return this.accountRepository.FindBy(x => x.Username == username);
        }

        public TAccount GetByUsername(string tenant, string username)
        {
            return this.accountRepository.FindBy(x => x.Tenant == tenant && x.Username == username);
        }

        public TAccount GetByEmail(string tenant, string email)
        {
            return this.accountRepository.FindBy(x => x.Tenant == tenant && x.Email == email);
        }

        public TAccount GetByMobilePhone(string tenant, string phone)
        {
            return this.accountRepository.FindBy(x => x.Tenant == tenant && x.MobilePhoneNumber == phone);
        }

        public TAccount GetByVerificationKey(string key)
        {
            return this.accountRepository.FindBy(x => x.VerificationKey == key);
        }

        public TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var accounts = from a in this.accountRepository.FindAll()
                          where a.Tenant == tenant
                          from la in a.LinkedAccountsCollection
                          where la.ProviderName == provider && la.ProviderAccountID == id
                          select a;

            return accounts.SingleOrDefault();
        }

        public TAccount GetByCertificate(string tenant, string thumbprint)
        {
            var accounts =
                from a in this.accountRepository.FindAll()
                where a.Tenant == tenant
                from c in a.CertificatesCollection
                where c.Thumbprint == thumbprint
                select a;
            return accounts.SingleOrDefault();
        }
    }

    public class NhUserAccountRepository : NhUserAccountRepository<NhUserAccount>
    {
        public NhUserAccountRepository(IRepository<NhUserAccount> accountRepository)
            : base(accountRepository)
        {
        }
    }
}