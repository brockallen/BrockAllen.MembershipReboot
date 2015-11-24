using System.Web.Mvc;

namespace TimeBasedOneTimePasswords.Areas.UserAccount
{
    public class UserAccountAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "UserAccount";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "UserAccount_default",
                "UserAccount/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
