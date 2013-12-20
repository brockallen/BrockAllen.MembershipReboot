using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models
{
    public class PasswordResetInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class PasswordResetWithSecretInputModel
    {
        public PasswordResetWithSecretInputModel ()
	    {
	    }

        public PasswordResetWithSecretInputModel(Guid accountID)
        {
            var bytes = Encoding.UTF8.GetBytes(accountID.ToString());
            bytes = MachineKey.Protect(bytes, "PasswordResetWithSecretViewModel");
            ProtectedAccountID = Convert.ToBase64String(bytes);
        }

        public PasswordResetSecretViewModel[] Questions { get; set; }
        [Required]
        public string ProtectedAccountID { get; set; }

        public Guid? UnprotectedAccountID
        {
            get
            {
                try
                {
                    if (this.ProtectedAccountID != null)
                    {
                        var bytes = Convert.FromBase64String(this.ProtectedAccountID);
                        bytes = MachineKey.Unprotect(bytes, "PasswordResetWithSecretViewModel");
                        var val = Encoding.UTF8.GetString(bytes);
                        return Guid.Parse(val);
                    }
                }
                catch { }
                return null;
            }
        }
    }

    public class PasswordResetSecretViewModel : PasswordResetSecretInputModel
    {
        public string Question { get; set; }
    }

    public class PasswordResetSecretInputModel
    {
        [HiddenInput]
        public Guid QuestionID { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Answer { get; set; }
    }
}