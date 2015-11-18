using System;
using System.IO;
using System.Linq;
using System.Web.Security;

namespace DowntimeCollection_Demo
{
    public partial class _Default : BasePage//System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            var membershipUser = Membership.GetUser();
            if (membershipUser == null) return;

            var providerUserKey = membershipUser.ProviderUserKey;
            if (providerUserKey == null) return;

            Guid UserId = (Guid)providerUserKey;

            string line = string.Empty;

            string userName = membershipUser.UserName.ToLower();

            switch (userName)
            {
                case "gfg":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\GardenFreshDashboard.aspx");
                    break;
                case "griffinpipe":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\GriffinPipe.aspx");
                    break;
                case "sage":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\SageIndexNew.aspx");
                    break;
                case "sageqa":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\SageQAIndex.aspx");
                    break;
                case "chg":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\CHGIndex.aspx");
                    break;
                case "txi":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\TXI.aspx");
                    break;
                case "skfoods":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\SkFood.aspx");
                    break;
                case "sunorchard":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\sunorchard.html");
                    break;
                case "millarddcs":
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\millard.html");
                    break;
                case "diamondcrystal":
                {
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\DiamondCrystalIndex.aspx");
                    //line = "?line=Shaker";
                    break;
                }
                default:
                {
                    if (File.Exists(Server.MapPath("~/" + userName + ".html")))
                    {
                        Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\" + userName + ".html");
                    }

                    break;
                }
            }

            lnkReasonCodes.OnClientClick = "window.open(\"DCSDemoReasonCodes.aspx" + line + "\", \"_self\"); return false;";
            lnkDcsDemo.OnClientClick = "window.open(\"DCSDemo.aspx" + line + "\", \"_self\"); return false;";

            using (DB db = new DB())
            {
                UserInfo info = (from o in db.UserInfoSet
                    where o.UserId == UserId
                    select o).FirstOrDefault();

                if (info != null)
                {
                    if (info.EffChartEnabled == true)
                        analyzeData.OnClientClick = "window.open(\"" + DCSDashboardDemoHelper.BaseVirtualAppPath + "\\DCSDashboard.aspx" + line + "\", \"_self\");return false;";
                    else
                        analyzeData.OnClientClick = "window.open(\"" + DCSDashboardDemoHelper.BaseVirtualAppPath + "\\DCSDashboardDemo.aspx" + line + "\", \"_self\");return false;";

                }
                else
                {
                    analyzeData.OnClientClick = "window.open(\"" + DCSDashboardDemoHelper.BaseVirtualAppPath + "\\DCSDashboardDemo.aspx" + line + "\", \"_self\");return false;";
                }
            }
        }
    }
}