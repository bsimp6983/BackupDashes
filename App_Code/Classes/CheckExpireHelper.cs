using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DowntimeCollection_Demo
{
    public static class CheckExpireHelper
    {
        public static void RedirectIfExpired(HttpContext context, string url)
        {
            if (string.Equals("admin", context.User.Identity.Name, StringComparison.CurrentCultureIgnoreCase)) return;
            WebProfile profile = WebProfile.GetProfile(context.User.Identity.Name);

            if (profile.ExpireDate < DateTime.Now)
            {
                context.Response.Redirect(url);
            }
        }

    }


}