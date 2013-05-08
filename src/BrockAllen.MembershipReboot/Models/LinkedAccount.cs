using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class LinkedAccount
    {
        public LinkedAccount()
        {
            this.Claims = new List<LinkedAccountClaim>();
        }

        [Key]
        [Column(Order=1)]
        [StringLength(50)]
        public virtual string ProviderName { get; set; }
        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public virtual string ProviderAccountID { get; set; }
        
        public virtual Guid LocalAccountID { get; set; }
        public virtual DateTime LastLogin { get; set; }

        public virtual ICollection<LinkedAccountClaim> Claims { get; internal set; }
    }
}
