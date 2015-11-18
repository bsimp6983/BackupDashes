using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DowntimeCollection_Demo;

namespace DCSDemoData
{
    public partial class DCSDemo : BasePage// System.Web.UI.Page
    {
        private HttpRequest request;
        private string _currentClient;
        public string Line = null;
        public string HashCode;

        protected void Page_Load(object sender, EventArgs e)
        {
            request = Context.Request;

            _currentClient = Context.User.Identity.Name;

            Line = request["line"];
            HashCode = DCSDashboardDemoHelper.CalculateHash(User.Identity.Name.ToLower());

            if (string.IsNullOrEmpty(Line))
                Line = "company-demo";

            Line = Line.Replace('#', ' ').Trim();

            ProductionSchedule ps = DiamondCrystaldashboardHelper.getLatestProductionSchedule(Line);

            if (ps != null)
            {
                drpLights.SelectedValue = (ps.LightsOn == true ? "On" : "Off");
                lblSince.Text = ps.EventTime.ToString("MM/dd/yyyy hh:mm tt");
            }

            lblLine.Text = Line;
            lightForm.Attributes.CssStyle["display"] = "none";
         

            if (DiamondCrystaldashboardHelper.hasLightsOnFeature())
            {
                lightForm.Attributes.CssStyle["display"] = "";
                lblLine.Attributes.CssStyle["margin-right"] = "2%";
            }
        }
    }
}
