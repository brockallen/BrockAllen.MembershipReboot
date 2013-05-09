using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class LinkedAccountService : IDisposable
    {
        ILinkedAccountRepository linkedAccountRepository;

        public LinkedAccountService(ILinkedAccountRepository linkedAccountRepository)
        {
            this.linkedAccountRepository = linkedAccountRepository;
        }
        
        public void Dispose()
        {
            if (this.linkedAccountRepository.TryDispose())
            {
                this.linkedAccountRepository = null;
            }
        }

        public virtual void Add(
            string providerName, string providerAccountID, 
            Guid localAccountID, 
            IEnumerable<Claim> externalAccountClaims = null)
        {
            var existing = Get(providerName, providerAccountID);
            if (existing != null)
            {
                if (existing.LocalAccountID == localAccountID)
                {
                    this.Update(existing, externalAccountClaims);
                    return;
                }
                else
                {
                    throw new ValidationException("This external account is already linked to another local account.");
                }
            }

            var claims = GetLinkedClaimsFromClaims(externalAccountClaims);
            
            var item = new LinkedAccount
            {
                ProviderName = providerName,
                ProviderAccountID = providerAccountID, 
                LocalAccountID = localAccountID, 
                LastLogin = UtcNow,
                Claims = claims.ToArray()
            };
            this.linkedAccountRepository.Add(item);
            this.linkedAccountRepository.SaveChanges();
        }

        public virtual void Update(LinkedAccount account, IEnumerable<Claim> externalClaims)
        {
            if (account == null) throw new ArgumentNullException("account");

            var claims = GetLinkedClaimsFromClaims(externalClaims);
            account.Claims.Clear();
            foreach (var c in claims)
            {
                account.Claims.Add(c);
            }
            account.LastLogin = UtcNow;
            this.linkedAccountRepository.SaveChanges();
        }

        public virtual void Remove(string providerName, string providerAccountID)
        {
            this.linkedAccountRepository.Remove(providerName, providerAccountID);
            this.linkedAccountRepository.SaveChanges();
        }

        public virtual void RemoveAll(Guid localAccountID)
        {
            foreach (var account in Get(localAccountID))
            {
                this.linkedAccountRepository.Remove(account);
            }
            this.linkedAccountRepository.SaveChanges();
        }

        public virtual LinkedAccount Get(string externalAccountProvider, string externalAccountID)
        {
            return this.linkedAccountRepository.Get(externalAccountProvider, externalAccountID);
        }

        public virtual IQueryable<LinkedAccount> Get(Guid localAccountID)
        {
            var query =
                from item in this.linkedAccountRepository.GetAll()
                where item.LocalAccountID == localAccountID
                select item;
            return query;
        }

        private static IEnumerable<LinkedAccountClaim> GetLinkedClaimsFromClaims(IEnumerable<Claim> externalAccountClaims)
        {
            externalAccountClaims = externalAccountClaims ?? Enumerable.Empty<Claim>();

            var claims =
                from c in externalAccountClaims
                select new LinkedAccountClaim
                {
                    Type = c.Type,
                    Value = c.Value
                };

            return claims;
        }

        protected internal virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
