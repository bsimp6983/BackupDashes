using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DowntimeCollection_Demo;

namespace DCSDemoData
{
    public static partial class MillardDashboardTVHelper
    {
        private const string Filter_Client = "sage";
        private const string ExcludeLevel1 = "*#&*@(@$(#$@&@$@$";//",Non-Downtime Machine Stops,";
        private const string ExcludeLevel2 = "*#&*@(@$(#$@&@$@$";
        private const string ExcludeLevel3 = "*#&*@(@$(#$@&@$@$";
        private const string Filter_Line = "company-demo";

        public static DowntimeReason getReason(int reasonCodeId)
        {
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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


            using (DB db = new DB())
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

        public static List<SpecialReportRow> TopEventsGroupByLevel3(DateTime? startTime, DateTime? endTime, string level1, int takeCount, bool orderByMinutes)
        {
            using (DB db = new DB())
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
                         group c by new { c.b.Level3, c.b.Level2, c.b.Level1 } into g
                         orderby g.Key.Level1 ascending
                         select new SpecialReportRow { Title = (!string.IsNullOrEmpty(g.Key.Level3) ? g.Key.Level3 : (!string.IsNullOrEmpty(g.Key.Level2) ? g.Key.Level2 : g.Key.Level1)), Minutes = g.Sum(o => o.a.Minutes), Occurences = g.Count(), appInfo = g.Min(o => o.b.ID) };//因为level3一定是唯一的，所以可以直接取一个MinId


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
            using (DB db = new DB())
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
                        orderby (orderByMinutes ? b.Minutes : b.Occurences) descending, c.Level1 ascending, c.Level2 ascending, c.Level3 ascending
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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

        public static List<SpecialReportRow> YearReport(DateTime? startTime, DateTime? endTime, string level1)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRows(startTime, endTime, level1);

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

        private static string SecondToHoursString(int seconds)
        {
            int h = (seconds - seconds % 3600) / 3600;//时间
            int m = (seconds % 3600 - (seconds % 3600) % 60) / 60;//分钟
            if (h > 12)
            {
                return string.Format("{0}:{1:00} PM", h - 12, m);
            }
            else
            {
                return string.Format("{0}:{1:00} AM", h, m);
            }
        }

        public static object DetailsByLine(DateTime? startTime, DateTime? endTime, string level1)
        {
            using (DB db = new DB())
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

            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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

            using (DB db = new DB())
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
            using (DB db = new DB())
            {
                var q = from o in db.GoalSet
                        where o.StartTime <= startTime && o.EndTime >= endTime
                        && o.CLIENT == Filter_Client
                        && o.LINE == Filter_Line
                        select o;
                int? totalOcc = null;
                try
                {
                    totalOcc = q.Sum(o => o.Occuring);
                }
                catch (Exception)
                {

                    //throw;
                }

                decimal? totalTime = null;
                try
                {
                    totalTime = q.Sum(o => o.Dowmtime);
                }
                catch (Exception)
                {

                    // throw;
                }

                if (!totalOcc.HasValue) totalOcc = 0;
                if (!totalTime.HasValue) totalTime = 0;

                decimal totalDays = 0;//Math.Abs(Convert.ToDecimal( endTime.Subtract(startTime).TotalDays));

                foreach (var item in q.ToList())
                {
                    totalDays += Convert.ToDecimal(item.EndTime.Subtract(item.StartTime).TotalDays);
                }

                if (totalDays == 0) totalDays = 1m;
                GoalReportRow grr = new GoalReportRow();
                grr.Downtimes.Hour = totalTime.Value / (totalDays * 24);
                grr.Downtimes.Day = totalTime.Value / totalDays;
                grr.Downtimes.Week = totalTime.Value / (totalDays / 7);
                grr.Downtimes.Month = totalTime.Value / (totalDays / 30);
                grr.Downtimes.Year = totalTime.Value / (totalDays / 365);

                grr.Occurences.Hour = Convert.ToInt32(totalOcc.Value / (totalDays * 24));
                grr.Occurences.Day = Convert.ToInt32(totalOcc.Value / totalDays);
                grr.Occurences.Week = Convert.ToInt32(totalOcc.Value / (totalDays / 7));
                grr.Occurences.Month = Convert.ToInt32(totalOcc.Value / (totalDays / 30));
                grr.Occurences.Year = Convert.ToInt32(totalOcc.Value / (totalDays / 365));

                return grr;
            }
        }

        public static List<ClientGoalRow> GetGoals()
        {
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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

                foreach (var item in DateTimeHelper.SplitMonths(r_starttime, r_endTime))
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
                        group a by a.EventStart.Value.Hour into g
                        select new DowntimeHistoryRow { week = g.Key, MinutesSum = g.Sum(o => o.Minutes.Value) }
                        ;
                result = q.ToList();

                List<DowntimeHistoryRow> result2 = new List<DowntimeHistoryRow>();
                for (decimal i = 0; i < 24m; i += 0.5m)
                {
                    DowntimeHistoryRow dhr = (from o in result
                                              where (Convert.ToDecimal(o.week) == i)
                                              select o).FirstOrDefault();
                    if (dhr == null)
                    {
                        result2.Add(new DowntimeHistoryRow { Level3 = SecondToHoursString(Convert.ToInt32(i * 3600m)), MinutesSum = 0 });
                    }
                    else
                    {
                        dhr.Level3 = SecondToHoursString(Convert.ToInt32(i * 3600m));
                        result2.Add(dhr);
                    }
                }
                return result2;

            }
        }

        public static List<DowntimeHistoryRow> DayDowntimeHistory(DateTime startTime, DateTime endTime, string level3)
        {
            List<DowntimeHistoryRow> result = new List<DowntimeHistoryRow>();
            using (DB db = new DB())
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
            using (DB db = new DB())
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

                            group a by new { b.ID, b.Level1, b.Level2, b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level1, g.Key.Level2, g.Key.Level3, Occurrences = g.Count() }
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
            using (DB db = new DB())
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

                            group a by new { b.ID, b.Level1, b.Level2, b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level1, g.Key.Level2, g.Key.Level3, Occurrences = g.Count() }
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
            using (DB db = new DB())
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

                            group a by new { b.ID, b.Level1, b.Level2, b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level1, g.Key.Level2, g.Key.Level3, Occurrences = g.Count() }
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
            using (DB db = new DB())
            {
                var q = from a in db.DowntimeDataSet
                        where a.ReasonCodeID == levelId
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        group a by a.EventStart.Value.Hour into g
                        select new { g.Key, Occurrence = g.Count() };

                var rows = q.ToList();

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
                }
                return result;
            }
        }

        private static Dictionary<string, int> getLevelWeekOccurrenceData(DateTime? startTime, DateTime? endTime, int levelId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB())
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
            using (DB db = new DB())
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

                            group a by new { b.ID, b.Level1, b.Level2, b.Level3 } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.ID, g.Key.Level3, g.Key.Level2, g.Key.Level1, minutes = g.Sum(o => o.Minutes) }
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
            using (DB db = new DB())
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
            using (DB db = new DB())
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
    }
}