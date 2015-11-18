using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DCSDemoData
{
    public partial class CHGIndex : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Context.User.Identity.IsAuthenticated)
                Context.Response.Redirect("Login.aspx?ReturnUrl=CHGIndex.aspx");
        }
    }
}