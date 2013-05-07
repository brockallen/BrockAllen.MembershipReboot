using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BrockAllen.MembershipReboot
{
    public static class HttpContextExtensions
    {
        public static string GetApplicationUrl(this HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            
            var request = ctx.Request;
            
            var baseUrl =
                request.Url.Scheme +
                "://" +
                request.Url.Host + (request.Url.Port == 80 ? "" : ":" + request.Url.Port) +
                request.ApplicationPath;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            
            return baseUrl;
        }
    }
}
