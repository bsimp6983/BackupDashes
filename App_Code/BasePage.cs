using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace DowntimeCollection_Demo
{
    public class BasePage : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            WebProfile profile = WebProfile.GetProfile(User.Identity.Name);

            if (profile != null)
            {
                string line = HttpContext.Current.Request["line"];

                if (string.IsNullOrEmpty(line))
                {
                    if (User.Identity.Name.ToLower() == "millarddcs")
                    {
                        line = "line1";

                        string url = HttpContext.Current.Request.Url.PathAndQuery;

                        if (url.Contains("?"))
                        {
                            url += "&line=" + line;
                        }
                        else
                        {
                            url += "?line=" + line;
                        }

                        HttpContext.Current.Response.Redirect(url);
                    }
                }
            }
            else
            {
                HttpContext.Current.Response.Redirect("~/Login.aspx?ReturnUrl=" + Path.GetFileName(HttpContext.Current.Request.Url.AbsolutePath) + HttpContext.Current.Request.Url.Query);
            }

            base.OnInit(e);
        }
    }
}