/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BrockAllen.MembershipReboot.WebHost
{
    public static class HttpContextExtensions
    {
        public static string GetApplicationUrl(this HttpContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");

            return new HttpContextWrapper(ctx).GetApplicationUrl();
        }

        public static string GetApplicationUrl(this HttpContextBase ctx)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");

            return ctx.Request.GetApplicationUrl();
        }

        public static string GetApplicationUrl(this HttpRequestBase request)
        {
            if (request == null) throw new ArgumentNullException("request");

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
