using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DowntimeCollection_Demo
{
    public partial class AnnualSaving : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string op = Request["op"];
            if (!string.IsNullOrEmpty(op))
            {
                string result = string.Empty;
                switch (op)
                {
                    case "updatePerson":
                        result = UpdatePerson();
                        break;
                    case "setSavings":
                        result = SetSavings();
                        break;
                }

                Response.Write(result);
                Response.End();
            }
        }

        protected string UpdatePerson()
        {
            string name = Request["name"];
            string number = Request["number"];
            string email = Request["email"];
            string type = Request["type"];
            decimal savings = 0M;

            decimal.TryParse(Request["savings"], out savings);

            return DCSDashboardDemoHelper.updatePersonSavings(name, number, email, savings, type).ToString();


        }
    
        protected string SetSavings()
        {
            decimal savings = 0M;

            decimal.TryParse(Request["savings"], out savings);

            return DCSDashboardDemoHelper.setSavings(savings).ToString();
        }
    }
}