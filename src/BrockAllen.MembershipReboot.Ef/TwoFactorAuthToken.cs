using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace BrockAllen.MembershipReboot.Ef
{
    public class TwoFactorAuthToken : ITwoFactorAuthToken
    {
        [Key]
        [Column(Order = 1)]
        public virtual Guid UserAccountID { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public virtual string Token { get; set; }

        public virtual DateTime Issued { get; set; }
    }
}
