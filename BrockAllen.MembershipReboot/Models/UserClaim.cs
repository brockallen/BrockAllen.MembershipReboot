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
        public virtual int ID { get; set; }
        //public virtual UserAccount User { get; set; }
        public virtual string Type { get; set; }
        public virtual string Value { get; set; }
    }
}
