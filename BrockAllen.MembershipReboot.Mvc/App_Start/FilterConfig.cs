using System.Web;
using System.Web.Mvc;

namespace BrockAllenBrockAllen.MembershipReboot.Mvc.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}