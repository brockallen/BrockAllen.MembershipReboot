namespace TimeBasedOneTimePasswords.Areas.UserAccount.Models
{
    public class RFC6238Model
    {
        public BrockAllen.MembershipReboot.UserAccount Account { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
}
