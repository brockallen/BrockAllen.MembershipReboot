using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace BrockAllen.MembershipReboot.Ef
{
    public class UserClaim : IUserClaim
    {
        [Key]
        [Column(Order = 1)]
        public virtual Guid UserAccountID { get; set; }
        [Key]
        [Column(Order = 2)]
        [StringLength(150)]
        public virtual string Type { get; set; }
        [Key]
        [Column(Order = 3)]
        [StringLength(150)]
        public virtual string Value { get; set; }

        [Required]
        [ForeignKey("UserAccountID")]
        public virtual UserAccount User { get; set; }

    }
}
