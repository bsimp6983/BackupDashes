using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DowntimeCollection_Demo;
using System.Web.Security;
using System.Data.Objects.DataClasses;
using System.Data;
/*
 * DCS Dashboard with Line Efficiency
 */

namespace DCSDemoData
{
    public partial class DCSDashBoard : BasePage//System.Web.UI.Page
    {
        string Line = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            Line = Request["line"];
            
            if (User.Identity.Name != "admin")
            {
                if (Membership.GetUser() != null)
                {
                    string line = string.Empty;

                    if (!string.IsNullOrEmpty(Request["line"]))
                        line = "?line=" + Request["line"];

                    Guid UserId = (Guid)Membership.GetUser().ProviderUserKey;

                    using (DB db = new DB())
                    {
                        UserInfo info = (from o in db.UserInfoSet
                                         where o.UserId == UserId
                                         select o).FirstOrDefault();

                        if (info != null)
                        {
                            if (info.EffChartEnabled == false)
                                Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\DCSDashboardDemo.aspx" + line);
                        }
                        else
                            Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\DCSDashboardDemo.aspx" + line);
                    }

                }
                else
                    Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\Login.aspx");
            }

            if (string.IsNullOrEmpty(Line))
                Line = "company-demo";

            Line = Line.Replace('#', ' ').Trim();

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

            DateTime? shiftTime = DiamondCrystaldashboardHelper.getUserShiftStart();
            DateTime? lightTime = shiftTime;

            if (DiamondCrystaldashboardHelper.getLightStatus(Line) == true)
            {
                lightTime = DiamondCrystaldashboardHelper.getLightTime(Line);

                if(DateTime.Now.Day == lightTime.Value.Day)
                    shiftTime = lightTime;
            }

            shiftStart.Text = "since " + (shiftTime.HasValue ? shiftTime.Value.ToString("hh:mm tt") : "6:00am");
            lblLine.Text = Line.Replace('_', ' ');
            lblLineTitle.Text = Line.Replace('_', ' ');

            string op = Request["op"];
            if (!string.IsNullOrEmpty(op))
            {
                DateTime? startDate = null;
                DateTime? endDate=null;
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
                        xml=getLossBuckets();
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
                        xml = getRedLines();//ConvertToJsonString(DiamondCrystaldashboardHelper.GetGoals());
                        break;
                    case "updateredline":
                        int update_Id,update_occ;
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

                        xml = (DiamondCrystaldashboardHelper.UpdateGoal(update_Id, startDate.Value, endDate.Value, update_downtime, update_occ) ? "1" : "Update failed,please try again.");
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
                        xml = (DiamondCrystaldashboardHelper.InsertGoal(startDate.Value,endDate.Value,add_downtime,add_occ) ? "1" : "0");
                        break;
                    case "deleteredline":
                        int delete_Id;
                        int.TryParse(Request["id"], out delete_Id);

                        xml = (DiamondCrystaldashboardHelper.DeleteGoal(delete_Id) ? "1" : "Update failed,please try again.");
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
                    case "getLeft12HoursCountCase":
                        xml = getLeft12HoursCountCase();
                        break;
                    case "getEfficiencyChartSetting":
                        xml = getEfficiencyChartSetting();
                        break;
                    case "saveEfficiencyChartSetting":
                        xml = saveEfficiencyChartSetting();
                        break;
                    case "getEfficiency":
                        xml = getEfficiency();
                        break;
                }

