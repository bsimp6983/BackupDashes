using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DowntimeCollection_Demo.Admin
{
    public partial class PersonSavings : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                using(DB db = new DB())
                {
                    List<PeopleSavings> list = (from o in db.PeopleSavings
                                               select o).ToList();

                    grdPeople.DataSource = list;
                    grdPeople.DataBind();
                }

            }
        }
    }
}