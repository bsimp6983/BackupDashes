using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using DowntimeCollection_Demo;
using System.Web.Security;
using System.Data.Objects.DataClasses;
using System.Data;
using DowntimeCollection_Demo.Classes;

namespace DCSDemoData
{
    public partial class DiamondCrystaldashboardHelper
    {
        public static string Filter_Client
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }
        private const string ExcludeLevel1 = "*#&*@(@$(#$@&@$@$";//",Non-Downtime Machine Stops,";
        private const string ExcludeLevel2 = "*#&*@(@$(#$@&@$@$";
        private const string ExcludeLevel3 = "*#&*@(@$(#$@&@$@$";
        private const string Filter_Line = "company-demo";

        public static DowntimeReason getReason(int reasonCodeId)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in db.DowntimeReasonSet
                        where o.ID == reasonCodeId
                        select o;
                DowntimeReason d = q.FirstOrDefault();
                return d;
            }
        }

        /// <summary>
        /// 按Level1分组统计
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="orderByMinutes">是否按Minutes排序（DESC）,否则按Occurences</param>
        /// <returns></returns>
        public static List<LossBucketsRow> LossBuckets(DateTime? startTime, DateTime? endTime, bool orderByMinutes)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from x in
                            (
                                from g in
                                    (from a in db.DowntimeDataSet
                                     where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                     && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                                     && (!startTime.HasValue || a.EventStart >= startTime.Value)
                                     && (!endTime.HasValue || a.EventStart < endTime.Value)
                                     join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                                     where !string.IsNullOrEmpty(b.Level1)
                                     && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                                     select new { a.ID, b.Level1, a.Minutes })
                                group g by g.Level1 into p
                                select new { Minutes = p.Sum(o => o.Minutes), Occurences = p.Count(), p.Key }
                                )
                        orderby (orderByMinutes ? x.Minutes : x.Occurences) descending
                        select new LossBucketsRow { Level1 = x.Key, Minutes = x.Minutes, Occurences = x.Occurences };

                List<LossBucketsRow> result = q.ToList();
                if (result != null)
                {
                    int occ_total = result.Sum(o => o.Occurences);
                    decimal? min_total = result.Sum(o => o.Minutes);
                    foreach (var item in result)
                    {
                        item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / Convert.ToDecimal(occ_total) * 100;
                        item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 根据Level1来查询它下面的 Top N Events
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="level1">父Level1(为空时不限定范围)</param>
        /// <param name="takeCount">返回的行数 Top N</param>
        /// <param name="orderByMinutes">是否按Minutes排序（DESC）,否则按Occurences</param>
        /// <returns>{b{Minutes, Occurences,ReasonCodeID},c{DowntimeReasonSet}}</returns>
        public static List<TopEventsRow> TopEvents(DateTime? startTime, DateTime? endTime, string level1, int takeCount, bool orderByMinutes)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from b in
                            (from a in db.DowntimeDataSet
                             where (!startTime.HasValue || a.EventStart >= startTime.Value)
                             && (!endTime.HasValue || a.EventStart < endTime.Value)
                             && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                             group a by a.ReasonCodeID into g
                             select new { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), g.Key })
                        join c in db.vw_DowntimeReasonSet on b.Key equals c.ID
                        where (c.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            //group by level3
                        //&& (!string.IsNullOrEmpty(c.Level3))
                        //&& (!string.IsNullOrEmpty(c.Level2))
                        && !ExcludeLevel1.Contains("," + c.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + c.Level2 + ",") || string.IsNullOrEmpty(c.Level2))
                                     && (!ExcludeLevel3.Contains("," + c.Level3 + ",") || string.IsNullOrEmpty(c.Level3))
                        orderby (orderByMinutes ? b.Minutes : b.Occurences) descending, c.Level1 ascending, c.Level2 ascending, c.Level3
                        select new TopEventsRow { Client = c.Client, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Minutes = b.Minutes, Occurences = b.Occurences, ReasonCodeId = b.Key }
                      ;

                List<TopEventsRow> result = (takeCount > 0 ? q.Take(takeCount).ToList() : q.ToList());
                if (result != null)
                {
                    int occ_total = result.Sum(o => o.Occurences);
                    decimal? min_total = result.Sum(o => o.Minutes);
                    foreach (var item in result)
                    {
                        item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / Convert.ToDecimal(occ_total) * 100;
                        item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                    }
                    result = result.OrderByDescending(o => (orderByMinutes ? o.Minutes : o.Occurences)).ToList();
                }
                return result;
            }
        }

        /// <summary>
        /// 根据reasonCodeId来查询它下面的 Top N Events
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="reasonCodeId">父ReasonCodeId，如果未找到相关记录将引发错误</param>
        /// <param name="takeCount">返回的行数 Top N</param>
        /// <param name="orderByMinutes"></param>
        /// <returns>{b{Minutes, Occurences,ReasonCodeID},c{DowntimeReasonSet}}</returns>
        public static List<TopEventsRow> TopEvents(DateTime? startTime, DateTime? endTime, int reasonCodeId, int takeCount, bool orderByMinutes)
        {
            DowntimeReason dr = getReason(reasonCodeId);
            if (dr == null) throw new NullReferenceException(string.Format("Reason Code Id {0} was not found.", reasonCodeId));


            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from b in
                            (from a in db.DowntimeDataSet
                             where (!startTime.HasValue || a.EventStart >= startTime.Value)
                             && (!endTime.HasValue || a.EventStart < endTime.Value)
                             && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                             group a by a.ReasonCodeID into g
                             select new { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), g.Key })
                        join c in db.vw_DowntimeReasonSet on b.Key equals c.ID
                        where (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (c.Level1 == dr.Level1)
                        && (c.Level2 == dr.Level2 || string.IsNullOrEmpty(dr.Level2))
                        && (c.Level3 == dr.Level3 || string.IsNullOrEmpty(dr.Level3))
                        && !ExcludeLevel1.Contains("," + c.Level1 + ",")

                                     && (!ExcludeLevel2.Contains("," + c.Level2 + ",") || string.IsNullOrEmpty(c.Level2))
                                     && (!ExcludeLevel3.Contains("," + c.Level3 + ",") || string.IsNullOrEmpty(c.Level3))
                        orderby (orderByMinutes ? b.Minutes : b.Occurences) descending,c.Level1 ascending,c.Level2 ascending,c.Level3
                        select new TopEventsRow { Client = c.Client, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Minutes = b.Minutes, Occurences = b.Occurences, ReasonCodeId = b.Key }
                      ;

                List<TopEventsRow> result = (takeCount > 0 ? q.Take(takeCount).ToList() : q.ToList());
                if (result != null)
                {
                    int occ_total = result.Sum(o => o.Occurences);
                    decimal? min_total = result.Sum(o => o.Minutes);
                    foreach (var item in result)
                    {
                        item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / Convert.ToDecimal(occ_total) * 100;
                        item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                    }

                    result = result.OrderByDescending(o => (orderByMinutes ? o.Minutes : o.Occurences)).ToList();
                }
                return result;
            }
        }

        public static List<SpecialReportRow> TopEventsGroupByLevel3(DateTime? startTime, DateTime? endTime, string level1, int takeCount, bool orderByMinutes)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q1 = from c in
                             (from a in db.DowntimeDataSet
                              where (!startTime.HasValue || a.EventStart >= startTime.Value)
                                && (!endTime.HasValue || a.EventStart < endTime.Value)
                                && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                              join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                              where (b.Level1 == level1 || string.IsNullOrEmpty(level1))
                              //&& !string.IsNullOrEmpty(b.Level2) 
                              //&& !string.IsNullOrEmpty(b.Level3)
                              select new { a, b })
                         where !ExcludeLevel1.Contains("," + c.b.Level1 + ",")

                                    && (!ExcludeLevel2.Contains("," + c.b.Level2 + ",") || string.IsNullOrEmpty(c.b.Level2))
                                    && (!ExcludeLevel3.Contains("," + c.b.Level3 + ",") || string.IsNullOrEmpty(c.b.Level3))
                         group c by new { c.b.Level3,c.b.Level2, c.b.Level1 } into g
                         orderby g.Key.Level1 ascending
                         select new SpecialReportRow { Title = (!string.IsNullOrEmpty(g.Key.Level3)?g.Key.Level3:(!string.IsNullOrEmpty(g.Key.Level2)?g.Key.Level2:g.Key.Level1)), Minutes = g.Sum(o => o.a.Minutes), Occurences = g.Count(), appInfo = g.Min(o => o.b.ID) };//因为level3一定是唯一的，所以可以直接取一个MinId


                List<SpecialReportRow> result = q1.ToList();
                result = (from o in result
                          orderby (orderByMinutes ? o.Minutes : o.Occurences) descending, o.Title ascending
                          select o).ToList();
                //result = result.OrderByDescending(o => (orderByMinutes ? o.Minutes : o.Occurences)).ToList();
                result = (takeCount > 0 ? result.Take(takeCount).ToList() : result);
                if (result != null)
                {
                    int occ_total = result.Sum(o => o.Occurences);
                    decimal? min_total = result.Sum(o => o.Minutes);
                    foreach (var item in result)
                    {
                        item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / Convert.ToDecimal(occ_total) * 100;
                        item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                    }

                    result = result.OrderByDescending(o => (orderByMinutes ? o.Minutes : o.Occurences)).ToList();
                }

                return result;
            }
        }

        public static List<TopEventsRow> TopLevel3Events(DateTime? startTime, DateTime? endTime, string level1, int takeCount, bool orderByMinutes)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from b in
                            (from a in db.DowntimeDataSet
                             where (!startTime.HasValue || a.EventStart >= startTime.Value)
                             && (!endTime.HasValue || a.EventStart < endTime.Value)
                             && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                             group a by a.ReasonCodeID into g
                             select new { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), g.Key })
                        join c in db.vw_DowntimeReasonSet on b.Key equals c.ID
                        where (c.Level1 == level1 || string.IsNullOrEmpty(level1))
                        //&& !string.IsNullOrEmpty(c.Level2)
                        //&& !string.IsNullOrEmpty(c.Level3)
                        && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && !ExcludeLevel1.Contains("," + c.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + c.Level2 + ",") || string.IsNullOrEmpty(c.Level2))
                                     && (!ExcludeLevel3.Contains("," + c.Level3 + ",") || string.IsNullOrEmpty(c.Level3))
                        orderby (orderByMinutes ? b.Minutes : b.Occurences) descending,c.Level1 ascending,c.Level2 ascending, c.Level3 ascending
                        select new TopEventsRow { Client = c.Client, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Minutes = b.Minutes, Occurences = b.Occurences, ReasonCodeId = b.Key }
      ;

                List<TopEventsRow> result = (takeCount > 0 ? q.Take(takeCount).ToList() : q.ToList());
                if (result != null)
                {
                    int occ_total = result.Sum(o => o.Occurences);
                    decimal? min_total = result.Sum(o => o.Minutes);
                    foreach (var item in result)
                    {
                        item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / Convert.ToDecimal(occ_total) * 100;
                        item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                    }

                    result = result.OrderByDescending(o => (orderByMinutes ? o.Minutes : o.Occurences)).ToList();
                }
                return result;
            }
        }

        public static List<SpecialReportRow> HoursReport_Level3(DateTime? startTime, DateTime? endTime, int levelid)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows_Level3(startTime, endTime, levelid);
            if (rows != null)
            {
                for (var i = 0m; i < 24m; i += 0.5m)
                {
                    var q = from o in rows
                            where (o.EventStart.Value.Hour >= i && o.EventStart.Value.Hour < i + 0.5m)
                            select o;

                    List<EventRowWithAllColumns> r = q.ToList();
                    SpecialReportRow srr = new SpecialReportRow(); ;
                    srr.Title = SecondToHoursString(Convert.ToInt32(i * 3600));
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }
                    result.Add(srr);
                }
            }



            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }
            return result;
        }


        public static List<SpecialReportRow> HoursReport_Level3_DateRange(DateTime? startTime, DateTime? endTime, int levelid)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = null;

            if (levelid > 0)
                rows = GetEventRows_Level3(startTime, endTime, levelid);
            else
                rows = GetEventRows(startTime, endTime, "");

            decimal diff = (decimal)startTime.Value.Subtract(endTime.Value).TotalHours;

            if (diff > 0)
                diff = diff * -1;

            rows = rows.OrderBy(o => o.EventStart.Value.Day).ThenBy(o => o.EventStart.Value.Hour).ToList();

            if (rows != null)
            {
                DateTime date = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, startTime.Value.Hour, 0, 0);

                for (var i = diff; i < 0.00m; i += 1.0m)
                {
                    //decimal num = (decimal)GetHourByNumber((int)i);

                    if (date > endTime.Value)
                        break;


                    List<EventRowWithAllColumns> r = new List<EventRowWithAllColumns>();

                    var q = from o in rows
                            where (o.EventStart.Value.Hour == date.Hour)
                            && (o.EventStart.Value.Day == date.Day)
                            select o;

                    r = q.ToList();

                    SpecialReportRow srr = new SpecialReportRow();

                    string title = string.Empty;

                    int t = (date.Hour == 0 ? 12 : date.Hour);
                    string day = date.Date.ToString("MM/dd");
                    int m = date.Minute;

                    if (date.Hour < 12)
                        title = day + " " + t + ":" + m + " AM";
                    else
                    {
                        t = t - 12;

                        if (t == 0)
                            t = 12;

                        title = day + " " + t + ":" + m + " PM";
                    }

                    srr.Title = title;//SecondToHoursString(Convert.ToInt32(i * 3600));
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }

                    result.Add(srr);

                    
                    if (date.Hour == 23)
                    {
                        date = date.AddDays(1);
                        date = date.AddHours(-23);
                    }
                    else
                        date = date.AddHours(1);
                }
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }
            return result;
        }

        public static List<SpecialReportRow> DayReport_Level3(DateTime? startTime, DateTime? endTime, int levelid)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows_Level3(startTime, endTime, levelid);
           

            if (rows != null)
            {
                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q = from o in rows
                            where o.EventStart.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                            select o;

                    List<EventRowWithAllColumns> r = q.ToList();
                    SpecialReportRow srr = new SpecialReportRow(); ;
                    srr.Title = r_starttime.AddDays(i).ToString(@"MM\/dd");
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }
                    result.Add(srr);
                }
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : (min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100);
                item.OccurencesPercent = occ_total == 0 ? 0 : occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            return result;
        }

        public static List<SpecialReportRow> MonthReport_Level3(DateTime? startTime, DateTime? endTime, int levelid)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows_Level3(startTime, endTime, levelid);


            DateTime r_starttime, r_endTime;
            r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
            //查询时结束日期是多一天的
            r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);
            foreach (var item in DateTimeHelper.SplitMonths(r_starttime, r_endTime))
            {
                var q = from o in rows
                        where o.EventStart.Value.ToString(@"MM\/yyyy") == item.ToString(@"MM\/yyyy")
                        select o;

                List<EventRowWithAllColumns> r = q.ToList();
                SpecialReportRow srr = new SpecialReportRow(); ;
                srr.Title = item.ToString("MMM,yyyy", new System.Globalization.CultureInfo("en-US"));
                if (r != null)
                {
                    srr.Minutes = r.Sum(o => o.Minutes);
                    srr.Occurences = r.Count();

                }
                else
                {
                    srr.Minutes = 0;
                    srr.Occurences = 0;

                }
                result.Add(srr);
            }



            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            return result;
        }

        public static List<SpecialReportRow> YearReport_Level3(DateTime? startTime, DateTime? endTime, int levelid)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows_Level3(startTime, endTime, levelid);
            DateTime r_starttime, r_endTime;
            r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
            //查询时结束日期是多一天的
            r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);
            foreach (var item in DateTimeHelper.SplitYears(r_starttime, r_endTime))
            {
                var q = from o in rows
                        where o.EventStart.Value.ToString(@"yyyy") == item.ToString()
                        select o;

                List<EventRowWithAllColumns> r = q.ToList();
                SpecialReportRow srr = new SpecialReportRow(); ;
                srr.Title = item.ToString();
                if (r != null)
                {
                    srr.Minutes = r.Sum(o => o.Minutes);
                    srr.Occurences = r.Count();

                }
                else
                {
                    srr.Minutes = 0;
                    srr.Occurences = 0;

                }
                result.Add(srr);
            }


            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            return result;
        }

        public static List<SpecialReportRow> WeekReport_Level3(DateTime? startTime, DateTime? endTime, int levelid)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows_Level3(startTime, endTime, levelid);


            var q = from o in rows
                    group o by FeedCommDashboard.DateTimeHelper.GetWeekStart(o.EventStart.Value, DayOfWeek.Monday) into g
                    select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = g.Key.ToString(@"MM\/dd\/yyyy") };

            result = q.ToList();
            //var i = 0;
            //foreach (var item in new string[] { DayOfWeek.Sunday.ToString(), DayOfWeek.Monday.ToString(), DayOfWeek.Tuesday.ToString(), DayOfWeek.Wednesday.ToString(), DayOfWeek.Thursday.ToString(), DayOfWeek.Friday.ToString(), DayOfWeek.Saturday.ToString() })
            //{
            //    if ((from o in result
            //         where o.Title == item
            //         select o).Count() == 0)
            //    {
            //        SpecialReportRow srr = new SpecialReportRow { Minutes = 0, Occurences = 0, Title = item };
            //        result.Insert(i, srr);
            //    }
            //}

            //List<DateTime> weeks = null;
            //if (startTime.HasValue && endTime.HasValue)
            //{
            //    weeks = ComputeWeeks(startTime.Value, endTime.Value);
            //}
            //else
            //{
            //    weeks = ComputeWeeks(rows.Min(o => o.EventStart).Value, rows.Max(o => o.EventStop).Value);
            //}

            //System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");


            //foreach (var week in weeks)
            //{
            //    // week_startTime 和 week_endTime 标识出当前星期
            //    DateTime week_startTime = week;
            //    DateTime week_endTime = week_startTime.AddDays(7.0);
            //    var q = from o in rows
            //            where o.EventStart >= week_startTime && o.EventStop < week_endTime
            //            group o by o.Id into g
            //            select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = WeekOfYear(week_startTime).ToString() };
            //    SpecialReportRow srr = q.FirstOrDefault();
            //    if (srr == null)//补齐
            //    {
            //        //srr = new SpecialReportRow();
            //        //srr.Minutes = 0;
            //        //srr.Occurences = 0;
            //        //srr.Title = WeekOfYear(week_startTime).ToString();
            //        //result.Add(srr);
            //    }
            //    else
            //    {
            //        result.Add(srr);
            //    }


            //}


            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            //result = (from o in result
            //          orderby Convert.ToInt32(o.Title) ascending
            //          select o).ToList();

            //result = (from o in result
            //          orderby getWeekValue(o.Title) ascending
            //          select o).ToList();

            return result;
        }

        public static List<EventRowWithAllColumns> GetEventRows(DateTime? startTime, DateTime? endTime, string level1)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        join c in db.vw_DowntimeReasonSet on a.ReasonCodeID equals c.ID
                        where (c.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && !ExcludeLevel1.Contains("," + c.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + c.Level2 + ",") || string.IsNullOrEmpty(c.Level2))
                                     && (!ExcludeLevel3.Contains("," + c.Level3 + ",") || string.IsNullOrEmpty(c.Level3))
                        select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.ID, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                ;
                List<EventRowWithAllColumns> rows = q.ToList();
                return rows;

            }
        }

        public static List<EventRowWithAllColumns> GetEventRows_Level3(DateTime? startTime, DateTime? endTime, string level1)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        join c in db.vw_DowntimeReasonSet on a.ReasonCodeID equals c.ID
                        where (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (c.Level1 == level1 || string.IsNullOrEmpty(level1))
                       // && !string.IsNullOrEmpty(c.Level2)
                        //&& !string.IsNullOrEmpty(c.Level3)
                        && !ExcludeLevel1.Contains("," + c.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + c.Level2 + ",") || string.IsNullOrEmpty(c.Level2))
                                     && (!ExcludeLevel3.Contains("," + c.Level3 + ",") || string.IsNullOrEmpty(c.Level3))
                        select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.ID, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                ;
                List<EventRowWithAllColumns> rows = q.ToList();
                return rows;

            }
        }

        public static List<EventRowWithAllColumns> GetEventRows_Level3(DateTime? startTime, DateTime? endTime, int levelId)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        join c in db.vw_DowntimeReasonSet on a.ReasonCodeID equals c.ID
                        where (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (c.ID == levelId || levelId <= 0)
                        //&& !string.IsNullOrEmpty(c.Level2)
                        //&& !string.IsNullOrEmpty(c.Level3)
                        && !ExcludeLevel1.Contains("," + c.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + c.Level2 + ",") || string.IsNullOrEmpty(c.Level2))
                                     && (!ExcludeLevel3.Contains("," + c.Level3 + ",") || string.IsNullOrEmpty(c.Level3))
                        select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.ID, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                ;
                List<EventRowWithAllColumns> rows = q.ToList();
                return rows;

            }
        }

        public static List<SpecialReportRow> HoursReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);
            if (rows != null)
            {
                for (var i = 0m; i < 24m; i += 0.5m)
                {
                    var q = from o in rows
                            where (o.EventStart.Value.Hour >= i && o.EventStart.Value.Hour < i + 0.5m)
                            select o;

                    List<EventRowWithAllColumns> r = q.ToList();
                    SpecialReportRow srr = new SpecialReportRow(); ;
                    srr.Title = SecondToHoursString(Convert.ToInt32(i * 3600));
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }
                    result.Add(srr);
                }

            }



            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }
            return result;
        }


        public static List<SpecialReportRow> HoursReport_DateRange(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);

            decimal diff = (decimal)startTime.Value.Subtract(endTime.Value).TotalHours;

            if (diff > 0)
                diff = diff * -1;

            rows = rows.OrderBy(o => o.EventStart.Value.Day).ThenBy(o => o.EventStart.Value.Hour).ToList();

            if (rows != null)
            {
                DateTime date = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, startTime.Value.Hour, 0, 0);

                for (var i = diff; i < 0.00m; i += 1.0m)
                {
                    //decimal num = (decimal)GetHourByNumber((int)i);

                    if (date > endTime.Value)
                        break;


                    List<EventRowWithAllColumns> r = new List<EventRowWithAllColumns>();
                    
                    var q = from o in rows
                            where (o.EventStart.Value.Hour == date.Hour)
                            && (o.EventStart.Value.Day == date.Day)
                            select o;

                    r = q.ToList();

                    SpecialReportRow srr = new SpecialReportRow();

                    string title = string.Empty;
                    
                    int t = (date.Hour == 0 ? 12 : date.Hour);
                    string day = date.Date.ToString("MM/dd");
                    int m = date.Minute;

                    if (date.Hour < 12)
                        title = day + " " + t + ":" + m + " AM";
                    else
                    {
                        t = t - 12;

                        if (t == 0)
                            t = 12;

                        title = day + " " + t + ":" + m + " PM";
                    }

                    srr.Title = title;//SecondToHoursString(Convert.ToInt32(i * 3600));
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }
                    result.Add(srr);


                    if (date.Hour == 23)
                    {
                        date = date.AddDays(1);
                        date = date.AddHours(-23);
                    }
                    else
                        date = date.AddHours(1);
                }
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }
            return result;
        }

        public static List<SpecialReportRow> DayReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);
            if (rows != null)
            {
                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q = from o in rows
                            where o.EventStart.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                            select o;

                    List<EventRowWithAllColumns> r = q.ToList();
                    SpecialReportRow srr = new SpecialReportRow(); ;
                    srr.Title = r_starttime.AddDays(i).ToString(@"MM\/dd");
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }
                    result.Add(srr);
                }
            }
            
            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }
            return result;
        }

        public static List<SpecialReportRow> MonthReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);

            DateTime r_starttime, r_endTime;
            r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
            //查询时结束日期是多一天的
            r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);
            foreach (var item in DateTimeHelper.SplitMonths(r_starttime,r_endTime))
            {
                var q = from o in rows
                        where o.EventStart.Value.ToString(@"MM\/yyyy") == item.ToString(@"MM\/yyyy")
                        select o;

                List<EventRowWithAllColumns> r = q.ToList();
                SpecialReportRow srr = new SpecialReportRow(); ;
                srr.Title = item.ToString("MMM,yyyy", new System.Globalization.CultureInfo("en-US"));
                if (r != null)
                {
                    srr.Minutes = r.Sum(o => o.Minutes);
                    srr.Occurences = r.Count();

                }
                else
                {
                    srr.Minutes = 0;
                    srr.Occurences = 0;

                }
             
               result.Add(srr);
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            return result;
        }

        public static List<SpecialReportRow> YearReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);
            
            DateTime r_starttime, r_endTime;
            r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
            //查询时结束日期是多一天的
            r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);
            foreach (var item in DateTimeHelper.SplitYears(r_starttime,r_endTime))
            {
                var q = from o in rows
                        where o.EventStart.Value.ToString(@"yyyy") == item.ToString()
                        select o;

                List<EventRowWithAllColumns> r = q.ToList();
                SpecialReportRow srr = new SpecialReportRow(); ;
                srr.Title = item.ToString();
                if (r != null)
                {
                    srr.Minutes = r.Sum(o => o.Minutes);
                    srr.Occurences = r.Count();

                }
                else
                {
                    srr.Minutes = 0;
                    srr.Occurences = 0;

                }
                result.Add(srr);
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            return result;
        }

        public static List<SpecialReportRow> WeekReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);


            var q = from o in rows
                    group o by FeedCommDashboard.DateTimeHelper.GetWeekStart(o.EventStart.Value, DayOfWeek.Monday) into g
                    select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = g.Key.ToString(@"MM\/dd\/yyyy") };

            //var q = from o in rows
            //        group o by o.EventStart.Value.DayOfWeek into g
            //        select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = g.Key.ToString() };

            result = q.ToList();
            //var i = 0;
            //foreach (var item in new string[] { DayOfWeek.Sunday.ToString(), DayOfWeek.Monday.ToString(), DayOfWeek.Tuesday.ToString(), DayOfWeek.Wednesday.ToString(), DayOfWeek.Thursday.ToString(), DayOfWeek.Friday.ToString(), DayOfWeek.Saturday.ToString() })
            //{
            //    if ((from o in result
            //         where o.Title == item
            //         select o).Count() == 0)
            //    {
            //        SpecialReportRow srr = new SpecialReportRow { Minutes = 0, Occurences = 0, Title = item };
            //        result.Insert(i, srr);
            //    }
            //}



            //List<DateTime> weeks = null;
            //if (startTime.HasValue && endTime.HasValue)
            //{
            //    weeks = ComputeWeeks(startTime.Value , endTime.Value );
            //}
            //else
            //{
            //    weeks = ComputeWeeks(rows.Min(o => o.EventStart).Value, rows.Max(o => o.EventStop).Value);
            //}

            //System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");


            //foreach (var week in weeks)
            //{
            //    // week_startTime 和 week_endTime 标识出当前星期
            //    DateTime week_startTime = week;
            //    DateTime week_endTime = week_startTime.AddDays(7.0);
            //    var q = from o in rows
            //            where o.EventStart >= week_startTime && o.EventStop < week_endTime
            //            group o by o.Id into g
            //            select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = WeekOfYear(week_startTime).ToString() };
            //    SpecialReportRow srr = q.FirstOrDefault();
            //    if (srr == null)//补齐
            //    {
            //        //srr = new SpecialReportRow();
            //        //srr.Minutes = 0;
            //        //srr.Occurences = 0;
            //        //srr.Title = WeekOfYear(week_startTime).ToString();
            //        //result.Add(srr);
            //    }
            //    else
            //    {
            //        result.Add(srr);
            //    }


            //}


            //int occ_total = result.Sum(o => o.Occurences);
            //decimal? min_total = result.Sum(o => o.Minutes);
            //foreach (SpecialReportRow item in result)
            //{
            //    item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
            //    item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            //}

            //result = (from o in result
            //          orderby Convert.ToInt32(o.Title) ascending
            //          select o).ToList();

            //result = (from o in result
            //          orderby getWeekValue(o.Title) ascending
            //          select o).ToList();

            return result;
        }

        private static int getWeekValue(string week)
        {
            switch (week)
            {
                case "Sunday":
                    return 0;
                case "Monday":
                    return 1;
                case "Tuesday":
                    return 2;
                case "Wednesday":
                    return 3;
                case "Thursday":
                    return 4;
                case "Friday":
                    return 5;
                case "Saturday":
                    return 6;
            }
            return -1;
        }

        private static string getMonthName(int month)
        {
            DateTime d = new DateTime(1900, month, 1);
            return d.ToString("MMM", new System.Globalization.CultureInfo("en-US"));
        }

        /// <summary>
        /// 获取该年中是第几周
        /// </summary>
        /// <param name="day">日期</param>
        /// <returns></returns>
        private static int WeekOfYear(System.DateTime day)
        {
            int weeknum;
            System.DateTime fDt = DateTime.Parse(day.Year.ToString() + "-01-01");
            int k = Convert.ToInt32(fDt.DayOfWeek);//得到该年的第一天是周几 
            if (k == 0)
            {
                k = 7;
            }
            int l = Convert.ToInt32(day.DayOfYear);//得到当天是该年的第几天 
            l = l - (7 - k + 1);
            if (l <= 0)
            {
                weeknum = 1;
            }
            else
            {
                if (l % 7 == 0)
                {
                    weeknum = l / 7 + 1;
                }
                else
                {
                    weeknum = l / 7 + 2;//不能整除的时候要加上前面的一周和后面的一周 
                }
            }
            return weeknum;
        }

        public static string SecondToHoursString(int seconds)
        {
            return SecondToHoursString(seconds, true);
        }

        public static string SecondToHoursString(int seconds, bool showMinutes)
        {
            DateTime d = new DateTime(1900, 1, 1, 0, 0, 0);
            d = d.AddSeconds(seconds);
            if (d.Hour == 0)
            {
                if (showMinutes)
                {
                    return d.ToString("12:m tt", System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                }
                else
                {
                    return d.ToString("12 tt", System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                }
            }
            else
            {
                if (showMinutes)
                {
                    return d.ToString("h:m tt", System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                }
                else
                {
                    return d.ToString("h tt", System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                }
            }
        }
        public static object DetailsByLine(DateTime? startTime, DateTime? endTime, string level1)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from g in
                            (from a in db.DowntimeDataSet
                             where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (!startTime.HasValue || a.EventStart >= startTime.Value)
                             && (!endTime.HasValue || a.EventStart < endTime.Value)
                             && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                             join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                             where !string.IsNullOrEmpty(b.Level1)
                             && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))
                             select new { a.Line, a.ReasonCodeID, b.Level1, b.Client, a.Minutes })
                        where (g.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (g.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        group g by g.Line into p
                        orderby p.Key ascending
                        select new { Minutes = p.Sum(o => o.Minutes), Occurences = p.Count(), p.Key }
                      ;
                return q.ToList();
            }
        }

        public static object DetailsByLine(DateTime? startTime, DateTime? endTime, int reasonCodeId)
        {
            DowntimeReason dr = getReason(reasonCodeId);
            if (dr == null) throw new NullReferenceException(string.Format("Reason Code Id {0} was not found.", reasonCodeId));

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from g in
                            (from a in db.DowntimeDataSet
                             where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (!startTime.HasValue || a.EventStart >= startTime.Value)
                             && (!endTime.HasValue || a.EventStart < endTime.Value)
                             && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                             join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                             where !string.IsNullOrEmpty(b.Level1)
                             select new { a.Line, a.ReasonCodeID, b.Level1, b.Level2, b.Level3, b.Client, a.Minutes })
                        where (g.Level1 == dr.Level1)
                        && (g.Level2 == dr.Level2 || string.IsNullOrEmpty(dr.Level2))
                        && (g.Level3 == dr.Level3 || string.IsNullOrEmpty(dr.Level3))
                        && !ExcludeLevel1.Contains("," + g.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + g.Level2 + ",") || string.IsNullOrEmpty(g.Level2))
                                     && (!ExcludeLevel3.Contains("," + g.Level3 + ",") || string.IsNullOrEmpty(g.Level3))
                        && (g.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        group g by g.Line into p
                        orderby p.Key ascending
                        select new { Minutes = p.Sum(o => o.Minutes), Occurences = p.Count(), p.Key }
                ;
                return q.ToList();
            }
        }

        public static List<CommentRow> Comments(DateTime? startTime, DateTime? endTime, int reasonCodeId, string level1)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (b.ID == reasonCodeId || reasonCodeId <= 0)
                        && (b.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && !string.IsNullOrEmpty(a.Comment)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))
                        select new CommentRow { Line = a.Line, Minutes = a.Minutes, Client = b.Client, Comment = a.Comment, Level1 = b.Level1, Level2 = b.Level2, Level3 = b.Level3, ReasonCodeId = a.ReasonCodeID, EventStart = a.EventStart, EventStop = a.EventStop }
                ;
                return q.ToList();
            }
        }

        public static List<CommentRow> Comments(DateTime? startTime, DateTime? endTime, int reasonCodeId, string level1, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (b.ID == reasonCodeId || reasonCodeId <= 0)
                        && (b.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && !string.IsNullOrEmpty(a.Comment)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))
                        select new CommentRow { Line = a.Line, Minutes = a.Minutes, Client = b.Client, Comment = a.Comment, Level1 = b.Level1, Level2 = b.Level2, Level3 = b.Level3, ReasonCodeId = a.ReasonCodeID, EventStart = a.EventStart, EventStop = a.EventStop }
                ;
                return q.ToList();
            }
        }

        ///// <summary>
        ///// 为指定的 startDate 和 endDate 计算包含哪些星期，用每个星期的第一天标识
        ///// </summary>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <returns></returns>
        //public static List<DateTime> ComputeWeeks(DateTime startDate, DateTime endDate)
        //{
        //    if (startDate > endDate)
        //    {
        //        throw new ArgumentException();
        //    }


        //    List<DateTime> result = new List<DateTime>();

        //    // 第一个星期
        //    DateTime firstWeekStart = GetWeekStart(startDate, DayOfWeek.Monday);

        //    //  最后一个星期的下一个星期
        //    DateTime lastWeekStart = GetWeekStart(endDate, DayOfWeek.Monday).AddDays(7.0);

        //    //  循环变量
        //    DateTime time = firstWeekStart;

        //    while (time < lastWeekStart)
        //    {
        //        result.Add(time);

        //        //  向下走一个星期
        //        time = time.AddDays(7.0);

        //    }

        //    return result;
        //}

        ///// <summary>
        ///// 获取 time 所属的周的第一天
        ///// </summary>
        ///// <param name="time"></param>
        ///// <param name="firstDayOfWeek"></param>
        ///// <returns></returns>
        //private static DateTime GetWeekStart(DateTime time, DayOfWeek firstDayOfWeek)
        //{

        //    // 这里采用的是遍历的方法，从当前时间向前走，直到找到符合条件的日期
        //    while (true)
        //    {
        //        if (time.DayOfWeek == firstDayOfWeek)
        //        {
        //            DateTime result = new DateTime(time.Year, time.Month, time.Day);
        //            return result;
        //        }
        //        time = time.AddDays(-1.0);
        //    }
        //}


        public static decimal? TotalDowntime(DateTime? startTime, DateTime? endTime)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))
                        select a;
                return q.Sum(o => o.Minutes);
            }
        }

        public static GoalReportRow GetGoals(DateTime? startTime, DateTime? endTime, string level1, int levelId)
        {

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (a.ReasonCodeID == levelId || levelId <= 0)
                        join c in db.vw_DowntimeReasonSet on a.ReasonCodeID equals c.ID
                        where (c.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && !ExcludeLevel1.Contains("," + c.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + c.Level2 + ",") || string.IsNullOrEmpty(c.Level2))
                                     && (!ExcludeLevel3.Contains("," + c.Level3 + ",") || string.IsNullOrEmpty(c.Level3))
                        select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.ID, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                ;


                DateTime? sd = q.Min(o => o.EventStart);
                DateTime? ed = q.Max(o => o.EventStart);

                if (!sd.HasValue || !ed.HasValue) return new GoalReportRow { Downtimes = new GoalDowntimeRow(), Occurences = new GoalOccurencesRow() };
                GoalReportRow grr = GetGoals(sd.Value, ed.Value);

                return (grr == null ? new GoalReportRow { Downtimes = new GoalDowntimeRow(), Occurences = new GoalOccurencesRow() } : grr);

            }
        }

        public static GoalReportRow GetGoals(DateTime startTime, DateTime endTime)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in db.GoalSet
                        where o.StartTime >= startTime
                        && o.EndTime < endTime
                        && o.CLIENT == Filter_Client
                        && o.LINE == Filter_Line
                        select o;

                List<Goal> list = new List<Goal>();

                if (q.Any())
                    list = q.ToList();

                int totalOcc = 0;
                decimal totalTime = 0m;

                decimal totalDays = 0;//Math.Abs(Convert.ToDecimal( endTime.Subtract(startTime).TotalDays));

                foreach (var item in list)
                {
                    totalDays += Convert.ToDecimal(item.EndTime.Subtract(item.StartTime).TotalDays);
                    totalOcc += item.Occuring;
                    totalTime += item.Dowmtime;
                }

                if (totalDays == 0) totalDays = 1m;
                GoalReportRow grr = new GoalReportRow();
                grr.Downtimes.Hour = totalTime / (totalDays * 24);
                grr.Downtimes.Day = totalTime / totalDays;
                grr.Downtimes.Week = totalTime / (totalDays / 7);
                grr.Downtimes.Month = totalTime / (totalDays / 30);
                grr.Downtimes.Year = totalTime / (totalDays / 365);

                grr.Occurences.Hour = Convert.ToInt32(totalOcc / (totalDays * 24));
                grr.Occurences.Day = Convert.ToInt32(totalOcc / totalDays);
                grr.Occurences.Week = Convert.ToInt32(totalOcc / (totalDays / 7));
                grr.Occurences.Month = Convert.ToInt32(totalOcc / (totalDays / 30));
                grr.Occurences.Year = Convert.ToInt32(totalOcc / (totalDays / 365));

                return grr;
            }
        }

        public static List<ClientGoalRow> GetGoals()
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in db.GoalSet
                        where o.CLIENT == Filter_Client
                        && o.LINE == Filter_Line
                        orderby o.StartTime ascending
                        select new ClientGoalRow { Id = o.Id, StartTime = o.StartTime, EndTime = o.EndTime, Downtime = o.Dowmtime, Occuring = o.Occuring };
                return q.ToList();
            }
        }

        public static bool InsertGoal(DateTime startTime, DateTime endTime, decimal downtime, int occ)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                Goal goal = new Goal();
                goal.StartTime = startTime;
                goal.EndTime = endTime;
                goal.Dowmtime = downtime;
                goal.Occuring = occ;
                goal.CLIENT = Filter_Client;
                goal.LINE = Filter_Line;
                db.AddToGoalSet(goal);
                return db.SaveChanges() > 0;
            }
        }

        public static bool UpdateGoal(int id, DateTime startTime, DateTime endTime, decimal downtime, int occ)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in db.GoalSet
                        where o.Id == id
                        select o;

                Goal goal = q.FirstOrDefault();
                if (goal == null) return false;


                goal.StartTime = startTime;
                goal.EndTime = endTime;
                goal.Dowmtime = downtime;
                goal.Occuring = occ;
                return db.SaveChanges() > 0;
            }
        }

        public static bool DeleteGoal(int id)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in db.GoalSet
                        where o.Id == id
                        select o;

                Goal goal = q.FirstOrDefault();
                if (goal == null) return false;
                db.DeleteObject(goal);
                return db.SaveChanges() > 0;
            }
        }

        public static List<DowntimeHistoryRow> WeekDowntimeHistory(DateTime startTime, DateTime endTime, string level3)
        {
            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            List<DateTime> weeks = FeedCommDashboard.DateTimeHelper.SplitWeeks(startTime, endTime);
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                foreach (var weekStart in weeks)
                {
                    result.Add(DowntimeHistory(db, weekStart, weekStart.AddDays(7.0), level3));
                }
            }
            return result;
        }

        public static List<DowntimeHistoryRow> WeekDowntimeHistory(DateTime? startTime, DateTime? endTime, int levelId)
        {
            //if (levelId <= 0) throw new ArgumentOutOfRangeException("levelId");

            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                //结果里的LEVEL3换成了周

                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)

                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.ID == levelId || levelId <= 0)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))
                        orderby a.EventStart.Value.Year, a.EventStart.Value.Month
                        select new { a, b };

                var q1 = from o in q.ToList()
                         group o by FeedCommDashboard.DateTimeHelper.GetWeekStart(o.a.EventStart.Value, DayOfWeek.Monday) into g
                         select new DowntimeHistoryRow { Level3 = g.Key.ToString(@"MM\/dd\/yyyy"), MinutesSum = (g.Sum(o => o.a.Minutes).HasValue ? g.Sum(o => o.a.Minutes).Value : 0) };
                result = q1.ToList();

                //var i = 0;
                //foreach (var item in new string[] { DayOfWeek.Sunday.ToString(), DayOfWeek.Monday.ToString(), DayOfWeek.Tuesday.ToString(), DayOfWeek.Wednesday.ToString(), DayOfWeek.Thursday.ToString(), DayOfWeek.Friday.ToString(), DayOfWeek.Saturday.ToString() })
                //{
                //    if ((from o in result
                //         where o.Level3 == item
                //         select o).Count() == 0)
                //    {
                //        i++;
                //        DowntimeHistoryRow srr = new DowntimeHistoryRow { Level3 = item, MinutesSum = 0 };
                //        result.Insert(i - 1, srr);
                //    }
                //}

            }
            return result;
        }

        public static List<DowntimeHistoryRow> YearDowntimeHistory(DateTime startTime, DateTime endTime, string level3)
        {
            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.EventStart >= startTime)
                        && (a.EventStart < endTime)
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.Level3 == level3 || level3 == null)

                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                      && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))
                        select new { a, b }
                        ;

                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = startTime;
                //查询时结束日期是多一天的
                r_endTime = endTime;

                foreach (var item in DateTimeHelper.SplitYears(r_starttime, r_endTime))
                {
                    var q1 = from o in rows
                             where o.a.EventStart.Value.ToString(@"yyyy") == item.ToString()
                             select o;

                    var r = q1.ToList();
                    result.Add(new DowntimeHistoryRow { Level3 = item.ToString(), MinutesSum = r.Sum(o => o.a.Minutes.Value) });
                }

            }
            return result;
        }

        public static List<DowntimeHistoryRow> YearDowntimeHistory(DateTime? startTime, DateTime? endTime, int levelId)
        {
            //if (levelId <= 0) throw new ArgumentOutOfRangeException("levelId");

            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                //结果里的LEVEL3换成了年

                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.ID == levelId || levelId <= 0)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                        select new { a, b }
                        ;

                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.a.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.a.EventStart).Value);

                foreach (var item in DateTimeHelper.SplitYears(r_starttime, r_endTime))
                {
                    var q1 = from o in rows
                             where o.a.EventStart.Value.ToString(@"yyyy") == item.ToString()
                             select o;

                    var r = q1.ToList();
                    result.Add(new DowntimeHistoryRow { Level3 = item.ToString(), MinutesSum = r.Sum(o => o.a.Minutes.Value) });
                }

            }
            return result;
        }

        public static List<DowntimeHistoryRow> MonthDowntimeHistory(DateTime? startTime, DateTime? endTime, int levelId)
        {
            //if (levelId <= 0) throw new ArgumentOutOfRangeException("levelId");

            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                //结果里的LEVEL3换成了月份

                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.ID == levelId || levelId <= 0)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                        select new { a, b }
                        ;

                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.a.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.a.EventStart).Value);

                foreach (var item in DateTimeHelper.SplitMonths(r_starttime,r_endTime))
                {
                    var q1 = from o in rows
                             where o.a.EventStart.Value.ToString(@"MM\/yyyy") == item.ToString(@"MM\/yyyy")
                             select o;

                    var r = q1.ToList();
                    result.Add(new DowntimeHistoryRow { Level3 = item.ToString("MMM,yyyy", new System.Globalization.CultureInfo("en-US")), MinutesSum = r.Sum(o => o.a.Minutes.Value) });
                }
               

            }
            return result;
        }

        public static List<DowntimeHistoryRow> DayDowntimeHistory(DateTime? startTime, DateTime? endTime, int levelId)
        {
            //if (levelId <= 0) throw new ArgumentOutOfRangeException("levelId");

            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                //结果里的LEVEL3换成了月份

                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.ID == levelId || levelId <= 0)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                        orderby a.EventStart.Value.Year, a.EventStart.Value.Month
                        select new { a, b }
                        ;

                var rows = q.ToList();

               
                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.a.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.a.EventStart).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q1 = from o in rows
                            where o.a.EventStart.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                            select o;

                    var r = q1.ToList();
                    result.Add(new DowntimeHistoryRow { Level3 = r_starttime.AddDays(i).ToString(@"MM\/dd"), MinutesSum = r.Sum(o => o.a.Minutes.Value) });
                }


            }
            return result;
        }

        public static List<DowntimeHistoryRow> HoursDowntimeHistory(DateTime? startTime, DateTime? endTime, int levelId)
        {
            //if (levelId <= 0) throw new ArgumentOutOfRangeException("levelId");

            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {

                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.ID == levelId || levelId <= 0)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                        orderby a.EventStart.Value.Year, a.EventStart.Value.Month
                        group a by a.EventStart.Value into g
                        select new DowntimeHistoryRow { Time = g.Key, MinutesSum = g.Sum(o => o.Minutes.Value) }
                        ;
                result = q.ToList();

                List<DowntimeHistoryRow> result2 = new List<DowntimeHistoryRow>();

                decimal diff = (decimal)endTime.Value.Subtract(startTime.Value).TotalHours;

                if (diff > 0)
                    diff = diff * -1;

                DateTime d = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, startTime.Value.Hour, 0, 0);

                for (decimal i = diff; i < 0m; i += 1.0m)
                {
                    DowntimeHistoryRow dhr = (from o in result
                                              where (o.Time.Hour == d.Hour)
                                              && (o.Time.Day == d.Day)
                                              select o).FirstOrDefault();

                    int t = d.Hour;
                    string day = d.Date.ToString("MM/dd");
                    string hr = string.Empty;

                    if (t < 12)
                    {
                        if (t == 0)
                            t = 12;

                        hr = day + " " + t + " AM";
                    }
                    else
                    {
                        t = t - 12;

                        if (t == 0)
                            t = 12;

                        hr = day + " " + t + " PM";
                    }
                    

                    if (dhr == null)
                    {
                        result2.Add(new DowntimeHistoryRow { Level3 = hr, MinutesSum = 0 });
                    }
                    else
                    {
                        dhr.Level3 = hr;//SecondToHoursString(Convert.ToInt32(i * 3600m));
                        result2.Add(dhr);
                    }


                    if (d.Hour == 23)
                    {
                        d = d.AddDays(1);
                        d = d.AddHours(-23);
                    }
                    else
                        d = d.AddHours(1);
                }
                return result2;

            }
        }

        public static List<DowntimeHistoryRow> DayDowntimeHistory(DateTime startTime, DateTime endTime, string level3)
        {
            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.EventStart >= startTime)
                        && (a.EventStart < endTime)
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                        join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                        where (b.Level3 == level3 || level3 == null)
                        && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                        select new { a, b }
                        ;

                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = startTime;
                //查询时结束日期是多一天的
                r_endTime = endTime;
                
                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q1 = from o in rows
                             where o.a.EventStart.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                             select o;

                    var r = q1.ToList();
                    result.Add(new DowntimeHistoryRow { Level3 = r_starttime.AddDays(i).ToString(@"MM\/dd"), MinutesSum = r.Sum(o => o.a.Minutes.Value) });
                }

            }
            return result;
        }

        /// <summary>
        /// 获取一段时间内（由 startTime和endTime标识），level3 的downtime之和
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="level3"></param>
        /// <returns></returns>
        private static DowntimeHistoryRow DowntimeHistory(DB db, DateTime startTime, DateTime endTime, string level3)
        {
            var q = from a in db.DowntimeDataSet
                    where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                    && (a.EventStart >= startTime)
                    && (a.EventStart < endTime)
                    && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                    join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                    where (b.Level3 == level3 || level3 == null)
                    && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                    select a
                    ;
            return new DowntimeHistoryRow
            {
                Time = startTime,
                Level3 = level3,
                MinutesSum = q.Sum(x => x.Minutes) ?? 0m
            };
        }

        public static List<AllOccurrenceHistoryRow> MonthOccurrenceHistory(DateTime? startTime, DateTime? endTime, int levelId)
        {
            List<AllOccurrenceHistoryRow> result = new List<AllOccurrenceHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in
                            (
                            from a in db.DowntimeDataSet
                            where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (!startTime.HasValue || a.EventStart >= startTime)
                            && (!endTime.HasValue || a.EventStart < endTime)
                            && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                            join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                            where (b.ID == levelId || levelId <= 0)
                            && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                            group a by new { b.ID,b.Level1,b.Level2, b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level1,g.Key.Level2,g.Key.Level3, Occurrences = g.Count() }
                            )
                        where o.Occurrences > 0
                        orderby o.Occurrences descending
                        select o;

                foreach (var item in q.Take(5).ToList())
                {
                    AllOccurrenceHistoryRow aohr = new AllOccurrenceHistoryRow();
                    if (!string.IsNullOrEmpty(item.Level3))
                    {
                        aohr.Level3 = item.Level3;
                    }
                    else if (!string.IsNullOrEmpty(item.Level2))
                    {
                        aohr.Level3 = item.Level2;
                    }
                    else
                    {
                        aohr.Level3 = item.Level1;
                    }
                    aohr.Datas = getLevelMonthOccurrenceData(startTime, endTime, item.ID);
                    result.Add(aohr);
                }
            }
            return result;
        }

        public static List<AllOccurrenceHistoryRow> YearOccurrenceHistory(DateTime? startTime, DateTime? endTime, int levelId)
        {
            List<AllOccurrenceHistoryRow> result = new List<AllOccurrenceHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in
                            (
                            from a in db.DowntimeDataSet
                            where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (!startTime.HasValue || a.EventStart >= startTime)
                            && (!endTime.HasValue || a.EventStart < endTime)
                            && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                            join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                            where (b.ID == levelId || levelId <= 0)
                            && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                            group a by new { b.ID,b.Level1, b.Level2,b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level1,g.Key.Level2,g.Key.Level3, Occurrences = g.Count() }
                            )
                        where o.Occurrences > 0
                        orderby o.Occurrences descending
                        select o;

                foreach (var item in q.Take(5).ToList())
                {
                    AllOccurrenceHistoryRow aohr = new AllOccurrenceHistoryRow();
                    if (!string.IsNullOrEmpty(item.Level3))
                    {
                        aohr.Level3 = item.Level3;
                    }
                    else if (!string.IsNullOrEmpty(item.Level2))
                    {
                        aohr.Level3 = item.Level2;
                    }
                    else
                    {
                        aohr.Level3 = item.Level1;
                    }
                    //aohr.Datas = getLevelMonthOccurrenceData(startTime, endTime, item.ID);
                    result.Add(aohr);
                }
            }
            return result;
        }

        public static List<AllOccurrenceHistoryRow> getOccurrenceHistory(DateTime? startTime, DateTime? endTime, int levelId, string type)
        {
            List<AllOccurrenceHistoryRow> result = new List<AllOccurrenceHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in
                            (
                            from a in db.DowntimeDataSet
                            where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (!startTime.HasValue || a.EventStart >= startTime)
                            && (!endTime.HasValue || a.EventStart < endTime)
                            && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                            join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                            where (b.ID == levelId || levelId <= 0)
                            && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                            group a by new { b.ID, b.Level1,b.Level2,b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level1,g.Key.Level2,g.Key.Level3, Occurrences = g.Count() }
                            )
                        where o.Occurrences > 0
                        orderby o.Occurrences descending
                        select o;

                foreach (var item in q.Take(5).ToList())
                {
                    AllOccurrenceHistoryRow aohr = new AllOccurrenceHistoryRow();
                     
                    if (!string.IsNullOrEmpty(item.Level3))
                    {
                        aohr.Level3 = item.Level3;
                    }
                    else if (!string.IsNullOrEmpty(item.Level2))
                    {
                        aohr.Level3 = item.Level2;
                    }
                    else
                    {
                        aohr.Level3 = item.Level1;
                    }

                    switch (type.ToLower())
                    {
                        case "hours":
                            aohr.Datas = getLevelHoursOccurrenceData(startTime, endTime, item.ID);
                            break;
                        case "day":
                            aohr.Datas = getLevelDayOccurrenceData(startTime, endTime, item.ID);
                            break;
                        case "week":
                            aohr.Datas = getLevelWeekOccurrenceData(startTime, endTime, item.ID);
                            break;
                        case "month":
                            aohr.Datas = getLevelMonthOccurrenceData(startTime, endTime, item.ID);
                            break;
                        case "year":
                            aohr.Datas = getLevelYearOccurrenceData(startTime, endTime, item.ID);
                            break;
                        default:
                            aohr.Datas = getLevelDayOccurrenceData(startTime, endTime, item.ID);
                            break;
                    }
                    result.Add(aohr);
                   
                }
            }
            return result;
        }

        private static Dictionary<string, int> getLevelMonthOccurrenceData(DateTime? startTime, DateTime? endTime, int levelId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where a.ReasonCodeID == levelId
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        select a
                        ;

                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);

                foreach (var item in DateTimeHelper.SplitMonths(r_starttime, r_endTime))
                {
                    var q1 = from o in rows
                             where o.EventStart.Value.ToString(@"MM\/yyyy") == item.ToString(@"MM\/yyyy")
                             select o;

                    var r = q1.ToList();
                    result.Add(item.ToString("MMM,yyyy", new System.Globalization.CultureInfo("en-US")), r.Count());    
                }
                return result;
            }
        }

        private static Dictionary<string, int> getLevelYearOccurrenceData(DateTime? startTime, DateTime? endTime, int levelId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where a.ReasonCodeID == levelId
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        select a
                        ;

                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);

                foreach (var item in DateTimeHelper.SplitYears(r_starttime, r_endTime))
                {
                    var q1 = from o in rows
                             where o.EventStart.Value.ToString(@"yyyy") == item.ToString()
                             select o;

                    var r = q1.ToList();
                    result.Add(item.ToString(), r.Count());
                }
                return result;
            }
        }

        private static Dictionary<string, int> getLevelDayOccurrenceData(DateTime? startTime, DateTime? endTime, int levelId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where a.ReasonCodeID == levelId
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        select a 
                        ;

                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q1 = from o in rows
                             where o.EventStart.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                             select o;

                    var r = q1.ToList();
                    result.Add(r_starttime.AddDays(i).ToString(@"MM\/dd"), r.Count());
                }
                return result;
            }
        }

        private static Dictionary<string, int> getLevelHoursOccurrenceData(DateTime? startTime, DateTime? endTime, int levelId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where a.ReasonCodeID == levelId
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        group a by a.EventStart.Value into g
                        select new { g.Key, Occurrence = g.Count() };

                var rows = q.ToList();

                decimal diff = (decimal)endTime.Value.Subtract(startTime.Value).TotalHours;

                if (diff > 0)
                    diff = diff * -1;

                DateTime d = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, startTime.Value.Hour, 0, 0);

                for (decimal i = diff; i < 0m; i += 1.0m)
                {
                    var dhr = (from o in rows
                                where (o.Key.Hour == d.Hour)
                                && (o.Key.Day == d.Day)
                                select o).FirstOrDefault();

                    int t = d.Hour;
                    string date = d.Date.ToString("MM/dd");
                    string hr = string.Empty;

                    if (t < 12)
                    {
                        if (t == 0)
                            t = 12;

                        hr = date + " " + t + " AM";
                    }
                    else
                    {
                        t = t - 12;

                        if (t == 0)
                            t = 12;

                        hr = date + " " + t + " PM";
                    }


                    if (dhr == null)
                    {
                        result.Add(hr, 0);
                    }
                    else
                    {
                        result.Add(hr, dhr.Occurrence);
                    }

                    if (d.Hour == 23)
                    {
                        d = d.AddDays(1);
                        d = d.AddHours(-23);
                    }
                    else
                        d = d.AddHours(1);
                }
                /*
                for (decimal i = 0; i < 24m; i += 0.5m)
                {
                    var dhr = (from o in rows
                               where (Convert.ToDecimal(o.Key) == i)
                               select o).FirstOrDefault();
                    if (dhr == null)
                    {
                        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m)), 0);
                    }
                    else
                    {
                        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m)), dhr.Occurrence);
                    }
                }*/
                return result;
            }
        }

        private static Dictionary<string, int> getLevelWeekOccurrenceData(DateTime? startTime, DateTime? endTime, int levelId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where a.ReasonCodeID == levelId
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        select a;

                var q1 = from a in q.ToList()
                         group a by FeedCommDashboard.DateTimeHelper.GetWeekStart(a.EventStart.Value, DayOfWeek.Monday) into g
                         select new { g.Key, Occurrence = g.Count() };

                var rows = q1.ToList();
                foreach (var item in rows)
                {
                    result.Add(item.Key.ToString(@"MM\/dd\/yyyy"), item.Occurrence);
                }
                //foreach (var item in new string[] { DayOfWeek.Sunday.ToString(), DayOfWeek.Monday.ToString(), DayOfWeek.Tuesday.ToString(), DayOfWeek.Wednesday.ToString(), DayOfWeek.Thursday.ToString(), DayOfWeek.Friday.ToString(), DayOfWeek.Saturday.ToString() })
                //{
                //    var r = (from o in rows
                //             where o.Key.ToString() == item
                //             select o).FirstOrDefault();
                //    if (r == null)
                //    {
                //        result.Add(item, 0);
                //    }
                //    else
                //    {
                //        result.Add(r.Key.ToString(), r.Occurrence);
                //    }
                //}

                return result;
            }
        }

        public static Dictionary<string, List<DowntimeHistoryRow>> getDowntimesHistory(DateTime? startTime, DateTime? endTime, int levelId, string type)
        {
            Dictionary<string, List<DowntimeHistoryRow>> result = new Dictionary<string, List<DowntimeHistoryRow>>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in
                            (
                            from a in db.DowntimeDataSet
                            where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (!startTime.HasValue || a.EventStart >= startTime)
                            && (!endTime.HasValue || a.EventStart < endTime)
                            && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))

                            join b in db.vw_DowntimeReasonSet on a.ReasonCodeID equals b.ID
                            where (b.ID == levelId || levelId <= 0)
                            && !ExcludeLevel1.Contains("," + b.Level1 + ",")
                                     && (!ExcludeLevel2.Contains("," + b.Level2 + ",") || string.IsNullOrEmpty(b.Level2))
                                     && (!ExcludeLevel3.Contains("," + b.Level3 + ",") || string.IsNullOrEmpty(b.Level3))

                            group a by new { b.ID,b.Level1,b.Level2,b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level3,g.Key.Level2,g.Key.Level1,minutes = g.Sum(o => o.Minutes) }
                            )
                        where o.minutes > 0
                        orderby o.minutes descending
                        select o;

                foreach (var item in q.Take(5).ToList())
                {
                    List<DowntimeHistoryRow> aohr = new List<DowntimeHistoryRow>();
                    switch (type.ToLower())
                    {
                        case "hours":
                            aohr = HoursDowntimeHistory(startTime, endTime, item.ID);
                            break;
                        case "day":
                            aohr = DayDowntimeHistory(startTime, endTime, item.ID);
                            break;
                        case "week":
                            aohr = WeekDowntimeHistory(startTime, endTime, item.ID);
                            break;
                        case "month":
                            aohr = MonthDowntimeHistory(startTime, endTime, item.ID);
                            break;
                        case "year":
                            aohr = YearDowntimeHistory(startTime, endTime, item.ID);
                            break;
                        default:
                            aohr = DayDowntimeHistory(startTime, endTime, item.ID);
                            break;
                    }
                    if (!string.IsNullOrEmpty(item.Level3))
                    {
                        result.Add(item.Level3, aohr);
                    }
                    else if (!string.IsNullOrEmpty(item.Level2))
                    {
                        result.Add(item.Level2, aohr);
                    }
                    else
                    {
                        result.Add(item.Level1, aohr);
                    }
                }
            }
            return result;
        }

        #region Hidden Reasons Downtimes
        public static List<TopEventsRow> Hidden_TopEvents(DateTime? startTime, DateTime? endTime, string level1, int takeCount, bool orderByMinutes)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from b in
                            (from a in db.DowntimeDataSet
                             where (!startTime.HasValue || a.EventStart >= startTime.Value)
                             && (!endTime.HasValue || a.EventStart < endTime.Value)
                             && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                             && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                             group a by a.ReasonCodeID into g
                             select new { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), g.Key })
                        join c in db.DowntimeReasonSet on b.Key equals c.ID
                        where (c.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && c.HideReasonInReports == true
                        orderby (orderByMinutes ? b.Minutes : b.Occurences) descending, c.Level1 ascending, c.Level2 ascending, c.Level3
                        select new TopEventsRow { Client = c.Client, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Minutes = b.Minutes, Occurences = b.Occurences, ReasonCodeId = b.Key }
                      ;

                List<TopEventsRow> result = (takeCount > 0 ? q.Take(takeCount).ToList() : q.ToList());
                if (result != null)
                {
                    int occ_total = result.Sum(o => o.Occurences);
                    decimal? min_total = result.Sum(o => o.Minutes);
                    foreach (var item in result)
                    {
                        item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / Convert.ToDecimal(occ_total) * 100;
                        item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                    }
                    result = result.OrderByDescending(o => (orderByMinutes ? o.Minutes : o.Occurences)).ToList();
                }
                return result;
            }
        }

        public static List<EventRowWithAllColumns> Hidden_GetEventRows(DateTime? startTime, DateTime? endTime, string level1)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.DowntimeDataSet
                        where (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        join c in db.DowntimeReasonSet on a.ReasonCodeID equals c.ID
                        where (c.Level1 == level1 || string.IsNullOrEmpty(level1))
                        && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && c.HideReasonInReports == true
                        select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.ID, Level1 = c.Level1, Level2 = c.Level2, Level3 = c.Level3, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                ;
                List<EventRowWithAllColumns> rows = q.ToList();
                return rows;

            }
        }

        public static List<SpecialReportRow> Hidden_HoursReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);

            decimal diff = (decimal)startTime.Value.Subtract(endTime.Value).TotalHours;

            if (diff > 0)
                diff = diff * -1;

            rows = rows.OrderBy(o => o.EventStart.Value.Day).ThenBy(o => o.EventStart.Value.Hour).ToList();

            if (rows != null)
            {
                DateTime date = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, startTime.Value.Hour, 0, 0);

                for (var i = diff; i < 0.00m; i += 1.0m)
                {
                    //decimal num = (decimal)GetHourByNumber((int)i);

                    if (date > endTime.Value)
                        break;


                    List<EventRowWithAllColumns> r = new List<EventRowWithAllColumns>();

                    var q = from o in rows
                            where (o.EventStart.Value.Hour == date.Hour)
                            && (o.EventStart.Value.Day == date.Day)
                            select o;

                    r = q.ToList();

                    SpecialReportRow srr = new SpecialReportRow();

                    string title = string.Empty;

                    int t = (date.Hour == 0 ? 12 : date.Hour);
                    string day = date.Date.ToString("MM/dd");
                    int m = date.Minute;

                    if (date.Hour < 12)
                        title = day + " " + t + ":" + m + " AM";
                    else
                    {
                        t = t - 12;

                        if (t == 0)
                            t = 12;

                        title = day + " " + t + ":" + m + " PM";
                    }

                    srr.Title = title;//SecondToHoursString(Convert.ToInt32(i * 3600));
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }
                    result.Add(srr);


                    if (date.Hour == 23)
                    {
                        date = date.AddDays(1);
                        date = date.AddHours(-23);
                    }
                    else
                        date = date.AddHours(1);
                }
            }



            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }
            return result;


        }

        public static int GetHourByNumber(int num)
        {
            for (int x = 1; x < 24; x++)
            {
                if (num % x == 0)
                    return x;
            }

            return -1;
        }

        public static List<SpecialReportRow> Hidden_DayReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, level1);
            if (rows != null)
            {
                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q = from o in rows
                            where o.EventStart.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                            select o;

                    List<EventRowWithAllColumns> r = q.ToList();
                    SpecialReportRow srr = new SpecialReportRow(); ;
                    srr.Title = r_starttime.AddDays(i).ToString(@"MM\/dd");
                    if (r != null)
                    {
                        srr.Minutes = r.Sum(o => o.Minutes);
                        srr.Occurences = r.Count();

                    }
                    else
                    {
                        srr.Minutes = 0;
                        srr.Occurences = 0;

                    }
                    result.Add(srr);
                }
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }
            return result;
        }

        public static List<SpecialReportRow> Hidden_MonthReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, level1);

            DateTime r_starttime, r_endTime;
            r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
            //查询时结束日期是多一天的
            r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);
            foreach (var item in DateTimeHelper.SplitMonths(r_starttime, r_endTime))
            {
                var q = from o in rows
                        where o.EventStart.Value.ToString(@"MM\/yyyy") == item.ToString(@"MM\/yyyy")
                        select o;

                List<EventRowWithAllColumns> r = q.ToList();
                SpecialReportRow srr = new SpecialReportRow(); ;
                srr.Title = item.ToString("MMM,yyyy", new System.Globalization.CultureInfo("en-US"));
                if (r != null)
                {
                    srr.Minutes = r.Sum(o => o.Minutes);
                    srr.Occurences = r.Count();

                }
                else
                {
                    srr.Minutes = 0;
                    srr.Occurences = 0;

                }

                result.Add(srr);
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            return result;
        }

        public static List<SpecialReportRow> Hidden_YearReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, level1);

            DateTime r_starttime, r_endTime;
            r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
            //查询时结束日期是多一天的
            r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStart).Value);
            foreach (var item in DateTimeHelper.SplitYears(r_starttime, r_endTime))
            {
                var q = from o in rows
                        where o.EventStart.Value.ToString(@"yyyy") == item.ToString()
                        select o;

                List<EventRowWithAllColumns> r = q.ToList();
                SpecialReportRow srr = new SpecialReportRow(); ;
                srr.Title = item.ToString();
                if (r != null)
                {
                    srr.Minutes = r.Sum(o => o.Minutes);
                    srr.Occurences = r.Count();

                }
                else
                {
                    srr.Minutes = 0;
                    srr.Occurences = 0;

                }
                result.Add(srr);
            }

            int occ_total = result.Sum(o => o.Occurences);
            decimal? min_total = result.Sum(o => o.Minutes);
            foreach (SpecialReportRow item in result)
            {
                item.MinutesPercent = min_total == 0 ? 0 : item.Minutes.Value / min_total.Value * 100;
                item.OccurencesPercent = occ_total == 0 ? 0 : item.Occurences / occ_total * 100;
            }

            return result;
        }

        public static List<SpecialReportRow> Hidden_WeekReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, level1);


            var q = from o in rows
                    group o by FeedCommDashboard.DateTimeHelper.GetWeekStart(o.EventStart.Value, DayOfWeek.Monday) into g
                    select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = g.Key.ToString(@"MM\/dd\/yyyy") };

            result = q.ToList();

            return result;
        }

        #endregion

        #region CountCase
        public static class CountCaseGoals
        {
            public static decimal Mark
            {
                get
                {
                    return getEstimatedCount(Filter_Line);
                    /*
                    if (System.Web.Configuration.WebConfigurationManager.AppSettings["CountCaseGoals_Mark"] == null)
                    {
                        Mark = 60m;
                    }
                    decimal v = 0;
                    decimal.TryParse(System.Web.Configuration.WebConfigurationManager.AppSettings.Get("CountCaseGoals_Mark"), out v);
                    return v;
                    */


                }

                set
                {
                    /*
                    System.Web.Configuration.WebConfigurationManager.AppSettings.Set("CountCaseGoals_Mark", value.ToString());
                    if (System.Web.Configuration.WebConfigurationManager.AppSettings["CountCaseGoals_Mark"] == null)
                    {
                        System.Web.Configuration.WebConfigurationManager.AppSettings.Add("CountCaseGoals_Mark", value.ToString());
                    }
                    else
                    {
                        System.Web.Configuration.WebConfigurationManager.AppSettings.Set("CountCaseGoals_Mark", value.ToString());
                    }
                     */
                }
            }
            public static int MaxCaseCount
            {
                get
                {
                    if (System.Web.Configuration.WebConfigurationManager.AppSettings["CountCaseGoals_MaxCaseCount"] == null)
                    {
                        MaxCaseCount = 300;
                    }
                    int v = 0;
                    int.TryParse(System.Web.Configuration.WebConfigurationManager.AppSettings.Get("CountCaseGoals_MaxCaseCount"), out v);
                    return v;
                }
                set {
                    System.Web.Configuration.WebConfigurationManager.AppSettings.Set("CountCaseGoals_MaxCaseCount", value.ToString());
                    if (System.Web.Configuration.WebConfigurationManager.AppSettings["CountCaseGoals_MaxCaseCount"] == null)
                    {
                        System.Web.Configuration.WebConfigurationManager.AppSettings.Add("CountCaseGoals_MaxCaseCount", value.ToString());
                    }
                    else
                    {
                        System.Web.Configuration.WebConfigurationManager.AppSettings.Set("CountCaseGoals_MaxCaseCount", value.ToString());
                    }
                }
            }


            public static decimal MinuteCases {
                get
                {
                    //return 7.14m;
                    return getEstimatedCount(Filter_Line) / 60;
                }
            }
        }
        public static Dictionary<string, int> getLeft12HoursCountCase(DateTime clientTime)
        {
            DateTime d = clientTime.AddHours(-12);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0);
            DateTime? endTime = new DateTime(clientTime.AddHours(-1).Year, clientTime.AddHours(-1).Month, clientTime.AddHours(-1).Day, clientTime.AddHours(-1).Hour, 59, 59);
            Dictionary<string, int> result = new Dictionary<string, int>();
            Dictionary<int, int> collection = new Dictionary<int, int>();

            ////sample datas
            //Random rdm = new Random();
            //for (var i = 1; i <= 12;i++ )
            //{
            //    double v = rdm.NextDouble()*CountCaseGoals.MaxCaseCount;
            //    int h = DateTime.Now.AddHours(i-12).Hour;
            //    result.Add(Regex.Replace(SecondToHoursString(Convert.ToInt32(h * 3600m)),"\\:00 ",""), (int)v);
            //}
            //return result;

            //
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        group a by a.EventStop.Value.Hour into g
                        select new { g.Key, CountCase = g.Sum(o => o.CaseCount1) };

                var rows = q.ToList();
                for (var i = 0; i < 12; i++)
                {
                    var row = rows.Where(x => x.Key == startTime.Value.AddHours(i).Hour).FirstOrDefault();
                    if (row != null)
                    {
                        result.Add(SecondToHoursString(Convert.ToInt32(row.Key * 3600m), false), row.CountCase);
                    }
                    else
                    {
                        result.Add(SecondToHoursString(Convert.ToInt32(startTime.Value.AddHours(i).Hour * 3600m), false), 0);
                    }
                }
                //foreach (var dhr in rows)
                //{
                //    collection.Add(Convert.ToInt32(dhr.Key * 3600m), dhr.CountCase);
                //}

                //foreach (var item in collection)//.OrderByDescending(x=>x.Key)
                //{
                //    result.Add(SecondToHoursString(item.Key, false), item.Value);
                //}                

                //for (decimal i = 0; i < 12m; i += 1m)
                //{
                //    var dhr = (from o in rows
                //               where (Convert.ToDecimal(o.Key) == i)
                //               select o).FirstOrDefault();
                //    if (dhr == null)
                //    {
                //        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m)), 0);
                //    }
                //    else
                //    {
                //        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m)), dhr.CountCase);
                //    }
                //}
                return result;
            }
        }



        public static List<CaseCount> getLast12hrsCaseCounts(DateTime clientTime)
        {
            DateTime d = clientTime.AddHours(-12);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0);
            DateTime? endTime = new DateTime(clientTime.AddHours(-1).Year, clientTime.AddHours(-1).Month, clientTime.AddHours(-1).Day, clientTime.AddHours(-1).Hour, 59, 59);
            List<CaseCount> list = new List<CaseCount>();

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        orderby a.EventStop.Value ascending
                        select a;

                if (q != null)
                    list = q.ToList();

                return list;
            }
        }
        public static List<CaseCount> getLast12hrsCaseCountsSinceShiftStart(DateTime shiftStart, DateTime clientTime, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            DateTime? startTime = shiftStart;
            DateTime? endTime = new DateTime(clientTime.AddHours(-1).Year, clientTime.AddHours(-1).Month, clientTime.AddHours(-1).Day, clientTime.AddHours(-1).Hour, 59, 59);
            List<CaseCount> list = new List<CaseCount>();

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        orderby a.EventStop.Value ascending
                        select a;

                if (q != null)
                    list = q.ToList();

                return list;
            }
        }

        public static Dictionary<string, int> getHoursCountCase(DateTime? startTime, DateTime? endTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
                        
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop < endTime)
                        group a by a.EventStop.Value.Hour into g
                        select new { g.Key, CountCase = g.Average(o => o.CaseCount1) };

                var rows = q.ToList();
                for (decimal i = 0; i < 24m; i ++)
                {
                    var dhr = (from o in rows
                               where (Convert.ToDecimal(o.Key) == i)
                               select o).FirstOrDefault();
                    if (dhr == null)
                    {
                        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m),false), 0);
                    }
                    else
                    {
                        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m), false), (int)dhr.CountCase);
                    }
                }

                //半点模式
                //for (decimal i = 0; i < 24m; i += 0.5m)
                //{
                //    var dhr = (from o in rows
                //               where (Convert.ToDecimal(o.Key) == i)
                //               select o).FirstOrDefault();
                //    if (dhr == null)
                //    {
                //        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m)), 0);
                //    }
                //    else
                //    {
                //        result.Add(SecondToHoursString(Convert.ToInt32(i * 3600m)), dhr.CountCase);
                //    }
                //}
                return result;
            }
        }

        public static Dictionary<string, int> getDayCountCase(DateTime? startTime, DateTime? endTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop < endTime)
                        select a;

                var rows = q.ToList();

                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStop).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q1 = from o in rows
                             where o.EventStop.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                             select o;

                    var r = q1.ToList();
                    result.Add(r_starttime.AddDays(i).ToString(@"MM\/dd"), r.Sum(x=>x.CaseCount1));
                }

                return result;
            }
        }

        public static Dictionary<string, int> getWeekCountCase(DateTime? startTime, DateTime? endTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop < endTime)
                        select a;

                var records = q.ToList();
                var maxDate=q.Max(x=>x.EventStart);

                var q1 = from a in records
                         group a by FeedCommDashboard.DateTimeHelper.GetWeekStart(a.EventStop.Value, DayOfWeek.Monday) into g
                         select new { g.Key, Occurrence = g.Sum(x=>x.CaseCount1) };

                var rows = q1.ToList();
                foreach (var item in rows)
                {
                    DateTime weekEnd = new DateTime(item.Key.AddDays(7).Year, item.Key.AddDays(7).Month, item.Key.AddDays(7).Day);
                    if (weekEnd > maxDate) weekEnd = maxDate.Value;
                    int div = (int)weekEnd.Subtract(item.Key).TotalDays;
                    if (div == 0) div = 1;
                    result.Add(item.Key.ToString(@"MM\/dd\/yyyy"), Convert.ToInt32(item.Occurrence / div));
                }

                return result;
            }
        }

        public static Dictionary<string, int> getMonthCountCase(DateTime? startTime, DateTime? endTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop < endTime)
                        select a;

                var maxDate = q.Max(x => x.EventStart);
                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.EventStop).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.EventStop).Value);

                foreach (var item in DateTimeHelper.SplitMonths(r_starttime, r_endTime))
                {
                    var q1 = from o in rows
                             where o.EventStop.Value.ToString(@"MM\/yyyy") == item.ToString(@"MM\/yyyy")
                             select o;

                    var r = q1.ToList();
                    DateTime monthEnd = new DateTime(item.AddMonths(1).Year, item.AddMonths(1).Month, 1);
                    if (monthEnd > maxDate) monthEnd = maxDate.Value;
                    int div = (int)monthEnd.Subtract(item).TotalDays;
                    if (div == 0) div = 1;
                    result.Add(item.ToString("MMM,yyyy", new System.Globalization.CultureInfo("en-US")), Convert.ToInt32(r.Sum(x => x.CaseCount1) / div));
                }

                return result;
            }
        }

        public static void turnLights(bool on, string line)
        {
            DateTime now = convertToTimeZone(DateTime.Now, line);

            if (hasLightsOnFeature())
            {
                using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
                {
                    ProductionSchedule ps = new ProductionSchedule();

                    ps.EventTime = now;
                    ps.Client = Filter_Client;
                    ps.Line = line;
                    ps.LightsOn = on;

                    db.AddToProductionSchedules(ps);

                    int res = db.SaveChanges();
                }
            }
        }

        public static ProductionSchedule getLatestProductionSchedule(string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = (from o in db.ProductionSchedules
                         where o.Client == Filter_Client
                         && o.Line == line
                         orderby o.EventTime descending
                         select o).FirstOrDefault();

                if (q != null)
                    return q;
            }

            return null;

        }

        public static bool getLightsOn(string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = (from o in db.ProductionSchedules
                         where o.Client == Filter_Client
                         && o.Line == line
                         orderby o.EventTime descending
                         select o).FirstOrDefault();

                if (q != null)
                    return q.LightsOn;

                return false;
            }
        }

        public static DateTime? getShiftTime()
        {
            /*
            Guid UserId = (Membership.GetUser() != null ? (Guid)Membership.GetUser().ProviderUserKey : Guid.Empty);

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                DateTime? date = (from o in db.UserInfoSet
                                  where o.UserId == UserId
                                  select o.ShiftStart).FirstOrDefault();

                if (date != null)
                    return date;
            }

            DateTime d = DateTime.Now;

            return new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
             */

            return getShiftTime(Filter_Line);
        }

        /*
        public static DateTime? getShiftTime(string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                LineStatus ls = (from o in db.LineStatus
                                 where o.Client == Filter_Client
                                 && o.Line == line
                                 select o).FirstOrDefault();

                //2012-11-19 07:01:32.037

                DateTime now = DateTime.Now;
                TimeSpan span = new TimeSpan(6, 0, 00);

                //strDate = d.ToString("yyyy-M-dd") + " " + strDate + ".000";

                if (ls != null)
                {
                    TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById(ls.Timezone.Trim());
                    now = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Local, hwZone);

                    TimeSpan.TryParse(ls.ShiftStart, out span);
                }

                return new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, span.Seconds);
            }
        }
         */

        public static DateTime? getYesterdayStartShiftTime(string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                DateTime y = DateTime.Now.AddDays(-1);

                ProductionSchedule ps = (from o in db.ProductionSchedules
                                         where o.Client == Filter_Client
                                         && o.Line == line
                                         && o.LightsOn == true
                                         && o.EventTime.Day == y.Day && o.EventTime.Year == y.Year && o.EventTime.Month == y.Month
                                         orderby o.EventTime ascending
                                         select o).FirstOrDefault();

                LineStatus ls = (from o in db.LineStatus
                                 where o.Client == Filter_Client
                                 && o.Line == line
                                 orderby o.EventTime ascending
                                 select o).FirstOrDefault();

                //2012-11-19 07:01:32.037

                DateTime now = DateTime.Now;
                TimeSpan span = new TimeSpan(6, 0, 00);

                //strDate = d.ToString("yyyy-M-dd") + " " + strDate + ".000";

                if (ps != null && ls != null)
                {
                    TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById(ls.Timezone.Trim());
                    now = TimeZoneInfo.ConvertTime(ps.EventTime, TimeZoneInfo.Local, hwZone);
                    span = now.TimeOfDay;

                    //TimeSpan.TryParse(ps.EventTime, out span);
                }
                else
                {
                    return getUserShiftStart();
                }

                return new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, span.Seconds);
            }
        }

        public static DateTime? getYesterdayEndShiftTime(string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                DateTime y = DateTime.Now.AddDays(-1);

                ProductionSchedule ps = (from o in db.ProductionSchedules
                                         where o.Client == Filter_Client
                                         && o.Line == line
                                         && o.LightsOn == false
                                         && o.EventTime.Day == y.Day && o.EventTime.Year == y.Year && o.EventTime.Month == y.Month
                                         orderby o.EventTime descending
                                         select o).FirstOrDefault();

                LineStatus ls = (from o in db.LineStatus
                                 where o.Client == Filter_Client
                                 && o.Line == line
                                 orderby o.EventTime descending
                                 select o).FirstOrDefault();

                //2012-11-19 07:01:32.037

                DateTime now = DateTime.Now;
                TimeSpan span = new TimeSpan(6, 0, 00);

                //strDate = d.ToString("yyyy-M-dd") + " " + strDate + ".000";

                if (ps != null && ls != null)
                {
                    TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById(ls.Timezone.Trim());
                    now = TimeZoneInfo.ConvertTime(ps.EventTime, TimeZoneInfo.Local, hwZone);
                    span = now.TimeOfDay;

                    //TimeSpan.TryParse(ps.EventTime, out span);
                }
                else
                {
                    return getUserShiftStop();
                }

                return new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, span.Seconds);
            }
        }

        public static DateTime? getShiftTime(string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                ProductionSchedule ps = (from o in db.ProductionSchedules
                                         where o.Client == Filter_Client
                                         && o.Line == line
                                         orderby o.EventTime descending
                                         select o).FirstOrDefault();

                LineStatus ls = (from o in db.LineStatus
                                         where o.Client == Filter_Client
                                         && o.Line == line
                                         orderby o.EventTime descending
                                         select o).FirstOrDefault();

                //2012-11-19 07:01:32.037

                DateTime now = DateTime.Now;
                TimeSpan span = new TimeSpan(6, 0, 00);

                //strDate = d.ToString("yyyy-M-dd") + " " + strDate + ".000";

                if (ps != null && ls != null)
                {
                    TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById(ls.Timezone.Trim());
                    now = TimeZoneInfo.ConvertTime(ps.EventTime, TimeZoneInfo.Local, hwZone);
                    span = now.TimeOfDay;

                    //TimeSpan.TryParse(ps.EventTime, out span);
                }
                else
                {
                    return getUserShiftStart();
                }

                return new DateTime(now.Year, now.Month, now.Day, span.Hours, span.Minutes, span.Seconds);
            }
        }

        public static DateTime convertToTimeZone(DateTime dt, string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                LineStatus ls = (from o in db.LineStatus
                                 where o.Client == Filter_Client
                                 && o.Line == line
                                 select o).FirstOrDefault();

                //2012-11-19 07:01:32.037

                TimeSpan span = new TimeSpan(6, 0, 00);

                //strDate = d.ToString("yyyy-M-dd") + " " + strDate + ".000";

                if (ls != null)
                {
                    TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById(ls.Timezone.Trim());
                    dt = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local, hwZone);

                    TimeSpan.TryParse(ls.ShiftStart, out span);
                }


                return dt;

            }

        }

        public static decimal getRunningEfficiency_old(DateTime clientTime, out int numerator, out int denominator)
        {
            DateTime d;
            if (clientTime.Hour < 7)
            {
                d = clientTime.AddDays(-1);
            }
            else
            {
                d = clientTime;
            }
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, getShiftTime().Value.Hour, 0, 0);
            DateTime? endTime = clientTime;//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);
            //
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        select a.CaseCount1;

                int v = 0, offset = 5, par = 25;
                int total = Convert.ToInt32(Math.Abs(startTime.Value.Subtract(endTime.Value).TotalMinutes) / offset);
                if (q.Count() > 0) v = q.Sum(x => x);
                numerator = v;
                denominator = (par * total);
                return (decimal)v / (decimal)(par * total) * 100m;
            }
        }
        
        public static decimal getRunningLineEfficiency(DateTime clientTime, out decimal numerator, out decimal denominator, string line)
        {
            DateTime d;
            if (clientTime.Hour < 7)
            {
                d = clientTime.AddDays(-1);
            }
            else
            {
                d = clientTime;
            }

            DateTime? shiftTime = getUserShiftStart();
            DateTime? lightTime = shiftTime;

            if (DateTime.Now.Day == lightTime.Value.Day && getLightStatus(line) == true && hasLightsOnFeature() == true)
                shiftTime = getLightTime(line);

            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, shiftTime.Value.Hour, 0, 0);
            DateTime? endTime = clientTime;//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);

            return getRunningLineEfficiency(startTime.Value, endTime.Value, out numerator, out denominator, line);

        }

        public static decimal getRunningEfficiency(DateTime clientTime, out decimal numerator, out decimal denominator, string line)
        {
            DateTime d;
            if (clientTime.Hour < 7)
            {
                d = clientTime.AddDays(-1);
            }
            else
            {
                d = clientTime;
            }

            DateTime? shiftTime = getUserShiftStart();
            DateTime? lightTime = shiftTime;

            if (DateTime.Now.Day == lightTime.Value.Day && getLightStatus(line) == true && hasLightsOnFeature() == true)
                shiftTime = getLightTime(line);

            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, shiftTime.Value.Hour, 0, 0);
            DateTime? endTime = clientTime;//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);

            return getRunningEfficiency(startTime.Value, endTime.Value, out numerator, out denominator, line);

        }

        public static bool getLightStatus(string line)
        {
            using (DB db = new DB())
            {
                bool stat = (from o in db.ProductionSchedules
                             where o.Client == Filter_Client
                             && o.Line == line
                             orderby o.EventTime descending
                             select o.LightsOn).FirstOrDefault();
                return stat;
            }
        }

        public static bool getLightStatus(string line, List<ProductionSchedule> schedules)
        {
            bool stat = (from o in schedules
                         where o.Client == Filter_Client
                         && o.Line == line
                         orderby o.EventTime descending
                         select o.LightsOn).FirstOrDefault();
            return stat;
        }

        public static List<LineEfficiency> getLineEfficiencies(DateTime startDate, DateTime endDate)
        {
            using (DB db = new DB())
            {
                List<LineEfficiency> efficiencies = (from o in db.LineEfficiencies
                                                     where o.Client == Filter_Client
                                                       && (o.Time >= startDate)
                                                       && (o.Time <= endDate)
                                                     select o).ToList();

                return efficiencies;
            }
        }

        public static List<LineEfficiency> getLineEfficiencies(DateTime startDate, DateTime endDate, string line)
        {
            using (DB db = new DB())
            {
                List<LineEfficiency> efficiencies = (from o in db.LineEfficiencies
                                                     where o.Line == line
                                                     && o.Client == Filter_Client
                                                       && (o.Time >= startDate)
                                                       && (o.Time <= endDate)
                                                     select o).ToList();

                return efficiencies;
            }
        }


        public static decimal getRunningLineEfficiency(DateTime startDate, DateTime endDate, string line)
        {
            List<LineEfficiency> efficiencies = getLineEfficiencies(startDate, endDate, line);

            decimal numerator = efficiencies.Sum(o => o.Cases);//cases.Sum(o => o.Value);
            decimal denonimator = efficiencies.Sum(o => o.Estimate);//(decimal)cases.Count;

            if (denonimator == 0)
                denonimator++;

            decimal eff = numerator / denonimator;

            return eff;
        }

        public static decimal getRunningEfficiency(DateTime startDate, DateTime endDate, string line)
        {

            // rows = MillardDashboardHelper.getLast12hrsCaseCounts(startDate, endDate, div, Line, increments);
            // div = DiamondCrystaldashboardHelper.CountCaseGoals.MinuteCases * 60;
            // caseCounts = MillardDashboardHelper.getLast12hrsCaseCountList(clientTime, div, Line);

            Dictionary<CCEfficiency, decimal> cases = MillardDashboardHelper.getGraphCaseCounts(startDate, endDate, line);

            decimal numerator = cases.Sum(o => o.Key.CaseCount);//cases.Sum(o => o.Value);
            decimal denonimator = cases.Sum(o => o.Key.EstCases);//(decimal)cases.Count;

            if (denonimator == 0)
                denonimator++;

            decimal eff = numerator / denonimator;

            return eff;
        }

        public static decimal getRunningLineEfficiency(string line, List<LineEfficiency> efficiencies, out decimal numerator, out decimal denonimator)
        {

            // rows = MillardDashboardHelper.getLast12hrsCaseCounts(startDate, endDate, div, Line, increments);
            // div = DiamondCrystaldashboardHelper.CountCaseGoals.MinuteCases * 60;
            // caseCounts = MillardDashboardHelper.getLast12hrsCaseCountList(clientTime, div, Line);

            numerator = efficiencies.Sum(o => o.Cases);//cases.Sum(o => o.Value);
            denonimator = efficiencies.Sum(o => o.Estimate);//(decimal)cases.Count;

            if (denonimator == 0)
                denonimator++;

            decimal eff = numerator / denonimator;

            return eff;
        }

        public static decimal getRunningLineEfficiency(DateTime startDate, DateTime endDate, out decimal numerator, out decimal denonimator, string line)
        {

            // rows = MillardDashboardHelper.getLast12hrsCaseCounts(startDate, endDate, div, Line, increments);
            // div = DiamondCrystaldashboardHelper.CountCaseGoals.MinuteCases * 60;
            // caseCounts = MillardDashboardHelper.getLast12hrsCaseCountList(clientTime, div, Line);

            List<LineEfficiency> efficiencies = getLineEfficiencies(startDate, endDate, line);

            numerator = efficiencies.Sum(o => o.Cases);//cases.Sum(o => o.Value);
            denonimator = efficiencies.Sum(o => o.Estimate);//(decimal)cases.Count;

            if (denonimator == 0)
                denonimator++;

            decimal eff = numerator / denonimator;

            return eff;
        }

        public static decimal getRunningEfficiency(DateTime startDate, DateTime endDate, out decimal numerator, out decimal denonimator, string line)
        {

           // rows = MillardDashboardHelper.getLast12hrsCaseCounts(startDate, endDate, div, Line, increments);
           // div = DiamondCrystaldashboardHelper.CountCaseGoals.MinuteCases * 60;
           // caseCounts = MillardDashboardHelper.getLast12hrsCaseCountList(clientTime, div, Line);

            Dictionary<CCEfficiency, decimal> cases = MillardDashboardHelper.getGraphCaseCounts(startDate, endDate, line);

            numerator = cases.Sum(o => o.Key.CaseCount);//cases.Sum(o => o.Value);
            denonimator = cases.Sum(o => o.Key.EstCases);//(decimal)cases.Count;

            if (denonimator == 0)
                denonimator++;

            decimal eff = numerator / denonimator;

            return eff;
        }

        public static decimal getRunningEfficiencyFromEstimate(DateTime clientTime, out int numerator, out decimal denominator, string line)
        {
            //clientTime = new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, 10, 0, 0);//For testing purposes

            DateTime d;
            if (clientTime.Hour < 7)
            {
                d = clientTime.AddDays(-1);
            }
            else
            {
                d = clientTime;
            }
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, getShiftTime().Value.Hour, 0, 0);
            DateTime? endTime = clientTime;//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);
            //
            List<CaseCount> list = getLast12hrsCaseCountsSinceShiftStart(startTime.Value, clientTime, line);
            List<ThroughputHistory> thList = DCSDashboardDemoHelper.getThroughPutHistories(line);

            if (list.Count > 0)
            {
                int amount = list.Count();

                decimal total = 0;
                decimal estCount = getEstimatedCount(line) / 60;
                decimal totalMinutes = (decimal)endTime.Value.Subtract(startTime.Value).TotalMinutes;

                if (totalMinutes < 0)
                    totalMinutes = totalMinutes * -1;

                decimal est = totalMinutes * estCount;

                decimal avgEst = estCount;
                decimal avgCount = 0;

                int caseCounter = 0;

                if (est < 0)
                    est = est * -1;

                DateTime et = list.ElementAt(0).EventStop.Value;
                int hours = list.ElementAt(list.Count - 1).EventStop.Value.Subtract(list.ElementAt(0).EventStop.Value).Hours;

                for (int x = 0; x <= hours; x++)
                {
                    //if (list.ElementAt(x).EventStop.Value.Hour != et.Hour)
                    //    continue;

                    ThroughputHistory th = (from o in thList
                                            where (o.Date.DayOfYear == et.DayOfYear && o.Date.Year == et.Year && o.Date.Hour == et.Hour)
                                            orderby o.Date descending
                                            select o).FirstOrDefault();

                    decimal tmpEst = estCount * 60;

                    if (th != null)
                    {
                        Throughput tp = GetThroughputFromReference(th.ThroughputReference);

                        if (tp != null)
                            tmpEst = tp.PerHour;
                    }

                    List<CaseCount> cList = (from o in list
                                             where (o.EventStop.Value.Hour == et.Hour && o.EventStop.Value.DayOfYear == et.DayOfYear && o.EventStop.Value.Year == et.Year && o.EventStop.Value.Minute >= et.Minute)
                                             orderby o.EventStop.Value.Minute ascending
                                             select o).ToList();
                    decimal count = 0;

                    if (cList.Count > 0)
                    {
                        CaseCount firstCase = cList.ElementAt(0);
                        CaseCount lastCase = cList.ElementAt(cList.Count - 1);

                        if (firstCase != null && lastCase != null)
                        {
                            count = lastCase.CaseCount1 - firstCase.CaseCount1;
                            caseCounter++;
                        }
                    }

                    // if (diff < 0)
                    //                        diff = diff * -1;//Make positive number

                    decimal diff = count / (tmpEst != 0 ? tmpEst : 1);

                    total += count;//diff;//diff / tmpEst;

                    avgEst += tmpEst;
                    avgCount++;

                    et = et.AddHours(1d);
                }

                if (avgEst < 0)
                    avgEst = avgEst * -1;

                avgEst = avgEst / avgCount;
                //avgEst = avgEst / 60;
                //avgEst = avgEst * totalMinutes;


                numerator = Convert.ToInt32(total / caseCounter);
                denominator = avgEst;

                if (denominator == 0)
                    denominator = 1;

                decimal result = (decimal)(numerator / (decimal)denominator) * 100m;

                return result;
            }
            else
            {
                numerator = 0;
                denominator = 1;

                return 0;
            }

        }
        private static int getLatestThroughput(string line)
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

        public static int GetThroughputIdFromReference(EntityReference reference)
        {
            if (reference == null)
                return 0;

            EntityKeyMember[] keys = reference.EntityKey.EntityKeyValues;
            int id = 0;

            if (keys[0].Key.ToLower() == "id")
                id = (int)keys[0].Value;

            return id;
        }

        public static Throughput GetThroughputFromReference(EntityReference reference)
        {
            EntityKeyMember[] keys = reference.EntityKey.EntityKeyValues;
            int id = 0;

            if (keys[0].Key.ToLower() == "id")
                id = (int)keys[0].Value;

            return DCSDashboardDemoHelper.GetThroughPut(id);
        }

        private static int getThroughput(DateTime date, string line)
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

        private static decimal getEstimatedCount(DateTime date, string line)
        {
            decimal results = Convert.ToDecimal(getThroughput(date, line));

            if (results > -1)
                return results;
            else
            {

                Guid UserId = (Membership.GetUser() != null ? (Guid)Membership.GetUser().ProviderUserKey : Guid.Empty);

                using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
                {
                    UserInfo q = (from o in db.UserInfoSet
                                  where o.UserId == UserId
                                  select o).FirstOrDefault();




                    if (q != null)
                        results = (decimal)q.EstimatedOutput;
                }
            }

            return results;
        }

        private static decimal getEstimatedCount(string line)
        {
            decimal results = (decimal)getLatestThroughput(line);

            if (results > -1)
                return results;
            else
            {

                Guid UserId = (Membership.GetUser() != null ? (Guid)Membership.GetUser().ProviderUserKey : Guid.Empty);

                using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
                {
                    UserInfo q = (from o in db.UserInfoSet
                                  where o.UserId == UserId
                                  select o).FirstOrDefault();




                    if (q != null)
                        results = (decimal)q.EstimatedOutput;
                }
            }

            return results;
        }

        public static decimal getYesterdayEffiency()
        {
            DateTime d = DateTime.Now.AddDays(-1);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
            DateTime? endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 59, 59);
            //
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        select a.CaseCount1;

                /*int v = 0, offset = 5, par = 25;
                int total = Convert.ToInt32(Math.Abs(startTime.Value.Subtract(DateTime.Now).TotalMinutes) / offset);
                if (q.Count() > 0) v = q.Sum(x => x);*/
                if (q.Count() == 0) return 0m;
                return (decimal)q.Sum(x => x) / (CountCaseGoals.MinuteCases * 60 * 24) * 100m;
            }
        }

        public static decimal getYesterdayLineEfficiency(string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            DateTime d = DateTime.Now.AddDays(-1);
            DateTime? y = getYesterdaysLightOffTime(line);
            DateTime? shiftTime = getUserShiftStart();
            DateTime? lightTime = shiftTime;

            if (getLightStatus(line) == true && hasLightsOnFeature() == true)
            {
                lightTime = getYesterdaysLightOnTime(line);

                if (DateTime.Now.Day == lightTime.Value.Day)
                    shiftTime = lightTime;
            }

            //DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, (DateTime.Now.DayOfWeek == DayOfWeek.Monday ? getLightTime(line) : getShiftTime(line)).Value.Hour, 0, 0);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, shiftTime.Value.Hour, 0, 0);

            if (!y.HasValue)
                y = startTime.Value.AddDays(1);

            DateTime? endTime = new DateTime(d.Year, d.Month, d.Day, y.Value.Hour, y.Value.Minute, y.Value.Second);
            //DateTime? endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 59, 59);//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);

            TimeSpan ss = startTime.Value.TimeOfDay;
            TimeSpan es = endTime.Value.TimeOfDay;

            if (ss.Hours == es.Hours && ss.Minutes == es.Minutes)
            {
                endTime = startTime.Value.AddDays(1);
            }

            return getRunningLineEfficiency(startTime.Value, endTime.Value, line);

        }

        public static decimal getYesterdayEfficiency(string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            DateTime d = DateTime.Now.AddDays(-1);
            DateTime? y = getYesterdaysLightOffTime(line);
            DateTime? shiftTime = getUserShiftStart();
            DateTime? lightTime = shiftTime;

            if (getLightStatus(line) == true && hasLightsOnFeature() == true)
            {
                lightTime = getYesterdaysLightOnTime(line);

                if(DateTime.Now.Day == lightTime.Value.Day)
                    shiftTime = lightTime;
            }

            //DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, (DateTime.Now.DayOfWeek == DayOfWeek.Monday ? getLightTime(line) : getShiftTime(line)).Value.Hour, 0, 0);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, shiftTime.Value.Hour, 0, 0);

            if (!y.HasValue)
                y = startTime.Value.AddDays(1);

            DateTime? endTime = new DateTime(d.Year, d.Month, d.Day, y.Value.Hour, y.Value.Minute, y.Value.Second);
            //DateTime? endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 59, 59);//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);

            TimeSpan ss = startTime.Value.TimeOfDay;
            TimeSpan es = endTime.Value.TimeOfDay;

            if (ss.Hours == es.Hours && ss.Minutes == es.Minutes)
            {
                endTime = startTime.Value.AddDays(1);
            }

            return getRunningEfficiency(startTime.Value, endTime.Value, line);

        }

        public static DateTime? getUserShiftStart()
        {
            if (Membership.GetUser() == null)
                return null;

            Guid UserId = (Guid)Membership.GetUser().ProviderUserKey;

            using (DB db = new DB())
            {
                UserInfo info = (from o in db.UserInfoSet
                                 where o.UserId == UserId
                                 select o).FirstOrDefault();

                if (info == null)
                {
                    DateTime d = DateTime.Now;
                    DateTime t = DateTime.Now.AddDays(1);

                    info = new UserInfo();
                    info.UserId = UserId;
                    info.UseLights = false;
                    info.ShiftStart = new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
                    info.ShiftStop = new DateTime(t.Year, t.Month, t.Day, 6, 0, 0);
                    info.EffChartEnabled = false;
                    info.EstimatedOutput = 0;

                    db.AddToUserInfoSet(info);
                    db.SaveChanges();

                    return info.ShiftStart;
                }
                else
                {
                    return info.ShiftStart;
                }
            }

        }

        public static DateTime? getUserShiftStop()
        {
            if (Membership.GetUser() == null)
                return null;

            Guid UserId = (Guid)Membership.GetUser().ProviderUserKey;

            using (DB db = new DB())
            {
                UserInfo info = (from o in db.UserInfoSet
                                 where o.UserId == UserId
                                 select o).FirstOrDefault();

                if (info == null)
                {
                    DateTime d = DateTime.Now;
                    DateTime t = DateTime.Now.AddDays(1);

                    info = new UserInfo();
                    info.UserId = UserId;
                    info.UseLights = false;
                    info.ShiftStart = new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
                    info.ShiftStop = new DateTime(t.Year, t.Month, t.Day, 6, 0, 0);
                    info.EffChartEnabled = false;
                    info.EstimatedOutput = 0;

                    db.AddToUserInfoSet(info);
                    db.SaveChanges();

                    return info.ShiftStop;
                }
                else
                {
                    return info.ShiftStop;
                }
            }

        }

        public static bool hasLightsOnFeature()
        {
            if (Membership.GetUser() == null)
                return false;

            Guid UserId = (Guid)Membership.GetUser().ProviderUserKey;

            using (DB db = new DB())
            {
                UserInfo info = (from o in db.UserInfoSet
                                 where o.UserId == UserId
                                 select o).FirstOrDefault();

                if (info == null)
                {
                    DateTime d = DateTime.Now;

                    info = new UserInfo();
                    info.UserId = UserId;
                    info.UseLights = false;
                    info.ShiftStart = new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
                    info.EffChartEnabled = false;
                    info.EstimatedOutput = 0;

                    db.AddToUserInfoSet(info);
                    db.SaveChanges();

                    return false;
                }
                else
                {
                    return info.UseLights;
                }
            }

        }

        public static DateTime? getYesterdaysLightOnTime(string line)
        {
            using (DB db = new DB())
            {
                if (hasLightsOnFeature())
                {
                    DateTime y = DateTime.Now.AddDays(-1);

                    DateTime? date = (from o in db.ProductionSchedules
                                      where o.Client == Filter_Client
                                      && o.Line == line
                                      && o.EventTime.Day == y.Day && o.EventTime.Year == y.Year && o.EventTime.Month == y.Month
                                      && o.LightsOn == true
                                      orderby o.EventTime ascending
                                      select o.EventTime).FirstOrDefault();

                    List<object> objs = new List<object>();

                    if (date != null)
                        return date;
                }

            }

            DateTime? d = getYesterdayStartShiftTime(line);

            return d;
        }

        public static DateTime? getYesterdaysLightOffTime(string line)
        {
            using (DB db = new DB())
            {
                if (hasLightsOnFeature())
                {
                    DateTime y = DateTime.Now.AddDays(-1);

                    DateTime? date = (from o in db.ProductionSchedules
                                      where o.Client == Filter_Client
                                      && o.Line == line
                                      && o.EventTime.Day == y.Day && o.EventTime.Year == y.Year && o.EventTime.Month == y.Month
                                      && o.LightsOn == false
                                      orderby o.EventTime descending
                                      select o.EventTime).FirstOrDefault();

                    List<object> objs = new List<object>();

                    if (date != null)
                        return date;
                }

            }

            DateTime? d = getYesterdayEndShiftTime(line);

            return d;
        }

        public static DateTime? getLightTime(string line)
        {
            using (DB db = new DB())
            {
                if (hasLightsOnFeature())
                {
                    DateTime? date = (from o in db.ProductionSchedules
                                      where o.Client == Filter_Client
                                      && o.Line == line
                                      orderby o.EventTime descending
                                      select o.EventTime).FirstOrDefault();

                    List<object> objs = new List<object>();

                    if (date != null)
                        return date;
                }

            }

            DateTime? d = getUserShiftStart();

            return d;
        }

        public static DateTime? getLightTime(string line, List<ProductionSchedule> schedules, bool hasLightFeature = false)
        {
            DateTime? date = null;

            if (hasLightFeature)
            {
                date = (from o in schedules
                                  where o.Client == Filter_Client
                                  && o.Line == line
                                  orderby o.EventTime descending
                                  select o.EventTime).FirstOrDefault();
            }

            return date;
        }
        
        public static decimal getYesterdayEfficiency_old(string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            DateTime d = DateTime.Now.AddDays(-1);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, getShiftTime().Value.Hour, 0, 0);
            DateTime? endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 59, 59);//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);
            //
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        select a;


                List<CaseCount> list = new List<CaseCount>();

                    if(q != null)
                        list = q.ToList();

                int amount = list.Count();

                int total = 0;
                decimal estCount = getEstimatedCount(line) / 60;
                decimal totalMinutes = Convert.ToDecimal(startTime.Value.Subtract(endTime.Value).TotalMinutes);

                decimal est = totalMinutes * estCount;

                if (est < 0)
                    est = est * -1;

                for (int x = 0; x < list.Count; x++)
                {
                    if (x > 0)
                    {
                        int first = list.ElementAt(x - 1).CaseCount1;
                        int sec = list.ElementAt(x).CaseCount1;

                        int diff = first - sec;

                        if (diff < 0)
                            diff = diff * -1;//Make positive number

                        total += diff;
                    }
                }

                decimal numerator = total;
                decimal denominator = est;

                if (denominator == 0)
                    denominator++;

                decimal result = (decimal)(numerator / denominator) * 100m;

                return result;
            }
        }
        #endregion
    }
}
