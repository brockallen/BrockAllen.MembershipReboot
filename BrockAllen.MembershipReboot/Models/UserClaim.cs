using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class UserClaim
    {
        [Key]
        [Column(Order=1)]
        public string Username { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Type { get; set; }
        [Key]
        [Column(Order = 3)]
        public string Value { get; set; }

        public static IEqualityComparer<UserClaim> Comparer = new UserClaimsComparer();

        class UserClaimsComparer : IEqualityComparer<UserClaim>
        {
            public bool Equals(UserClaim x, UserClaim y)
            {
                return x.Username == y.Username &&
                       x.Type == y.Type &&
                       x.Value == y.Value;
            }

            public int GetHashCode(UserClaim obj)
            {
                return obj.Username.GetHashCode();
            }
        }
    }
}
