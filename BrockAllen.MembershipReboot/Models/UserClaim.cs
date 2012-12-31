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
        [StringLength(50)]
        public virtual string Tenant { get; set; }
        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public virtual string Username { get; set; }
        [Key]
        [Column(Order = 3)]
        [StringLength(150)]
        public virtual string Type { get; set; }
        [Key]
        [Column(Order = 4)]
        [StringLength(150)]
        public virtual string Value { get; set; }
        
        [Required]
        [ForeignKey("Tenant, Username")]
        public virtual UserAccount User { get; set; }
    }
}
