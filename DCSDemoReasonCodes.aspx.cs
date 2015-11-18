using System;
using System.Web.Configuration;

namespace DowntimeCollection_Demo
{
    public partial class DCSDemoReasonCodes : BasePage//System.Web.UI.Page
    {
        public string HashCode;

        protected void Page_Load(object sender, EventArgs e)
        {
            string url = WebConfigurationManager.AppSettings["expire url"];

            HashCode = DCSDashboardDemoHelper.CalculateHash(User.Identity.Name.ToLower());
        }
    }
}
