namespace BrockAllen.MembershipReboot.Repository
{
    using System;
    using System.Linq;

    public class SimpleUserAccountRepository : ISimpleUserAccountRepository<UserAccount>
    {
        private readonly IUserAccountRepository userRepository;

        public SimpleUserAccountRepository(IUserAccountRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public void Dispose()
        {
            //TODO: look into dispose implementation
            //userRepository.Dispose();
        }

        public UserAccount Get(Guid key)
        {
            return userRepository.Get(key);
        }

        public UserAccount Create()
        {
            return userRepository.Create();
        }

        public void Add(UserAccount item)
        {
            userRepository.Add(item);
        }

        public void Remove(UserAccount item)
        {
            userRepository.Remove(item);
        }

        public void Update(UserAccount item)
        {
            userRepository.Update(item);
        }

        public UserAccount GetByUsername(string tenant, string username, bool usernamesUniqueAcrossTenants)
        {
            var query = userRepository.GetAll().Where(x => x.Username == username);
            if (!usernamesUniqueAcrossTenants)
            {
                query = query.Where(x => x.Tenant == tenant);
            }

            var account = query.SingleOrDefault();
            return account;
        }

        public UserAccount GetByEmail(string tenant, string email)
        {
            var account = userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).SingleOrDefault();
            return account;
        }

        public UserAccount GetByVerificationKey(string key)
        {
            var account = userRepository.GetAll().Where(x => x.VerificationKey == key).SingleOrDefault();
            return account;
        }

        public UserAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            var query =
                from u in userRepository.GetAll()
                where u.Tenant == tenant
                from l in u.LinkedAccounts
                where l.ProviderName == provider && l.ProviderAccountID == id
                select u;

            var account = query.SingleOrDefault();
            return account;
        }

        public UserAccount GetByCertificate(string tenant, string thumbprint)
        {
            var query =
                from u in userRepository.GetAll()
                where u.Tenant == tenant
                from c in u.Certificates
                where c.Thumbprint == thumbprint
                select u;

            var account = query.SingleOrDefault();
            return account;
        }

        public bool UsernameExistsAcrossTenants(string username)
        {
            return this.userRepository.GetAll().Where(x => x.Username == username).Any();
        }

        public bool UsernameExists(string tenant, string username)
        {
            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.Username == username).Any();
        }

        public bool EmailExists(string tenant, string email)
        {
            return this.userRepository.GetAll().Where(x => x.Tenant == tenant && x.Email == email).Any();
        }

        public bool EmailExistsOtherThan(UserAccount account, string email)
        {
            return this.userRepository.GetAll().Where(x => x.Tenant == account.Tenant && x.Email == email && x.ID != account.ID).Any();
        }

        public bool IsMobilePhoneNumberUnique(string tenant, Guid accountId, string mobile)
        {
            var query =
                from a in userRepository.GetAll().Where(x => x.Tenant == tenant && x.IsAccountClosed == false)
                where a.MobilePhoneNumber == mobile && a.ID != accountId
                select a;
            return !query.Any();
        }
    }
}