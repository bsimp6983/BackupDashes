using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace DowntimeCollection_Demo
{
    public partial class Active : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string username = Request["user"];
            string activeCode;
            if (!string.IsNullOrEmpty(username))
            {
                activeCode = Request["code"];
                MembershipUser mu = Membership.GetUser(username);
                if (mu == null)
                {
                    Response.Write("User not found.");
                }
                else
                {
                    WebProfile profile = WebProfile.GetProfile(mu.UserName);
                    if (profile.ActiveCode != activeCode)
                    {
                        Response.Write("Active Code is invalid.");
                    }
                    else
                    {
                        mu.IsApproved = true;
                        Membership.UpdateUser(mu);
                        Response.Write("Your account has been activated.");
                    }
                }
            }
            else
            {
                Response.Write("Arguments is invalid.");
            }
        }
    }
}