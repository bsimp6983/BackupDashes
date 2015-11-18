using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;

namespace DowntimeCollection_Demo
{
    public partial class DCSDemo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url = WebConfigurationManager.AppSettings["expire url"];
            CheckExpireHelper.RedirectIfExpired(Context, url);
        }
    }
}
