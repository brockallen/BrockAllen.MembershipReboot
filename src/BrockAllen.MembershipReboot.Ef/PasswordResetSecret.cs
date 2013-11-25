using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace BrockAllen.MembershipReboot.Ef
{
    public class PasswordResetSecret : IPasswordResetSecret
    {
        [Key]
        [Column(Order = 1)]
        public virtual Guid ID { get; set; }

        [Key]
        [Column(Order = 2)]
        public virtual Guid UserAccountID { get; set; }

        [StringLength(150)]
        [Required]
        public virtual string Question { get; set; }

        [StringLength(150)]
        [Required]
        public virtual string Answer { get; set; }

        [Required]
        [ForeignKey("UserAccountID")]
        public virtual UserAccount User { get; set; }
    }
}
