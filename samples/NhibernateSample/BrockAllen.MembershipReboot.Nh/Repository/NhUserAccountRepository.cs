namespace BrockAllen.MembershipReboot.Nh.Repository
{
    using System;
    using System.Linq;

    using BrockAllen.MembershipReboot;
    using BrockAllen.MembershipReboot.Nh;

    public class NhUserAccountRepository<TAccount> : QueryableUserAccountRepository<TAccount>
        where TAccount : NhUserAccount
    {
        private readonly IRepository<TAccount> accountRepository;

        public NhUserAccountRepository(IRepository<TAccount> accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        protected override IQueryable<TAccount> Queryable
        {
            get
            {
                return this.accountRepository.FindAll();
            }
        }

        public override TAccount Create()
        {
            var account = Activator.CreateInstance<TAccount>();
            return account;
        }

        public override void Add(TAccount item)
        {
            this.accountRepository.Save(item);
        }

        public override void Remove(TAccount item)
        {
            this.accountRepository.Delete(item);
        }

        public override void Update(TAccount item)
        {
            this.accountRepository.Update(item);
        }

        public override TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var accounts = from a in this.accountRepository.FindAll()
                          where a.Tenant == tenant
                          from la in a.LinkedAccountsCollection
                          where la.ProviderName == provider && la.ProviderAccountID == id
                          select a;

            return accounts.SingleOrDefault();
        }

        public override TAccount GetByCertificate(string tenant, string thumbprint)
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