                Response.Write(xml);
                Response.End();
            }
        }

        private string getRedLines()
        {
           List<ClientGoalRow> list = DiamondCrystaldashboardHelper.GetGoals();

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
            DateTime? endDate=null;
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

            List<DowntimeCollection_Demo.LossBucketsRow> result = DCSDashboardDemoHelper.LossBuckets(startDate, endDate, Line, true).Take(8).ToList();
            //List<LossBucketsRow> result = DiamondCrystaldashboardHelper.LossBuckets(startDate, endDate, true);
            StringBuilder sb=new StringBuilder();
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
            List<DowntimeCollection_Demo.LossBucketsRow> result = DCSDashboardDemoHelper.LossBuckets(startDate, endDate, Line, false);
            //List<LossBucketsRow> result = DiamondCrystaldashboardHelper.LossBuckets(startDate, endDate,false);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Loss Buckets\" FontSize=\"14\" Padding=\"5\"/>");
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
            List<DowntimeCollection_Demo.TopEventsRow> result = DCSDashboardDemoHelper.TopEvents(startDate, endDate, level1, 5, Line, showMinutes);
            //List<TopEventsRow> result = DiamondCrystaldashboardHelper.TopEvents(startDate, endDate, level1, 5, showMinutes);


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine(" <vc:Title "+(isHome?"Cursor=\"Hand\" FontColor=\"Blue\" TextDecorations=\"Underline\"":"")+" Padding=\"5\" Text=\"" + (showMinutes ? "Top 5 Downtime Events" : "Top 5 Occuring Events") + " > " + (string.IsNullOrEmpty(level1)?"ALL":level1) + "\" FontSize=\"14\"/>");
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
                    sb.AppendFormat("<vc:DataPoint AxisXLabel=\"{0}\" YValue=\"{1}\" LegendText=\"{2}\"/>", item.LevelString(), (showMinutes ? item.Minutes : item.Occurences),item.ReasonCodeId);
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


            //GoalReportRow grr = null;
            DowntimeCollection_Demo.GoalReportRow grr = null;

            grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0, Line);

            /*
            if (!string.IsNullOrEmpty(level1))
                grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0, Line);
                //grr = DiamondCrystaldashboardHelper.GetGoals(startDate, endDate, level1, 0);
            else
                grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0, Line);
                //grr = DiamondCrystaldashboardHelper.GetGoals(startDate.Value, endDate.Value);
            */
            /*
            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DiamondCrystaldashboardHelper.HoursReport_DateRange(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Hour) : grr.Occurences.Hour);
                    break;
                case "day":
                    rows = DiamondCrystaldashboardHelper.DayReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    rows = DiamondCrystaldashboardHelper.WeekReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    rows = DiamondCrystaldashboardHelper.MonthReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    rows = DiamondCrystaldashboardHelper.YearReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default :
                    rows = DiamondCrystaldashboardHelper.DayReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;

            }
            */

            List<DowntimeCollection_Demo.SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DCSDashboardDemoHelper.HoursReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Hour) : grr.Occurences.Hour);
                    break;
                case "day":
                    rows = DCSDashboardDemoHelper.DayReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.WeekReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.MonthReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.YearReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default:
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
            List<DowntimeCollection_Demo.SpecialReportRow> result = DCSDashboardDemoHelper.TopEventsGroupByLevel3(startDate, endDate, level1, 5, true, Line);
            //List<SpecialReportRow> result = DiamondCrystaldashboardHelper.TopEventsGroupByLevel3(startDate, endDate, level1,5,true);
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

            DowntimeCollection_Demo.GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id, Line);
            /*
            if (!string.IsNullOrEmpty(level1))
                grr = DiamondCrystaldashboardHelper.GetGoals(startDate, endDate, level1, 0);
            else
                grr = DiamondCrystaldashboardHelper.GetGoals(startDate.Value, endDate.Value);
            */

            /*
            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DiamondCrystaldashboardHelper.HoursReport_Level3_DateRange(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Hour);
                    break;
                case "day":
                    rows = DiamondCrystaldashboardHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;
                case "week":
                    rows = DiamondCrystaldashboardHelper.WeekReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Week);
                    break;
                case "month":
                    rows = DiamondCrystaldashboardHelper.MonthReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Month);
                    break;
                case "year":
                    rows = DiamondCrystaldashboardHelper.YearReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Year);
                    break;
                default:
                    rows = DiamondCrystaldashboardHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;

            }
            */

            List<DowntimeCollection_Demo.SpecialReportRow> rows = null;
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
        /*
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
            return ConvertToJsonString(DiamondCrystaldashboardHelper.Comments(startDate, endDate, level3_id, level1, Line));
        }
        */

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

            foreach (CommentRow row in DiamondCrystaldashboardHelper.Comments(startDate, endDate, level3_id, level1, Line))
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
            List<DowntimeCollection_Demo.SpecialReportRow> result = DCSDashboardDemoHelper.TopEventsGroupByLevel3(startDate, endDate, level1, 5, false, Line);
            //List<SpecialReportRow> result = DiamondCrystaldashboardHelper.TopEventsGroupByLevel3(startDate, endDate, level1, 5,false);
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

            /*
            GoalReportRow grr = null;

            if (level3_id > 0)
                grr = DiamondCrystaldashboardHelper.GetGoals(startDate, endDate, level1, level3_id);
            else
                grr = DiamondCrystaldashboardHelper.GetGoals(startDate.Value, endDate.Value);

            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DiamondCrystaldashboardHelper.HoursReport_Level3_DateRange(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Hour);
                    break;
                case "day":
                    rows = DiamondCrystaldashboardHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;
                case "week":
                    rows = DiamondCrystaldashboardHelper.WeekReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Week);
                    break;
                case "month":
                    rows = DiamondCrystaldashboardHelper.MonthReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Month);
                    break;
                case "year":
                    rows = DiamondCrystaldashboardHelper.YearReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Year);
                    break;
                default:
                    rows = DiamondCrystaldashboardHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;

            }
            */

            DowntimeCollection_Demo.GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id, Line);

            List<DowntimeCollection_Demo.SpecialReportRow> rows = null;
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
            /*
            if (rows != null)
            {
                rows=rows.OrderByDescending(o => o.Minutes).ToList();
            }
            */

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            //sb.AppendLine("<vc:Title Text=\"Occuring Actual vs. Goal > " + level3 + "\" FontSize=\"14\"/>");
            sb.AppendLine("<vc:Title Text=\"Occuring Actual vs. Goal\" FontSize=\"14\" Padding=\"5\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.TrendLines>");
            sb.AppendLine("     <vc:TrendLine LineColor=\"Red\" LineThickness=\"2\" Value=\"" + goal.ToString() + "\" LabelText=\"\"/>");
            sb.AppendLine("</vc:Chart.TrendLines>");

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

            string tmp = string.Empty;
            foreach (var item in rows)
            {
                if(tmp != item.Title)
                    sb.AppendFormat("<vc:DataPoint BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, item.Minutes);

                tmp = item.Title;
            }
            sb.AppendLine("</vc:DataSeries.DataPoints>");
            sb.AppendLine("</vc:DataSeries>");


            //occurences
            sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" Color=\"Green\" LabelEnabled=\"True\" LegendText=\"Occuring\">");
            sb.AppendLine("<vc:DataSeries.DataPoints>");

            foreach (var item in rows)
            {
                if(tmp != item.Title)
                    sb.AppendFormat("<vc:DataPoint BorderColor=\"Green\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Title, item.Occurences);
                tmp = item.Title;
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


            DowntimeCollection_Demo.GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id, Line);
            Dictionary<string, List<DowntimeCollection_Demo.DowntimeHistoryRow>> rows = DCSDashboardDemoHelper.getDowntimesHistory(startDate, endDate, level3_id, type, Line);
            /*

            GoalReportRow grr = DiamondCrystaldashboardHelper.GetGoals(startDate, endDate, level1, level3_id);
            Dictionary<string, List<DowntimeHistoryRow>> rows = DiamondCrystaldashboardHelper.getDowntimesHistory(startDate, endDate, level3_id, type);
            */

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
                sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" LabelEnabled=\"True\" LegendText=\""+row.Key+"\">");
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


            List<DowntimeCollection_Demo.AllOccurrenceHistoryRow> rows = DCSDashboardDemoHelper.getOccurrenceHistory(startDate, endDate, level3_id, type, Line);//ReportHelper.MonthOccurrenceHistory(startDate, endDate, level3_id);

            //List<AllOccurrenceHistoryRow> rows = DiamondCrystaldashboardHelper.getOccurrenceHistory(startDate, endDate, level3_id, type);//ReportHelper.MonthOccurrenceHistory(startDate, endDate, level3_id);

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
                sb.AppendLine("<vc:DataSeries RenderAs=\"Line\" LabelEnabled=\"True\" LegendText=\"" + item .Level3+ "\">");
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

        /*
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
            List<DowntimeCollection_Demo.EventRowWithAllColumns> rows = DCSDashboardDemoHelper.GetEventRows(startDate, endDate, level1, Line);
            //List<EventRowWithAllColumns> rows = DiamondCrystaldashboardHelper.GetEventRows(startDate, endDate, level1);
            var q = from o in rows
                    group o by new { o.Level1, o.Level2, o.Level3 } into g
                    select new { g.Key, Occurrences = g.Count(), Minutes = g.Sum(o => o.Minutes), Path = getPath(g.Key.Level1, g.Key.Level2, g.Key.Level3), StartDate = g.Min(o => o.EventStart).Value.ToString(@"MM\/dd\/yyyy"), EndDate = g.Max(o => o.EventStop).Value.ToString(@"MM\/dd\/yyyy") };
            return ConvertToJsonString(q.ToList().OrderByDescending(o=>o.Minutes).Take(15));
        }
         */


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

            List<DowntimeCollection_Demo.EventRowWithAllColumns> rows = DCSDashboardDemoHelper.GetEventRows(startDate, endDate, level1, Line);

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
                    Cumulative = (cumulative * 100).ToString("#.##") + "%"
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
            List<DowntimeCollection_Demo.TopEventsRow> result = DCSDashboardDemoHelper.Hidden_TopEvents(startDate, endDate, level1, 5, showMinutes, Line);
            //List<TopEventsRow> result = DiamondCrystaldashboardHelper.Hidden_TopEvents(startDate, endDate, level1, 5, showMinutes);


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


            /*
            GoalReportRow grr = null;

            if (!string.IsNullOrEmpty(level1))
                grr = DiamondCrystaldashboardHelper.GetGoals(startDate, endDate, level1, 0);
            else
                grr = DiamondCrystaldashboardHelper.GetGoals(startDate.Value, endDate.Value);

            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DiamondCrystaldashboardHelper.Hidden_HoursReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Hour) : grr.Occurences.Hour);
                    break;
                case "day":
                    rows = DiamondCrystaldashboardHelper.Hidden_DayReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    rows = DiamondCrystaldashboardHelper.Hidden_WeekReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    rows = DiamondCrystaldashboardHelper.Hidden_MonthReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    rows = DiamondCrystaldashboardHelper.Hidden_YearReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default:
                    rows = DiamondCrystaldashboardHelper.Hidden_DayReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;

            }
            */

            DowntimeCollection_Demo.GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0, Line);
            List<DowntimeCollection_Demo.SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DCSDashboardDemoHelper.Hidden_HoursReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "day":
                    rows = DCSDashboardDemoHelper.Hidden_DayReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.Hidden_WeekReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.Hidden_MonthReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.Hidden_YearReport(startDate, endDate, level1, Line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default:
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
            List<DowntimeCollection_Demo.EventRowWithAllColumns> rows = DCSDashboardDemoHelper.Hidden_GetEventRows(startDate, endDate, level1, Line);
            //List<EventRowWithAllColumns> rows = DiamondCrystaldashboardHelper.Hidden_GetEventRows(startDate, endDate, level1);
            var q = from o in rows
                    group o by new { o.Level1, o.Level2, o.Level3 } into g
                    select new { g.Key, Occurrences = g.Count(), Minutes = g.Sum(o => o.Minutes), Path = getPath(g.Key.Level1, g.Key.Level2, g.Key.Level3), StartDate = g.Min(o => o.EventStart).Value.ToString(@"MM\/dd\/yyyy"), EndDate = g.Max(o => o.EventStop).Value.ToString(@"MM\/dd\/yyyy") };
            return ConvertToJsonString(q.ToList().OrderByDescending(o => o.Minutes).Take(15));
        }

        public string createCustomAxisLabels()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Axis.AxisLabels>");
            sb.AppendLine(" <vc:AxisLabels Enabled=\"false\"></vc:AxisLabels>");
            sb.AppendLine("</vc:Axis.AxisLabels>");
            sb.AppendLine("<vc:Axis.CustomAxisLabels>");
            sb.AppendLine("     <vc:CustomAxisLabels>");
            sb.AppendLine("         <vc:CustomAxisLabels.Labels>");

            Dictionary<int, decimal> values = GetCustomAxisValues();


            Dictionary<int, string> labels = new Dictionary<int, string>();

            for (int x = 0; x < values.Count; x++)
            {
                labels.Add(values.Keys.ElementAt(x), (values.Values.ElementAt(x) * 100).ToString("0.##") + "%");
            }


            foreach (var label in labels)
            {
                sb.AppendLine("             <vc:CustomAxisLabel From=\"" + label.Key.ToString() + "\" To=\"" + label.Key.ToString() + "\" Text=\"" + label.Value + "\"></vc:CustomAxisLabel>");
            }
            sb.AppendLine("         </vc:CustomAxisLabels.Labels>");
            sb.AppendLine("     </vc:CustomAxisLabels>");
            sb.AppendLine("</vc:Axis.CustomAxisLabels>");
            return sb.ToString();
        }


        private Dictionary<int, decimal> GetCustomAxisValues()
        {
            Dictionary<int, decimal> values = new Dictionary<int, decimal>();

            //To&From, Percent
            /*
            values.Add(100, 1);
            values.Add(90, .99m);
            values.Add(80, .98m);
            values.Add(65, .95m);
            values.Add(50, .90m);
            values.Add(30, .85m);
            values.Add(10, .75m);
            values.Add(-15, .65m);
            values.Add(-50, .50m);
            values.Add(-105, .30m);
            values.Add(-180, .0m);
            
            values.Add(100, 1);
            values.Add(75, .99m);
            values.Add(50, .98m);
            values.Add(25, .95m);
            values.Add(0, .90m);
            values.Add(-20, .85m);
            values.Add(-40, .75m);
            values.Add(-65, .65m);
            values.Add(-100, .50m);
            values.Add(-155, .30m);
            values.Add(-230, .0m);
            */

            values.Add(100, 1);
            values.Add(75, .99m);
            values.Add(50, .98m);
            values.Add(25, .95m);
            values.Add(0, .90m);
            values.Add(-25, .85m);
            values.Add(-50, .75m);
            values.Add(-75, .65m);
            values.Add(-100, .50m);
            values.Add(-125, .30m);
            values.Add(-150, .0m);

            return values;
        }

        private int Transform(decimal v)
        {
            Dictionary<int, decimal> values = GetCustomAxisValues();

            decimal from = (v > 100 ? v : 100);//From Value (Between 0 - 100%)
            int fromKey = 100;//From Key (Between all the keys)

            for (int x = 0; x < values.Count; x++)
            {
                int toKey = values.Keys.ElementAt(x);
                decimal to = values.Values.ElementAt(x) * 100;//Percent

                if (v < from && v > to)
                {
                    int toDiff = Convert.ToInt32(from - v);

                    decimal multiplier = (fromKey - toKey) / (from - to);

                    int resultKey = Convert.ToInt32(toDiff * multiplier);

                    int resultValue = fromKey - resultKey;

                    return resultValue;
                }
                else if (v == from)
                    return fromKey;
                else if (v == to)
                    return toKey;

                from = to;
                fromKey = toKey;

            }

            return -1;
        }

        public decimal Transform(decimal origValue, decimal origMax)
        {
            Decimal ratio = origValue / origMax;
            if (ratio >= 0 && ratio < 0.3m)
            {
                return origValue;
            }
            else if (ratio >= 0.3m && ratio < 0.5m)
            {
                return origValue;
            }
            else if (ratio >= 0.5m && ratio < 0.65m)//只有15%的间隔，所以加15%
            {
                return origValue + origMax * 0.15m;
            }
            else if (ratio >= 0.65m && ratio < 0.75m)//只有10%的间隔，所以加20%，同时加上前面的15%
            {
                return origValue + origMax * 0.35m;
            }
            else if (ratio >= 0.75m && ratio < 0.85m)//只有10%的间隔，所以加20%，同时加上前面的所有加数35%
            {
                return origValue + origMax * 0.55m;
            }
            else if (ratio >= 0.85m && ratio < 0.9m)//只有5%的间隔，所以加25%，同时加上前面的所有加数55%
            {
                return origValue + origMax * 0.80m;
            }
            else if (ratio >= 0.9m && ratio < 0.95m)//只有5%的间隔，所以加25%，同时加上前面的所有加数80%
            {
                return origValue + origMax * 1.05m;
            }
            else if (ratio >= 0.95m && ratio < 0.98m)//只有3%的间隔，所以加28%，同时加上前面的所有加数105%
            {
                return origValue + origMax * 1.33m;
            }
            else if (ratio >= 0.98m && ratio < 0.99m)//只有1%的间隔，所以加29%，同时加上前面的所有加数133%
            {
                return origValue + origMax * 1.62m;
            }
            else if (ratio >= 0.99m && ratio < 1m)//只有1%的间隔，所以加29%，同时加上前面的所有加数162%
            {
                return origValue + origMax * 1.91m;
            }
            else if (ratio >= 1m)//只有1%的间隔，所以加29%，同时加上前面的所有加数191%
            {
                return origValue + origMax * 2.2m;
            }
            else
            {
                throw new Exception(ratio.ToString("0.##"));
            }
            //if (ratio >= 0.98m && ratio < 0.99m)
            //{
            //    return origValue + origMax * (3m / 100m);
            //}
            //else if (ratio >= 0.99m && ratio < 1m)
            //{
            //    return origValue + origMax * (3m / 100m) * 2;//前面有一个98%
            //}
            //else if (ratio >= 1m)
            //{
            //    return origValue + origMax * (3m / 100m) * 3;//前面有一个98%,99%
            //}
            //else
            //{
            //    return origValue;
            //}
        }

        public decimal getCustomAxisLabelValue(decimal pgoal, decimal v, decimal max)
        {
            v = Transform(v, max);
            pgoal = Transform(pgoal, max);
            return v - pgoal;
        }
        public string createCustomAxisLabels(decimal div)
        {
            StringBuilder sb = new StringBuilder();
            decimal pgoal = DiamondCrystaldashboardHelper.CountCaseGoals.Mark / 100 * div;//红线量
            sb.AppendLine("<vc:Axis.AxisLabels>");
            sb.AppendLine(" <vc:AxisLabels Enabled=\"false\"></vc:AxisLabels>");
            sb.AppendLine("</vc:Axis.AxisLabels>");
            sb.AppendLine("<vc:Axis.CustomAxisLabels>");
            sb.AppendLine("     <vc:CustomAxisLabels>");
            sb.AppendLine("         <vc:CustomAxisLabels.Labels>");
            Dictionary<string, decimal> labels = new Dictionary<string, decimal>();
            {
                labels.Add("100%", div);
                labels.Add("99%", div / 100 * 99);
                labels.Add("98%", div / 100 * 98);
                labels.Add("95%", div / 100 * 95);
                labels.Add("90%", div / 100 * 90);
                labels.Add("85%", div / 100 * 85);
                labels.Add("75%", div / 100 * 75);
                labels.Add("65%", div / 100 * 65);
                labels.Add("50%", div / 100 * 50);
                labels.Add("30%", div / 100 * 30);
                labels.Add("0%", 0);
            }
            foreach (var label in labels)
            {
                sb.AppendLine("             <vc:CustomAxisLabel From=\"" + getCustomAxisLabelValue(pgoal, label.Value, div).ToString("0.##") + "\" To=\"" + getCustomAxisLabelValue(pgoal, label.Value, div).ToString("0.##") + "\" Text=\"" + label.Key + "\"></vc:CustomAxisLabel>");
            }
            sb.AppendLine("         </vc:CustomAxisLabels.Labels>");
            sb.AppendLine("     </vc:CustomAxisLabels>");
            sb.AppendLine("</vc:Axis.CustomAxisLabels>");
            return sb.ToString();
        }

        private string GetDataPointWidthAttr(int DataPointCount)
        {
            if (DataPointCount < 12)
            {
                return " DataPointWidth=\"8\"";
            }
            else
            {
                return string.Empty;
            }
        }

        private string getLeft12HoursCountCase()
        {
            int width = 500;
            int height = 300;
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);

            bool autoCalculate = false;

            bool.TryParse(Request.Form["ac"], out autoCalculate);


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

            DateTime clientTime = DateTime.Parse(Request.Form["clientTime"]);
            //clientTime = DateTime.Now.AddHours(-16); //test
            string type = Request.Form["type"];

            //startDate = DiamondCrystaldashboardHelper.convertToTimeZone(startDate.Value, Line);
            //endDate = DiamondCrystaldashboardHelper.convertToTimeZone(endDate.Value, Line);
            //clientTime = DiamondCrystaldashboardHelper.convertToTimeZone(clientTime, Line);

            decimal div = DiamondCrystaldashboardHelper.CountCaseGoals.MinuteCases * 60;//计算比例基数（默认是每小时）


            string renderAs = Request["showas"];
            if (!string.Equals(renderAs, "line", StringComparison.CurrentCultureIgnoreCase) &&
            !string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                renderAs = "Line";
            }

            List<CCEfficiency> caseCounts = new List<CCEfficiency>();
            Dictionary<string, decimal> rows = new Dictionary<string, decimal>();

            List<LineEfficiency> efficiencies = DiamondCrystaldashboardHelper.getLineEfficiencies(clientTime.AddHours(-12), clientTime, Line).OrderBy(o => o.Time).ToList();

            
            decimal axisMax = 100m;
            decimal axisMin = 0m;

            if (efficiencies.Count > 0)
            {
                if (efficiencies.Max(o => o.Value) > 1m)
                {
                    axisMax = efficiencies.Max(o => o.Value);

                    if (axisMax > 3m)//Values are in decimal format
                        axisMax = 300m;
                    else
                        axisMax *= 100;
                }
            }

            decimal columnGoal = getCustomAxisLabelValue(80, 80, div);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"true\" AnimationEnabled=\"true\" View3D=\"False\"  ShadowEnabled=\"true\" CornerRadius=\"12\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\" " + GetDataPointWidthAttr(rows.Count()) + ">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Efficiency Chart\" FontSize=\"14\"/>");
            sb.AppendLine("</vc:Chart.Titles>");

            sb.AppendLine("<vc:Chart.TrendLines>");
            if (string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine("     <vc:TrendLine LineColor=\"Red\" LineThickness=\"2\" Value=\"" + columnGoal.ToString("0.##") + "\" LabelText=\"\"/>");
            }
            else
            {
                sb.AppendLine("     <vc:TrendLine LineColor=\"Red\" LineThickness=\"2\" Value=\"" + (80.00).ToString() + "\" LabelText=\"\"/>");
            }
            sb.AppendLine("</vc:Chart.TrendLines>");

            sb.AppendLine("<vc:Chart.Legends>");
            sb.AppendLine("     <vc:Legend Enabled=\"False\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" />");
            sb.AppendLine("</vc:Chart.Legends>");

            sb.AppendLine("<vc:Chart.AxesY>");
            if (string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine(" <vc:Axis AxisMaximum=\"" + Transform(axisMax).ToString("f0") + "\" AxisMinimum=\"" + Transform(0) + "\" LineThickness=\"0\" >");
            }
            else
            {
                sb.AppendLine(" <vc:Axis AxisMaximum=\"" + axisMax.ToString("f0") + "\" AxisMinimum=\"" + axisMin.ToString("f0") + "\"  Suffix=\"%\" Interval=\"10\" LineThickness=\"0\">");
            }

            if (!string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine("     <vc:Axis.Grids>");
                sb.AppendLine("         <vc:ChartGrid Enabled=\"True\" />");
                sb.AppendLine("     </vc:Axis.Grids>");
            }

            if (string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine(createCustomAxisLabels());
            }
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesY>");

            sb.AppendLine("<vc:Chart.AxesX>");
            if (string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine(" <vc:Axis LineThickness=\"0\">");
            }
            else
            {
                sb.AppendLine(" <vc:Axis LineThickness=\"0\" Suffix=\"%\">");
            }
            if (!string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine("     <vc:Axis.Grids>");
                sb.AppendLine("         <vc:ChartGrid Interval=\"1\" Enabled=\"False\" />");
                sb.AppendLine("     </vc:Axis.Grids>");
            }

            sb.AppendLine("     <vc:Axis.AxisLabels>");
            sb.AppendLine("         <vc:AxisLabels Angle=\"-50\"/>");//-60
            sb.AppendLine("     </vc:Axis.AxisLabels>");
            sb.AppendLine(" </vc:Axis>");
            sb.AppendLine("</vc:Chart.AxesX>");


            sb.AppendLine("<vc:Chart.Series>");
            if (string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
            {
                sb.AppendLine("<vc:DataSeries RenderAs=\"" + renderAs + "\" Color=\"Blue\" LabelEnabled=\"False\" LegendText=\"CaseCount\">");
            }
            else
            {
                sb.AppendLine("<vc:DataSeries RenderAs=\"" + renderAs + "\" Color=\"Blue\" LabelEnabled=\"True\" LegendText=\"CaseCount\">");
            }
            sb.AppendLine("<vc:DataSeries.DataPoints>");

            using (DB db = new DB())
            {
                for (int x = 0; x < efficiencies.Count; x++)
                {
                    LineEfficiency eff = efficiencies.ElementAt(x);

                    decimal value = eff.Value * 100;//((!MillardDashboardHelper.getLightStatus()) ? 0 : item.Value);

                    decimal customLabel = Transform(value);

                    string tooltip = value.ToString();

                    if (!string.IsNullOrEmpty(eff.SKU))
                        tooltip = eff.Cases + "/" + eff.Estimate + " [" + eff.SKU + "]";

                    //sb.AppendFormat("<vc:DataPoint Color=\"" + getPointColor((decimal)item.Value / (decimal)div * (decimal)100) + "\" BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\"/>", item.Key, ((decimal)item.Value / (decimal)div * (decimal)100).ToString("f2"));
                    if (string.Equals(renderAs, "column", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sb.AppendFormat("<vc:DataPoint Color=\"" + getPointColor(value, 90) + "\" BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\" ToolTipText=\"" + eff.Cases + "\"/>", eff.Time.ToString("h tt"), customLabel);
                    }
                    else
                    {
                        sb.AppendFormat("<vc:DataPoint Color=\"" + getPointColor(value, 80) + "\" BorderColor=\"Blue\" AxisXLabel=\"{0}\" YValue=\"{1}\" ToolTipText=\"" + tooltip + "\"/>", eff.Time.ToString("h tt"), value.ToString("f2"));
                    }
                }
                sb.AppendLine("</vc:DataSeries.DataPoints>");
                sb.AppendLine("</vc:DataSeries>");

                sb.AppendLine("</vc:Chart.Series>");
                sb.AppendLine("</vc:Chart>");

                return sb.ToString();
            }
        }

        private string getPointColor(decimal v, decimal mark)
        {
            if (v < mark)
            {
                return "Red";
            }
            else
            {
                return "Green";
            }
        }

        public static Throughput GetThroughputFromReference(EntityReference reference)
        {
            EntityKeyMember[] keys = reference.EntityKey.EntityKeyValues;
            int id = 0;

            if (keys[0].Key.ToLower() == "id")
                id = (int)keys[0].Value;

            return DCSDashboardDemoHelper.GetThroughPut(id);
        }

        private int getLatestThroughput(string line)
        {
            ThroughputHistory th = DCSDashboardDemoHelper.getLatestThroughPutHistory(line);

            if (th != null)
            {
                Throughput tp = th.Throughput;

                if (tp == null)
                    tp = GetThroughputFromReference(th.ThroughputReference);

                if (tp != null)
                    return tp.PerHour;

            }

            return -1;
        }

        private int getThroughput(DateTime date, string line)
        {
            ThroughputHistory th = DCSDashboardDemoHelper.getThroughPutHistory(date, line);

            if (th != null)
            {
                Throughput tp = th.Throughput;

                if (tp == null)
                    tp = GetThroughputFromReference(th.ThroughputReference);

                if (tp != null)
                    return tp.PerHour;

            }

            return -1;
        }

        private int getEstimatedCount(DateTime date, string line)
        {
            int results = getThroughput(date, line);

            if (results > -1)
                return results;
            else
            {

                Guid UserId = (Membership.GetUser() != null ? (Guid)Membership.GetUser().ProviderUserKey : Guid.Empty);

                using (DB db = new DB())
                {
                    UserInfo q = (from o in db.UserInfoSet
                                  where o.UserId == UserId
                                  select o).FirstOrDefault();




                    if (q != null)
                        results = q.EstimatedOutput;
                }
            }

            return results;
        }

        private int getEstimatedCount(string line)
        {
            int results = getLatestThroughput(line);

            if (results > -1)
                return results;
            else
            {

                Guid UserId = (Membership.GetUser() != null ? (Guid)Membership.GetUser().ProviderUserKey : Guid.Empty);

                using (DB db = new DB())
                {
                    UserInfo q = (from o in db.UserInfoSet
                                  where o.UserId == UserId
                                  select o).FirstOrDefault();




                    if (q != null)
                        results = q.EstimatedOutput;
                }
            }

            return results;
        }

        private string getEfficiency()
        {
            DateTime clientTime = DateTime.Parse(Request.Form["clientTime"]);

            clientTime = DiamondCrystaldashboardHelper.convertToTimeZone(clientTime, Line);

            decimal numerator;
            decimal denominator;
            //decimal re=DiamondCrystaldashboardHelper.getRunningEfficiency(clientTime,out numerator,out denominator);
            decimal re = DiamondCrystaldashboardHelper.getRunningLineEfficiency(clientTime, out numerator, out denominator, Line);//DiamondCrystaldashboardHelper.getRunningEfficiencyFromEstimate(clientTime, out numerator, out denominator, Line);
            decimal ye = DiamondCrystaldashboardHelper.getYesterdayLineEfficiency(Line);
            
            //if (re < 1)
                re = re * 100;

            //if (ye < 1)
                ye = ye * 100;
            
            var obj = new {
                RunningEfficiency = string.Format("{0:0.##}%", re),
                YesterdayEffiency = string.Format("{0:0.##}%", ye),
                Numerator=numerator,
                Denominator=denominator,
                Mask = DiamondCrystaldashboardHelper.CountCaseGoals.Mark
            };
            return ConvertToJsonString(obj);
        }

        private string getEfficiencyChartSetting()
        {
            object s = new
            {
                Mark = DiamondCrystaldashboardHelper.CountCaseGoals.Mark,
                MaxCaseCount = DiamondCrystaldashboardHelper.CountCaseGoals.MaxCaseCount
            };
            return ConvertToJsonString(s);
        }

        private string saveEfficiencyChartSetting()
        {
            int mark = 0;
            int maxCaseCount = 0;
            if (!int.TryParse(Request.Form["Mark"], out mark))
            {
                return "false";
            }

            if (!int.TryParse(Request.Form["MaxCaseCount"], out maxCaseCount))
            {
                return "false";
            }

            DiamondCrystaldashboardHelper.CountCaseGoals.Mark = mark;
            DiamondCrystaldashboardHelper.CountCaseGoals.MaxCaseCount = maxCaseCount;
            return "true";
        }
    }
}
