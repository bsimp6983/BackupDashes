using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.Configuration;
using System.Web.Security;
using DowntimeCollection_Demo.Classes;

namespace DowntimeCollection_Demo
{
    public partial class DCSDashboardDemo : BasePage// System.Web.UI.Page
    {
        string Line = string.Empty;

        int? DetailId;

        string DetailKey = string.Empty;
        string DetailValue = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            Line = Request["line"];

            int dId = -1;

            int.TryParse(Request["detailId"], out dId);

            if (dId > 0)
                DetailId = dId;


            if (User.Identity.Name != "admin")
            {
                string line = string.Empty;

                if (!string.IsNullOrEmpty(Line))
                    line = "?line=" + Line;

                if (Membership.GetUser() != null)
                {
                    Guid UserId = (Guid)Membership.GetUser().ProviderUserKey;

                    using (DB db = new DB())
                    {
                        UserInfo info = (from o in db.UserInfoSet
                                         where o.UserId == UserId
                                         select o).FirstOrDefault();

                        if (info != null)
                        {
                            if (info.EffChartEnabled == true)
                                Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\DCSDashboard.aspx" + line);
                        }
                    }

                }
                else
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\Login.aspx" + line);
            }

            string url = WebConfigurationManager.AppSettings["expire url"];

            if (User.Identity.Name.ToLower() == "txi" || User.Identity.Name.ToLower() == "admin")
                divKPIS.Visible = true;
            else
                divKPIS.Visible = false;

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Line))
                    Line = "company-demo";

                clientLines.Items.Clear();

                List<string> lines = DCSDashboardDemoHelper.getLines();

                lines.Sort();

                foreach (string line in lines)
                {
                    string l = line.Replace("#", "").Trim();

                    ListItem item = clientLines.Items.FindByValue(l);

                    if (item == null && !string.IsNullOrEmpty(l))
                    {
                        item = new ListItem(l);

                        clientLines.Items.Add(item);
                    }
                }

                foreach (ListItem item in clientLines.Items)
                {
                    if (string.IsNullOrEmpty(item.Text))
                        clientLines.Items.Remove(item);
                }
            }

            string op = Request["op"];

            if (!string.IsNullOrEmpty(op))
            {
                DateTime? startDate = null;
                DateTime? endDate = null;
                DateTime d;
                if (DateTime.TryParse(Request.Form["startdate"], out d))
                {
                    startDate = d;
                }
                if (DateTime.TryParse(Request.Form["enddate"], out d))
                {
                    endDate = d;
                }


                string xml = string.Empty;
                switch (op)
                {
                    case "LossBuckets":
                        xml = getLossBuckets();
                        break;
                    case "LossBuckets_Occuring":
                        xml = LossBuckets_Occuring();
                        break;
                    case "Top5DowntimeEvents":
                        xml = Top5DowntimeEvents(true);
                        break;
                    case "Top5OccuringEvents":
                        xml = Top5DowntimeEvents(false);
                        break;
                    case "DowntimeActualVsGoal":
                        xml = DowntimeActualVsGoal();
                        break;
                    case "Top5DowntimeEvents_Top5DowntimeEvents":
                        xml = Top5DowntimeEvents_Top5DowntimeEvents();
                        break;
                    case "Top5OccuringEvents_Top5OccuringEvents":
                        xml = Top5OccuringEvents_Top5OccuringEvents();
                        break;
                    case "Top5DowntimeEvents_DowntimeActualVsGoal":
                        xml = Top5DowntimeEvents_DowntimeActualVsGoal();
                        break;
                    case "Top5OccuringEvents_OccuringActualVsGoal":
                        xml = Top5OccuringEvents_OccuringActualVsGoal();
                        break;
                    case "Top5OccuringEvents_Comments":
                    case "Top5DowntimeEvents_Comments":
                        xml = Top5DowntimeEvents_Comments();
                        break;
                    case "HistoricalDetail_DowntimeHistory":
                        xml = HistoricalDetail_DowntimeHistory();
                        break;
                    case "HistoricalDetail_OccurrenceHistory":
                        xml = HistoricalDetail_OccurrenceHistory();
                        break;
                    case "TotalDowntime":
                        xml = DCSDashboardDemoHelper.TotalDowntime(startDate, endDate, Line).ToString();
                        break;
                    case "RedLineList":
                        xml = getRedLines();//ConvertToJsonString(DCSDashboardDemoHelper.GetGoals());
                        break;
                    case "updateredline":
                        int update_Id, update_occ;
                        decimal update_downtime;

                        int.TryParse(Request["id"], out update_Id);
                        if (!startDate.HasValue)
                        {
                            xml = "please input a date";
                            break;
                        }

                        if (!endDate.HasValue)
                        {
                            xml = "please input a date";
                            break;
                        }

                        decimal.TryParse(Request["downtime"], out update_downtime);
                        int.TryParse(Request.Form["Occuring"], out update_occ);

                        xml = (DCSDashboardDemoHelper.UpdateGoal(update_Id, startDate.Value, endDate.Value, update_downtime, update_occ) ? "1" : "Update failed,please try again.");
                        break;
                    case "addredline":
                        decimal add_downtime;
                        int add_occ;

                        if (!startDate.HasValue)
                        {
                            xml = "please input a date";
                            break;
                        }

                        if (!endDate.HasValue)
                        {
                            xml = "please input a date";
                            break;
                        }
                        decimal.TryParse(Request.Form["downtime"], out add_downtime);
                        int.TryParse(Request.Form["Occuring"], out add_occ);
                        xml = (DCSDashboardDemoHelper.InsertGoal(startDate.Value, endDate.Value, add_downtime, add_occ, Line) ? "1" : "0");
                        break;
                    case "deleteredline":
                        int delete_Id;
                        int.TryParse(Request["id"], out delete_Id);

                        xml = (DCSDashboardDemoHelper.DeleteGoal(delete_Id) ? "1" : "Update failed,please try again.");
                        break;
                    case "GetEventRows":
                        xml = GetEventRows();
                        break;
                    case "Hidden_top5downtime":
                        xml = Hidden_Top5DowntimeEvents(true);
                        break;
                    case "Hidden_top5occuring":
                        xml = Hidden_Top5DowntimeEvents(false);
                        break;
                    case "Hidden_DowntimeActualVsGoal":
                        xml = Hidden_DowntimeActualVsGoal();
                        break;
                    case "Hidden_GetEventRows":
                        xml = Hidden_GetEventRows();
                        break;
                }

                Response.Write(xml);
                Response.End();
            }
        }

        private string getRedLines()
        {
            List<ClientGoalRow> list = DCSDashboardDemoHelper.GetGoals(Line);

            List<object> objs = new List<object>();

            foreach (ClientGoalRow row in list)
            {
                objs.Add(new
                {
                    Id = row.Id,
                    Downtime = row.Downtime,
                    EndTimeStr = row.EndTimeStr,
                    Occuring = row.Occuring,
                    StartTimeStr = row.StartTimeStr
                }
                );
            }

            return ConvertToJsonString(objs);
        }


        private string getLossBuckets()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            List<LossBucketsRow> result = new List<LossBucketsRow>();

            if (DetailId.HasValue)
            {
                result = DBHelper.LossBuckets(startDate, endDate, Line, DBHelper.GetDetailKey(DetailId.Value), true).Take(8).ToList(); ;
            }
            else
            {
                result = DCSDashboardDemoHelper.LossBuckets(startDate, endDate, Line, true).Take(8).ToList(); ;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Downtime Total by Percent (Top 8)\" FontSize=\"14\" Padding=\"5\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis Suffix=\"%\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\" Suffix=\"%\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine("     <vc:Axis.AxisLabels>");
            sb.AppendLine("         <vc:AxisLabels Angle=\"0\"/>");//-60
            sb.AppendLine("     </vc:Axis.AxisLabels>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");


            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Column\" LabelEnabled=\"True\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            if (result != null)
            {
                foreach (var item in result)
                {
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Level1, item.MinutesPercent);
                }
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");
            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");


            return sb.ToString();
        }

        private string LossBuckets_Occuring()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }

            List<LossBucketsRow> result = new List<LossBucketsRow>();

            if (DetailId.HasValue)
            {
                result = DBHelper.LossBuckets(startDate, endDate, Line, DBHelper.GetDetailKey(DetailId.Value), false).Take(8).ToList();
            }
            else
            {
                result = DCSDashboardDemoHelper.LossBuckets(startDate, endDate, Line, false).Take(8).ToList();
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Downtime Total by Percent (Top 8)\" FontSize=\"14\" Padding=\"5\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis Suffix=\"%\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\" Suffix=\"%\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine("     <vc:Axis.AxisLabels>");
            sb.AppendLine("         <vc:AxisLabels Angle=\"-60\"/>");
            sb.AppendLine("     </vc:Axis.AxisLabels>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");


            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Column\" LabelEnabled=\"True\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            if (result != null)
            {
                foreach (var item in result)
                {
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Level1, item.OccurencesPercent);
                }
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");
            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");


            return sb.ToString();
        }

        //private int getGridInterval(

        private string Top5DowntimeEvents(bool showMinutes)
        {
            bool isHome = !string.IsNullOrEmpty(Request.Form["isHome"]);
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }

            List<TopEventsRow> result = new List<TopEventsRow>();

            if (DetailId.HasValue)
            {
                result = DBHelper.TopEvents(startDate, endDate, DetailId.Value, level1, 5, Line, showMinutes);
            }
            else
            {
                result = DCSDashboardDemoHelper.TopEvents(startDate, endDate, level1, 5, Line, showMinutes);
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");

            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine(" <vc:Title " + (isHome ? "Cursor=\"Hand\" FontColor=\"Blue\" TextDecorations=\"Underline\"" : "") + " Padding=\"5\" Text=\"" + (showMinutes ? "Top 5 Downtime Events" : "Top 5 Occuring Events") + " > " + (string.IsNullOrEmpty(level1) ? "ALL" : level1) + "\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");


            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Column\" LabelEnabled=\"True\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            if (result != null)
            {
                foreach (var item in result)
                {
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\" LegendText=\"{2}\"/>", item.LevelString(), (showMinutes ? item.Minutes : item.Occurences), item.ReasonCodeId);
                }
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");
            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");


            return sb.ToString();
        }

        private string DowntimeActualVsGoal()
        {
            bool isHome = !string.IsNullOrEmpty(Request.Form["isHome"]);
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            int goal = 10;
            if (!int.TryParse(Request.Form["goal"], out goal)) goal = 0;

            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];
            string type = Request.Form["type"];
            if (string.IsNullOrEmpty(type)) type = "day";
            bool isEvents = false;
            if (!bool.TryParse(Request.Form["isEvents"], out isEvents)) isEvents = true;


            GoalReportRow grr = new GoalReportRow();

            if (DetailId.HasValue)
            {
                grr = DBHelper.GetGoals(startDate, endDate, DetailId.Value, level1, 0, Line);

            }
            else
            {
                grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0, Line);
            }

            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    if(DetailId.HasValue)
                        rows = DBHelper.HoursReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.HoursReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Hour) : grr.Occurences.Hour);
                    break;
                case "day":
                    if(DetailId.HasValue)
                        rows = DBHelper.DayReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.DayReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    if(DetailId.HasValue)
                        rows = DBHelper.WeekReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.WeekReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    if(DetailId.HasValue)
                        rows = DBHelper.MonthReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.MonthReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    if(DetailId.HasValue)
                        rows = DBHelper.YearReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.YearReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default:
                    if(DetailId.HasValue)
                        rows = DBHelper.DayReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.DayReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;

            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            //sb.AppendLine("<vc:Title Text=\"" + (isEvents ? "Downtime Actual vs. Goal" : "Occurences Actual vs. Goal") + " > " + level1 + "\" FontSize=\"14\"/>");
            sb.AppendLine("<vc:Title " + (isHome ? "Cursor=\"Hand\" Padding=\"5\" FontColor=\"Blue\" TextDecorations=\"Underline\"" : "") + " Text=\"" + (isEvents ? "Downtime Actual vs. Goal" : "Occurences Actual vs. Goal") + "\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.TrendLines>");
            sb.AppendLine("     <vc:TrendLine LineColor=\"Red\" LineThickness=\"2\" Value=\"" + goal.ToString() + "\" LabelText=\"\"/>");
            sb.AppendLine("</vc:Chart.TrendLines>");

            sb.AppendLine("<vc:Chart.Legends>");
            sb.AppendLine("     <vc:Legend Enabled=\"False\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" />");
            sb.AppendLine("</vc:Chart.Legends>");

            sb.AppendLine("<vc:Chart.AxesY>");
            if (rows.Max(o => (isEvents ? o.Minutes : o.Occurences)) < goal)
            {
                sb.AppendLine(" <vc:Axis AxisMaximum=\"" + (goal + Convert.ToInt32(goal * 0.3)).ToString() + "\">");
            }
            else
            {
                sb.AppendLine(" <vc:Axis >");
            }
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Blue\" LabelEnabled=\"True\" LegendText=\"" + (isEvents ? "Downtime" : "Occurences") + "\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, (isEvents ? item.Minutes : item.Occurences));
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");

            //就为了加个legend到右边，这个蛋痛的
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Red\"  LineThickness=\"0\" LabelEnabled=\"False\" LegendText=\"Goal\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Red\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", "", goal);
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");

            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");

            return sb.ToString();

        }

        private string Top5DowntimeEvents_Top5DowntimeEvents()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }

            if (DetailId.HasValue)
                return Top5DowntimeEvents(true);


            List<SpecialReportRow> result = DCSDashboardDemoHelper.TopEventsGroupByLevel3(startDate, endDate, level1, 5, true, Line);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Top 5 Downtime Events > " + (string.IsNullOrEmpty(level1) ? "ALL" : level1) + "\" Padding=\"5\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Column\" LabelEnabled=\"True\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            if (result != null)
            {
                foreach (var item in result)
                {
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\" LegendText=\"{2}\"/>", item.Title, item.Minutes, item.appInfo);
                }
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");
            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");


            return sb.ToString();
        }

        private string Top5DowntimeEvents_DowntimeActualVsGoal()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            int goal = 10;
            if (!int.TryParse(Request.Form["goal"], out goal)) goal = 0;

            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];
            string type = Request.Form["type"];
            if (string.IsNullOrEmpty(type)) type = "day";

            string level3 = Request.Form["level3"];
            int level3_id = 0;
            int.TryParse(Request.Form["level3_id"], out level3_id);

            if (DetailId.HasValue)
                return DowntimeActualVsGoal();

            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id, Line);

            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DCSDashboardDemoHelper.HoursReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Hour);
                    break;
                case "day":
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.WeekReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.MonthReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.YearReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Year);
                    break;
                default:
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;

            }

            if (rows != null)
            {
                rows.OrderByDescending(o => o.Minutes);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Downtime Actual vs. Goal\" FontSize=\"14\" Padding=\"5\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.Legends>");
            sb.AppendLine("     <vc:Legend VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" />");
            sb.AppendLine("</vc:Chart.Legends>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");

            //downtime
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Blue\" LabelEnabled=\"True\" LegendText=\"Downtime\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, item.Minutes);
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");


            //occurences
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Green\" LabelEnabled=\"True\" LegendText=\"Occuring\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Green\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, item.Occurences);
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");

            ////就为了加个legend到右边，这个蛋痛的
            //sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Red\"  LineThickness=\"0\" LabelEnabled=\"False\" LegendText=\"Goal\">");
            //sb.AppendLine("<vc:DataSeries.DataPoints>");
            //foreach (var item in rows)
            //{
            //    sb.AppendFormat("<vc:DataPoint BorderColor=\"Red\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", "", goal);
            //}
            //sb.AppendLine("</vc:DataSeries.DataPoints>");
            //sb.AppendLine("</vc:DataSeries>");

            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");

            return sb.ToString();

        }

        private string Top5DowntimeEvents_Comments()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            string level1 = Request.Form["level1"];
            string level3 = Request.Form["level3"];
            int level3_id = 0;
            int.TryParse(Request.Form["level3_id"], out level3_id);

            List<object> list = new List<object>();

            List<CommentRow> comments = new List<CommentRow>();

            if(DetailId.HasValue)
            {
                if (string.IsNullOrEmpty(level1))
                    level1 = level3;

                comments = DBHelper.Comments(startDate, endDate, level3_id, DetailId.Value, level1, Line).Where(o => !string.IsNullOrEmpty(o.Comment)).ToList();
            }
            else
            {
                comments = DCSDashboardDemoHelper.Comments(startDate, endDate, level3_id, level1, Line);
            }

            foreach (CommentRow row in comments)
            {
                list.Add(new
                {
                    Client = row.Client,
                    Comment = row.Comment,
                    EventStart = (row.EventStart.HasValue ? row.EventStart.Value.ToString("MM/dd/yyyy") : ""),
                    EventStartTimeStr = row.EventStartTimeStr,
                    EventStop = (row.EventStart.HasValue ? row.EventStop.Value.ToString("MM/dd/yyyy") : ""),
                    EventStopTimeStr = row.EventStopTimeStr,
                    Line = row.Line,
                    Minutes = row.Minutes,
                    ReasonCodeId = row.ReasonCodeId,
                    Level1 = row.Level1,
                    Level2 = row.Level2,
                    Level3 = row.Level3
                });
            }

            string result = ConvertToJsonString(list);

            return result;
        }

        public string ConvertToJsonString(object obj)
        {
            JsonSerializer js = new JsonSerializer();
            js.Converters.Add(new JavaScriptDateTimeConverter());
            System.IO.TextWriter tw = new System.IO.StringWriter();
            js.Serialize(tw, obj);
            return tw.ToString();

        }

        private string Top5OccuringEvents_Top5OccuringEvents()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }


            if (DetailId.HasValue)
                return Top5DowntimeEvents(false);

            List<SpecialReportRow> result = DCSDashboardDemoHelper.TopEventsGroupByLevel3(startDate, endDate, level1, 5, false, Line);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Top 5 Occuring Events > " + (string.IsNullOrEmpty(level1) ? "ALL" : level1) + "\" FontSize=\"14\" Padding=\"5\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Column\" LabelEnabled=\"True\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            if (result != null)
            {
                foreach (var item in result)
                {
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\" LegendText=\"{2}\"/>", item.Title, item.Occurences, item.appInfo);
                }
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");
            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");


            return sb.ToString();
        }

        private string Top5OccuringEvents_OccuringActualVsGoal()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            int goal = 10;
            if (!int.TryParse(Request.Form["goal"], out goal)) goal = 0;

            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];
            string type = Request.Form["type"];
            if (string.IsNullOrEmpty(type)) type = "day";

            string level3 = Request.Form["level3"];
            int level3_id = 0;
            int.TryParse(Request.Form["level3_id"], out level3_id);

            if (DetailId.HasValue)
                return DowntimeActualVsGoal();

            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id, Line);

            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DCSDashboardDemoHelper.HoursReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Hour);
                    break;
                case "day":
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.WeekReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.MonthReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.YearReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Year);
                    break;
                default:
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id, Line);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;

            }

            if (rows != null)
            {
                rows = rows.OrderByDescending(o => o.Minutes).ToList();
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            //sb.AppendLine("<vc:Title Text=\"Occuring Actual vs. Goal > " + level3 + "\" FontSize=\"14\"/>");
            sb.AppendLine("<vc:Title Text=\"Occuring Actual vs. Goal\" FontSize=\"14\" Padding=\"5\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            //sb.AppendLine("<vc:Chart.TrendLines>");
            //sb.AppendLine("     <vc:TrendLine LineColor=\"Red\" LineThickness=\"2\" Value=\"" + goal.ToString() + "\" LabelText=\"\"/>");
            //sb.AppendLine("</vc:Chart.TrendLines>");

            sb.AppendLine("<vc:Chart.Legends>");
            sb.AppendLine("     <vc:Legend VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" />");
            sb.AppendLine("</vc:Chart.Legends>");

            //if (rows.Max(o => (o.Minutes)) < goal)
            //{
            //    sb.AppendLine("<vc:Chart.AxesY>");
            //    sb.AppendLine("     <vc:Axis AxisMaximum=\"" + (goal + Convert.ToInt32(goal * 0.3)).ToString() + "\" />");
            //    sb.AppendLine("</vc:Chart.AxesY>");
            //}

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");


            sb.AppendLine("<vc:Chart.Series>");

            //downtime
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Blue\" LabelEnabled=\"True\" LegendText=\"Downtime\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, item.Minutes);
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");


            //occurences
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Green\" LabelEnabled=\"True\" LegendText=\"Occuring\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Green\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, item.Occurences);
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");



            ////就为了加个legend到右边，这个蛋痛的
            //sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Red\"  LineThickness=\"0\" LabelEnabled=\"False\" LegendText=\"Goal\">");
            //sb.AppendLine("<vc:DataSeries.DataPoints>");
            //foreach (var item in rows)
            //{
            //    sb.AppendFormat("<vc:DataPoint BorderColor=\"Red\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", "", goal);
            //}
            //sb.AppendLine("</vc:DataSeries.DataPoints>");
            //sb.AppendLine("</vc:DataSeries>");

            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");

            return sb.ToString();

        }

        private string HistoricalDetail_DowntimeHistory()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            int goal = 10;
            if (!int.TryParse(Request.Form["goal"], out goal)) goal = 0;

            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];
            string type = Request.Form["type"];
            if (string.IsNullOrEmpty(type)) type = "day";

            string level3 = Request.Form["level3"];
            int level3_id = 0;
            int.TryParse(Request.Form["level3_id"], out level3_id);


            if (DetailId.HasValue)
                return DowntimeActualVsGoal();

            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id, Line);
            Dictionary<string, List<DowntimeHistoryRow>> rows = DCSDashboardDemoHelper.getDowntimesHistory(startDate, endDate, level3_id, type, Line);


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Downtime History > " + (string.IsNullOrEmpty(level3) ? "ALL" : level3) + "\" Padding=\"5\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");

            //downtime
            foreach (var row in rows)
            {
                sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" LabelEnabled=\"True\" LegendText=\"" + row.Key + "\">");
                sb.AppendLine("<vc:DataSeries.DataPoints>");
                foreach (var item in row.Value)
                {
                    sb.AppendFormat("<vc:DataPoint BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Level3, item.MinutesSum);
                }
                sb.AppendLine("</vc:DataSeries.DataPoints>");
                sb.AppendLine("</vc:DataSeries>");
            }

            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");

            return sb.ToString();
        }

        private string HistoricalDetail_OccurrenceHistory()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            int goal = 10;
            if (!int.TryParse(Request.Form["goal"], out goal)) goal = 0;

            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];
            string type = Request.Form["type"];
            if (string.IsNullOrEmpty(type)) type = "day";

            string level3 = Request.Form["level3"];
            int level3_id = 0;
            int.TryParse(Request.Form["level3_id"], out level3_id);


            List<AllOccurrenceHistoryRow> rows = new List<AllOccurrenceHistoryRow>();

            if (DetailId.HasValue)
            {
                rows = DBHelper.getOccurrenceHistory(startDate, endDate, DetailId.Value, level1, type, Line);
            }
            else
            {
                rows = DCSDashboardDemoHelper.getOccurrenceHistory(startDate, endDate, level3_id, type, Line);//ReportHelper.MonthOccurrenceHistory(startDate, endDate, level3_id);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Occurrence History > " + (string.IsNullOrEmpty(level3) ? "ALL" : level3) + "\" Padding=\"5\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");

            foreach (var item in rows)
            {
                sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" LabelEnabled=\"True\" LegendText=\"" + item.Level3 + "\">");
                sb.AppendLine("<vc:DataSeries.DataPoints>");
                foreach (var data in item.Datas)
                {
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\"/>", data.Key, data.Value);
                }
                sb.AppendLine("</vc:DataSeries.DataPoints>");
                sb.AppendLine("</vc:DataSeries>");
            }

            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");

            return sb.ToString();
        }

        private string GetEventRows()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }

            string level1 = Request.Form["level1"];
            List<EventRowWithAllColumns> rows = new List<EventRowWithAllColumns>();

            if (DetailId.HasValue)
            {
                rows = DBHelper.GetEventRows(startDate, endDate, DetailId.Value, level1, Line);
            }
            else
            {
                rows = DCSDashboardDemoHelper.GetEventRows(startDate, endDate, level1, Line);
            }


            var q = from o in rows
                    group o by new { o.Level1, o.Level2, o.Level3 } into g
                    select new { g.Key, Occurrences = g.Count(), Minutes = g.Sum(o => o.Minutes), Path = getPath(g.Key.Level1, g.Key.Level2, g.Key.Level3), StartDate = g.Min(o => o.EventStart).Value.ToString(@"MM\/dd\/yyyy"), EndDate = g.Max(o => o.EventStop).Value.ToString(@"MM\/dd\/yyyy") };

            List<object> results = new List<object>();

            decimal totalOccurrences = q.Sum(o => o.Occurrences);
            decimal totalMinutes = q.Sum(o => o.Minutes.Value);

            if (totalOccurrences == 0)
                totalOccurrences = 1;

            if (totalMinutes == 0)
                totalMinutes = 1;

            decimal lastAvg = 0.00m;
            decimal lastCumulativeDurationPercent = 0.00m;
            decimal lastCumulativeFrequencyPercent = 0.00m;
            decimal lastMinute = 0.00m;
            decimal lastCumulativeMinute = 0.00m;

            decimal lastOccurrence = 0.00m;
            decimal lastCumulativeOccurrence = 0.00m;

            decimal[] avgs = new decimal[2];

            foreach (var r in q.OrderByDescending(o => o.Minutes.Value).Take(15))
            {
                decimal duration = 0.00m;
                decimal frequency = 0.00m;
                decimal average = 0.00m;
                decimal cumulative = 0.00m;

                decimal queriedMinutes = Convert.ToDecimal(endDate.Value.Subtract(startDate.Value).TotalMinutes);
                decimal totalEventMinutes = r.Minutes.Value;
                decimal cumulativeMinute = lastMinute + r.Minutes.Value;
                decimal cumulativeOccurrence = lastOccurrence + r.Occurrences;

                decimal cumulativeDurationPercent = cumulativeMinute / totalMinutes;
                decimal cumulativeFrequencePercent = cumulativeOccurrence / totalOccurrences;

                lastMinute = cumulativeMinute;
                lastOccurrence = cumulativeOccurrence;

                lastCumulativeMinute = lastCumulativeMinute + lastMinute;
                lastCumulativeOccurrence = lastCumulativeOccurrence + lastOccurrence;

                if (queriedMinutes == 0)
                    queriedMinutes = 1;

                duration = totalEventMinutes / queriedMinutes;
                frequency = r.Occurrences / totalOccurrences;

                if (frequency == 0)
                    frequency = 1;


                decimal durationPercent = (cumulativeDurationPercent - lastCumulativeDurationPercent);
                decimal frequencyPercent = (cumulativeFrequencePercent - lastCumulativeFrequencyPercent);

                avgs[0] = durationPercent;
                avgs[1] = frequencyPercent;

                average = avgs.Average();

                cumulative = average + lastAvg;

                results.Add(new
                {
                    Key = r.Key,
                    Occurrences = r.Occurrences,
                    Minutes = r.Minutes,
                    Path = r.Path,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Duration = (durationPercent * 100).ToString("#.##") + "%",
                    Frequency = (frequencyPercent * 100).ToString("#.##") + "%",
                    Average = (average * 100).ToString("#.##") + "%",
                    Cumulative = ( cumulative * 100).ToString("#.##") + "%"
                });

                lastCumulativeDurationPercent = cumulativeDurationPercent;
                lastCumulativeFrequencyPercent = cumulativeFrequencePercent;

                lastAvg = cumulative;
              
            }
            
            return ConvertToJsonString(results);
        }

        private string getPath(string level1, string level2, string level3)
        {
            string p = level1;
            if (!string.IsNullOrEmpty(level2))
            {
                p += " > " + level2;
            }
            if (!string.IsNullOrEmpty(level3))
            {
                p += " > " + level3;
            }

            return p;
        }

        private string Hidden_Top5DowntimeEvents(bool showMinutes)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            List<TopEventsRow> result = new List<TopEventsRow>();

            if (DetailId.HasValue)
            {
                result = DBHelper.Hidden_TopEvents(startDate, endDate, DetailId.Value, level1, 5, showMinutes, Line);
            }
            else
            {
                result = DCSDashboardDemoHelper.Hidden_TopEvents(startDate, endDate, level1, 5, showMinutes, Line);
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");

            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine(" <vc:Title Padding=\"5\" Text=\"" + (showMinutes ? "Top 5 Downtime Events" : "Top 5 Occuring Events") + " > " + (string.IsNullOrEmpty(level1) ? "ALL" : level1) + "\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.AxesY>");
            sb.AppendLine(" <vc:Axis >");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");


            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Column\" LabelEnabled=\"True\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            if (result != null)
            {
                foreach (var item in result)
                {
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\" LegendText=\"{2}\"/>", item.LevelStringWithLevelNum(), (showMinutes ? item.Minutes : item.Occurences), item.ReasonCodeId);
                }
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");
            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");


            return sb.ToString();
        }

        private string Hidden_DowntimeActualVsGoal()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            int goal = 10;
            if (!int.TryParse(Request.Form["goal"], out goal)) goal = 0;

            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];
            string type = Request.Form["type"];
            if (string.IsNullOrEmpty(type)) type = "day";
            bool isEvents = false;
            if (!bool.TryParse(Request.Form["isEvents"], out isEvents)) isEvents = true;


            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0, Line);
            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    if(DetailId.HasValue)
                        rows = DBHelper.Hidden_HoursReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.Hidden_HoursReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "day":
                    if (DetailId.HasValue)
                        rows = DBHelper.Hidden_DayReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.Hidden_DayReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    if (DetailId.HasValue)
                        rows = DBHelper.Hidden_WeekReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.Hidden_WeekReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    if (DetailId.HasValue)
                        rows = DBHelper.Hidden_MonthReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.Hidden_MonthReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    if (DetailId.HasValue)
                        rows = DBHelper.Hidden_YearReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.Hidden_YearReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default:
                    if (DetailId.HasValue)
                        rows = DBHelper.Hidden_DayReport(startDate, endDate, DetailId.Value, level1, Line);
                    else
                        rows = DCSDashboardDemoHelper.Hidden_DayReport(startDate, endDate, level1, Line);

                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;

            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"" + (isEvents ? "Downtime Actual vs. Goal" : "Occurences Actual vs. Goal") + "\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.TrendLines>");
            sb.AppendLine("     <vc:TrendLine LineColor=\"Red\" LineThickness=\"2\" Value=\"" + goal.ToString() + "\" LabelText=\"\"/>");
            sb.AppendLine("</vc:Chart.TrendLines>");

            sb.AppendLine("<vc:Chart.Legends>");
            sb.AppendLine("     <vc:Legend Enabled=\"False\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" />");
            sb.AppendLine("</vc:Chart.Legends>");

            sb.AppendLine("<vc:Chart.AxesY>");
            if (rows.Max(o => (isEvents ? o.Minutes : o.Occurences)) < goal)
            {
                sb.AppendLine(" <vc:Axis AxisMaximum=\"" + (goal + Convert.ToInt32(goal * 0.3)).ToString() + "\">");
            }
            else
            {
                sb.AppendLine(" <vc:Axis >");
            }
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid InterlacedColor=\"White\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            sb.AppendLine("     <vc:Axis.Grids>");
            sb.AppendLine("         <vc:ChartGrid Interval=\"1\" />");
            sb.AppendLine("     </vc:Axis.Grids>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");

            sb.AppendLine("<vc:Chart.PlotArea>");
            sb.AppendLine(" <vc:PlotArea ShadowEnabled=\"true\" CornerRadius=\"0,7,5,0\" BorderThickness=\"0.2,0.2,2,0.2\" >");
            sb.AppendLine("     <vc:PlotArea.Background>");
            sb.AppendLine("         <LinearGradientBrush EndPoint=\"1,0.5\" StartPoint=\"0,0.5\">");
            sb.AppendLine("             <GradientStop Color=\"#FFD8D8D8\"/>");
            sb.AppendLine("             <GradientStop Color=\"#9FFEFCFC\" Offset=\"1\"/>");
            sb.AppendLine("         </LinearGradientBrush>");
            sb.AppendLine("     </vc:PlotArea.Background>");
            sb.AppendLine(" </vc:PlotArea>");
            sb.AppendLine("</vc:Chart.PlotArea>");

            sb.AppendLine("<vc:Chart.Series>");
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Blue\" LabelEnabled=\"True\" LegendText=\"" + (isEvents ? "Downtime" : "Occurences") + "\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, (isEvents ? item.Minutes : item.Occurences));
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");

            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Red\"  LineThickness=\"0\" LabelEnabled=\"False\" LegendText=\"Goal\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");
            foreach (var item in rows)
            {
                sb.AppendFormat("<vc:DataPoint BorderColor=\"Red\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", "", goal);
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");

            sb.AppendLine("</vc:Chart.Series>");
            sb.AppendLine("</vc:Chart>");

            return sb.ToString();

        }

        private string Hidden_GetEventRows()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }

            string level1 = Request.Form["level1"];
            List<EventRowWithAllColumns> rows = DCSDashboardDemoHelper.Hidden_GetEventRows(startDate, endDate, level1, Line);
            var q = from o in rows
                    group o by new { o.Level1, o.Level2, o.Level3 } into g
                    select new { g.Key, Occurrences = g.Count(), Minutes = g.Sum(o => o.Minutes), Path = getPath(g.Key.Level1, g.Key.Level2, g.Key.Level3), StartDate = g.Min(o => o.EventStart).Value.ToString(@"MM\/dd\/yyyy"), EndDate = g.Max(o => o.EventStop).Value.ToString(@"MM\/dd\/yyyy") };
            return ConvertToJsonString(q.ToList().OrderByDescending(o => o.Minutes).Take(15));
        }
    }
}
