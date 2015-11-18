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

namespace DowntimeCollection_Demo
{
    public partial class TvDashboardDemo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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

                string line = Request.Form["line"];

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
                        xml = DCSDashboardDemoHelper.TotalDowntime(startDate, endDate, line).ToString();
                        break;
                    case "RedLineList":
                        xml = ConvertToJsonString(DCSDashboardDemoHelper.GetGoals());
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
                        xml = (DCSDashboardDemoHelper.InsertGoal(startDate.Value, endDate.Value, add_downtime, add_occ) ? "1" : "0");
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
                    //case "getLineStatus":
                    //    xml = GetLineStatus();
                    //    break;
                    case "getSavings":
                        xml = DCSDashboardDemoHelper.GetCurrentSavings();
                        break;
                }

                Response.Write(xml);
                Response.End();
            }
        }

        //private string GetLineStatus()
        //{
        //    List<LineStats> lineList = DCSDashboardDemoHelper.getLineStats();

        //    List<object> objList = new List<object>();


        //    foreach (LineStats line in lineList)
        //    {
        //        objList.Add(new
        //        {
        //            Line = line.Line,
        //            Status = line.Status,
        //            Counter = line.Counter
        //        });
        //    }

        //    return ConvertToJsonString(objList);

        //}


        private string getLossBuckets()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            int width = 500;
            int height = 300;
            string line = "1";
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            line = Request.Form["line"];

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            List<LossBucketsRow> result = DCSDashboardDemoHelper.LossBuckets(startDate, endDate, line, true);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine("<vc:Title Text=\"Loss Buckets\" Cursor=\"Hand\" FontColor=\"Blue\" TextDecorations=\"Underline\" FontSize=\"14\" Padding=\"5\"/>");
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
            List<LossBucketsRow> result = DCSDashboardDemoHelper.LossBuckets(startDate, endDate, false);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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
            string line = "5";
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            line = Request.Form["line"];

            string level1 = Request.Form["level1"];

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            List<TopEventsRow> result = DCSDashboardDemoHelper.TopEvents(startDate, endDate, level1, 5, line, showMinutes);


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");

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
            string line = "5";
            int.TryParse(Request.Form["width"], out width);
            int.TryParse(Request.Form["height"], out height);
            string level1 = Request.Form["level1"];
            string type = Request.Form["type"];
            if (string.IsNullOrEmpty(type)) type = "day";
            bool isEvents = false;
            if (!bool.TryParse(Request.Form["isEvents"], out isEvents)) isEvents = true;
            line = Request.Form["line"];


            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0, line);
            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DCSDashboardDemoHelper.HoursReport(startDate, endDate, level1, line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Hour) : grr.Occurences.Hour);
                    break;
                case "day":
                    rows = DCSDashboardDemoHelper.DayReport(startDate, endDate, level1, line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.WeekReport(startDate, endDate, level1, line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.MonthReport(startDate, endDate, level1, line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.YearReport(startDate, endDate, level1, line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default:
                    rows = DCSDashboardDemoHelper.DayReport(startDate, endDate, level1, line);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;

            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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
            List<SpecialReportRow> result = DCSDashboardDemoHelper.TopEventsGroupByLevel3(startDate, endDate, level1, 5, true);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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

            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id);

            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DCSDashboardDemoHelper.HoursReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Hour);
                    break;
                case "day":
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.WeekReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.MonthReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.YearReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Year);
                    break;
                default:
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;

            }

            if (rows != null)
            {
                rows.OrderByDescending(o => o.Minutes);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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
            string line = Request.Form["line"];
            int level3_id = 0;
            int.TryParse(Request.Form["level3_id"], out level3_id);

            List<CommentRow> list = DCSDashboardDemoHelper.Comments(startDate, endDate, level3_id, level1, line);

            List<object> oList = new List<object>();

            foreach (CommentRow row in list)
            {
                oList.Add(new
                {
                    EventStartTimeStr = row.EventStartTimeStr,
                    Comment = row.Comment
                });
            }

            return ConvertToJsonString(oList);
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
            List<SpecialReportRow> result = DCSDashboardDemoHelper.TopEventsGroupByLevel3(startDate, endDate, level1, 5, false);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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

            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id);

            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "hours":
                    rows = DCSDashboardDemoHelper.HoursReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Hour);
                    break;
                case "day":
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.WeekReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.MonthReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.YearReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Year);
                    break;
                default:
                    rows = DCSDashboardDemoHelper.DayReport_Level3(startDate, endDate, level3_id);
                    goal = Convert.ToInt32(grr.Downtimes.Day);
                    break;

            }

            if (rows != null)
            {
                rows = rows.OrderByDescending(o => o.Minutes).ToList();
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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



            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, level3_id);
            Dictionary<string, List<DowntimeHistoryRow>> rows = DCSDashboardDemoHelper.getDowntimesHistory(startDate, endDate, level3_id, type);


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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


            List<AllOccurrenceHistoryRow> rows = DCSDashboardDemoHelper.getOccurrenceHistory(startDate, endDate, level3_id, type);//ReportHelper.MonthOccurrenceHistory(startDate, endDate, level3_id);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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
            string line = Request.Form["line"];

            List<EventRowWithAllColumns> rows = DCSDashboardDemoHelper.GetEventRowsByLine(startDate, endDate, level1, line);

            var q = from o in rows
                    group o by new { o.Level1, o.Level2, o.Level3 } into g
                    select new { g.Key, Occurrences = g.Count(), Minutes = g.Sum(o => o.Minutes), Path = getPath(g.Key.Level1, g.Key.Level2, g.Key.Level3), StartDate = g.Min(o => o.EventStart).Value.ToString(@"MM\/dd\/yyyy"), EndDate = g.Max(o => o.EventStop).Value.ToString(@"MM\/dd\/yyyy") };
            return ConvertToJsonString(q.ToList().OrderByDescending(o => o.Minutes).Take(15));
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
            string line = Request.Form["line"];
            string level1 = Request.Form["level1"];

            if (DateTime.TryParse(Request.Form["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request.Form["enddate"], out d))
            {
                endDate = d;
            }
            List<TopEventsRow> result = DCSDashboardDemoHelper.Hidden_TopEvents(startDate, endDate, level1, 5, showMinutes, line);


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");

            sb.AppendLine("<vc:Chart.Titles>");
            sb.AppendLine(" <vc:Title Padding=\"5\" Cursor=\"Hand\" FontColor=\"Blue\" TextDecorations=\"Underline\" Text=\"" + (showMinutes ? "Top 5 Downtime Events" : "Top 5 Occuring Events") + " > " + (string.IsNullOrEmpty(level1) ? "ALL" : level1) + "\" FontSize=\"14\"/>");
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


            GoalReportRow grr = DCSDashboardDemoHelper.GetGoals(startDate, endDate, level1, 0);
            List<SpecialReportRow> rows = null;
            switch (type.ToLower())
            {
                case "day":
                    rows = DCSDashboardDemoHelper.Hidden_DayReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;
                case "week":
                    rows = DCSDashboardDemoHelper.Hidden_WeekReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Week) : grr.Occurences.Week);
                    break;
                case "month":
                    rows = DCSDashboardDemoHelper.Hidden_MonthReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Month) : grr.Occurences.Month);
                    break;
                case "year":
                    rows = DCSDashboardDemoHelper.Hidden_YearReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Year) : grr.Occurences.Year);
                    break;
                default:
                    rows = DCSDashboardDemoHelper.Hidden_DayReport(startDate, endDate, level1);
                    goal = (isEvents ? Convert.ToInt32(grr.Downtimes.Day) : grr.Occurences.Day);
                    break;

            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<vc:Chart xmlns:vc=\"clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts\" Width=\"" + width.ToString() + "\" Height=\"" + height.ToString() + "\"  Theme=\"Theme1\" AnimatedUpdate=\"false\" AnimationEnabled=\"false\" View3D=\"True\"  ShadowEnabled=\"true\" CornerRadius=\"12\" DataPointWidth=\"15\" BorderThickness=\"0.5\" BorderBrush=\"Gray\" Padding=\"5,5,10,5\">");
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
            List<EventRowWithAllColumns> rows = DCSDashboardDemoHelper.Hidden_GetEventRows(startDate, endDate, level1);
            var q = from o in rows
                    group o by new { o.Level1, o.Level2, o.Level3 } into g
                    select new { g.Key, Occurrences = g.Count(), Minutes = g.Sum(o => o.Minutes), Path = getPath(g.Key.Level1, g.Key.Level2, g.Key.Level3), StartDate = g.Min(o => o.EventStart).Value.ToString(@"MM\/dd\/yyyy"), EndDate = g.Max(o => o.EventStop).Value.ToString(@"MM\/dd\/yyyy") };
            return ConvertToJsonString(q.ToList().OrderByDescending(o => o.Minutes).Take(15));
        }
    }
}
