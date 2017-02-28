
namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class ChangeMobileRequestInputModel
    {
        public string Current { get; set; }
        //[Required]
        public string NewMobilePhone { get; set; }
    }
}