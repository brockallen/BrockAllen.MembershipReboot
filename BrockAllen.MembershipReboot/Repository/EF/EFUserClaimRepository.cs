using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.EF
{
    public class EFUserClaimRepository : IUserClaimRepository
    {
        EFMembershipRebootDatabase db = new EFMembershipRebootDatabase();

        public void Add(string username, UserClaim[] claims)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (claims == null || !claims.Any()) return;

            var currentClaimsForUser =
                from item in db.UserClaims
                where item.Username == username
                select item;

            var claimsToAdd = 
                claims.Except(currentClaimsForUser, UserClaim.Comparer);

            foreach (var c in claimsToAdd)
            {
                db.UserClaims.Add(c);
            }

            db.SaveChanges();
        }

        public void Remove(string username, UserClaim[] claims)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            if (claims == null || !claims.Any()) return;

            var currentClaimsForUser =
                from item in db.UserClaims
                where item.Username == username
                select item;

            var claimsToRemove =
                claims.Intersect(currentClaimsForUser, UserClaim.Comparer);

            foreach (var c in claimsToRemove)
            {
                db.UserClaims.Remove(c);
            }

            db.SaveChanges();
        }

        public void Clear(string username)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");

            var currentClaimsForUser =
                from item in db.UserClaims
                where item.Username == username
                select item;

            foreach (var c in currentClaimsForUser)
            {
                db.UserClaims.Remove(c);
            }

            db.SaveChanges();
        }

        public IEnumerable<UserClaim> Get(string username)
        {
            if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("username");
            return db.UserClaims.Where(x => x.Username == username).ToArray();
        }
    }
}
