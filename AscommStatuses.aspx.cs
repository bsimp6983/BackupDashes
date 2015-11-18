using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DowntimeCollection_Demo.Classes;

namespace DowntimeCollection_Demo
{
    public partial class AscommStatuses : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
                return;

            if (User.Identity.Name.ToLower() != "admin")
                return;

            if (!IsPostBack)
            {
                grdStatuses.DataSource = DBHelper.GetAscommStatuses();
                grdStatuses.DataBind();
            }
        }
    }
}