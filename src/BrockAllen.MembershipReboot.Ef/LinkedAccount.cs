using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace BrockAllen.MembershipReboot.Ef
{
    public class LinkedAccount : ILinkedAccount
    {
        [Key]
        [Column(Order = 1)]
        public virtual Guid UserAccountID { get; set; }
        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public virtual string ProviderName { get;  set; }
        [Key]
        [Column(Order = 3)]
        [StringLength(100)]
        public virtual string ProviderAccountID { get;  set; }

        public virtual DateTime LastLogin { get;  set; }

        [Required]
        [ForeignKey("UserAccountID")]
        public virtual UserAccount User { get;  set; }

        public virtual ICollection<IUserClaim> Claims { get;  set; }

        public IUserClaim CreateClaim()
        {
            return new UserClaim();
        }
    }
}
