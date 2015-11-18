using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace DowntimeCollection_Demo.Classes
{
    public class DBHelper
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

        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DB"].ToString();
        }

        public static string GetConnectionString(string client = "")
        {
            return ConfigurationManager.ConnectionStrings["DB"].ToString();
        }

        public static List<AscommStatus> GetAscommStatuses()
        {
            using (DB db = new DB(GetConnectionString()))
            {
                return (from o in db.AscommStatuses
                        select o).ToList();
            }
        }

        public static AscommStatus GetAscommStatus(string client, string line)
        {
            using (DB db = new DB(GetConnectionString()))
            {
                AscommStatus status =  (from o in db.AscommStatuses
                                        where o.Client == client
                                        && o.Line == line
                                    select o).FirstOrDefault();

                if (status == null)
                {
                    status = new AscommStatus();
                    status.Client = Filter_Client;
                    status.LastPing = DateTime.Now;
                    status.Line = line;
                    status.Status = false;

                    db.AddToAscommStatuses(status);

                    db.SaveChanges();
                }

                return status;
            }
        }

        public static void UpdateAscommPing(string client, DateTime date, string line, bool s)
        {
            using (DB db = new DB(GetConnectionString()))
            {
                AscommStatus status = (from o in db.AscommStatuses
                                       where o.Client == client
                                       && o.Line == line
                                       select o).FirstOrDefault();

                if (status == null)
                {
                    status = new AscommStatus();

                    status.Client = client;
                    status.LastPing = DateTime.Now;
                    status.Status = s;
                    status.Line = line;

                    db.AddToAscommStatuses(status);
                }
                else
                {
                    status.Status = s;
                    status.LastPing = date;
                }

                db.SaveChanges();
            }
        }

        public static List<DowntimeData> GetDowntimesByDetail(int detailId, bool includeChildren = false)
        {
            using(DB db = new DB(GetConnectionString(Filter_Client)))
            {
                List<DowntimeData> result = (from o in db.vw_DowntimeDetails
                                             where o.DetailId == detailId
                                             && (includeChildren == false || o.ParentId == detailId)
                                             select (from b in db.DowntimeDataSet
                                                     where b.ID == o.DowntimeId
                                                     select b).FirstOrDefault()
                                             ).ToList();
                return result;
            }
        }

        public static List<int> GetDowntimesIdsByDetail(int detailId, bool includeChildren = false)
        {
            List<DowntimeData> data = GetDowntimesByDetail(detailId, includeChildren);

            List<int> results = new List<int>();

            foreach (DowntimeData dt in data)
            {
                if (!results.Contains(dt.ID))
                    results.Add(dt.ID);
            }

            return results;
        }

        public static List<vw_DowntimeDetails> GetDetails(DateTime? startTime, DateTime? endTime, string line, string key, bool orderByMinutes = false)
        {
            using (DB db = new DB(GetConnectionString(Filter_Client)))
            {
                key = key.ToLower();

                List<vw_DowntimeDetails> results = (from o in db.vw_DowntimeDetails
                                                    where o.Key.ToLower() == key
                                                     && (o.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                                     && (o.Line == line || string.IsNullOrEmpty(line))
                                                     && (!startTime.HasValue || o.EventStart >= startTime.Value)
                                                     && (!endTime.HasValue || o.EventStart < endTime.Value)
                                                    select o).ToList();

                return results;
            }
        }

        public static string GetDetailKey(int id)
        {
            using (DB db = new DB(GetConnectionString(Filter_Client)))
            {
                Detail results = (from o in db.Details
                                                    where o.Id == id
                                                    select o).FirstOrDefault();

                if (results != null)
                    return results.Key;
                else
                    return null;
            }
        }

        public static List<DowntimeData> DetDowntimeByDetail(DateTime? startTime, DateTime? endTime, string line, string key, bool orderByMinutes = false)
        {
            using (DB db = new DB(GetConnectionString(Filter_Client)))
            {
                key = key.ToLower();

                List<DowntimeData> results = (from o in db.vw_DowntimeDetails
                                                    join b in db.DowntimeDataSet
                                                    on o.DowntimeId equals b.ID
                                                    where o.Key.ToLower() == key
                                                     && (o.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                                     && (o.Line == line || string.IsNullOrEmpty(line))
                                                     && (!startTime.HasValue || o.EventStart >= startTime.Value)
                                                     && (!endTime.HasValue || o.EventStart < endTime.Value)
                                                    select b).ToList();

                return results;
            }
        }

        public static List<LossBucketsRow> LossBuckets(DateTime? startTime, DateTime? endTime, string line, string key, bool orderByMinutes)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            key = key.ToLower();

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                if (line != "all")
                {
                    var q = from x in
                                (
                                    from g in
                                        (from a in db.vw_DowntimeDetails
                                         where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                         && (a.Line == line || string.IsNullOrEmpty(line))
                                         && (!startTime.HasValue || a.EventStart >= startTime.Value)
                                         && (!endTime.HasValue || a.EventStart < endTime.Value)
                                         && a.Key.ToLower() == key
                                         select new { a.DowntimeId, a.Minutes, a.Key, a.Value })
                                    group g by g.Value into p
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
                else
                {
                    var q = from x in
                                (
                                    from g in
                                        (from a in db.vw_DowntimeDetails
                                         where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                         //&& (a.Line == line || string.IsNullOrEmpty(line))
                                         && (!startTime.HasValue || a.EventStart >= startTime.Value)
                                         && (!endTime.HasValue || a.EventStart < endTime.Value)
                                         && a.Key.ToLower() == key
                                         select new { a.DowntimeId, a.Minutes, a.Key, a.Value })
                                    group g by g.Value into p
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
        }

        public static List<CommentRow> Comments(DateTime? startTime, DateTime? endTime, int reasonCodeId, int detailId, string value, string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                if (line != "all")
                {
                    var q = from a in db.vw_DowntimeDetails
                            where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (!startTime.HasValue || a.EventStart >= startTime.Value)
                            && (!endTime.HasValue || a.EventStart < endTime.Value)
                            && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && a.DetailId == detailId
                            && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                            && (a.Line == line || string.IsNullOrEmpty(line))
                            select new CommentRow { Line = a.Line, Minutes = a.Minutes, Client = a.Client, Comment = a.Comment, Level1 = a.Value, ReasonCodeId = a.ReasonCodeID, EventStart = a.EventStart, EventStop = a.EventStop }
                    ;
                    return q.ToList();
                }
                else
                {
                    var q = from a in db.vw_DowntimeDetails
                            where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (!startTime.HasValue || a.EventStart >= startTime.Value)
                            && (!endTime.HasValue || a.EventStart < endTime.Value)
                            && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && a.DetailId == detailId
                            && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                            //&& (a.Line == line || string.IsNullOrEmpty(line))
                            select new CommentRow { Line = a.Line, Minutes = a.Minutes, Client = a.Client, Comment = a.Comment, Level1 = a.Value, ReasonCodeId = a.ReasonCodeID, EventStart = a.EventStart, EventStop = a.EventStop }
                    ;
                    return q.ToList();
                }
            }
        }

        public static List<TopEventsRow> TopEvents(DateTime? startTime, DateTime? endTime, int detailId, string value, int takeCount, string line, bool orderByMinutes)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                if (!string.IsNullOrEmpty(value))
                    value = value.ToLower();

                if (line != "all")
                {
                    var q = from b in
                                (from a in db.vw_DowntimeDetails
                                 where (!startTime.HasValue || a.EventStart >= startTime.Value)
                                 && (!endTime.HasValue || a.EventStart < endTime.Value)
                                 && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                 && (a.Line == line || string.IsNullOrEmpty(line))
                                 && a.DetailId == detailId
                                 && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                                 group a by a.Value into g
                                 select new { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), g.Key })
                            orderby (orderByMinutes ? b.Minutes : b.Occurences) descending
                            select new TopEventsRow { Client = Filter_Client, Level1 = b.Key, Minutes = b.Minutes, Occurences = b.Occurences}
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
                else
                {
                    var q = from b in
                                (from a in db.vw_DowntimeDetails
                                 where (!startTime.HasValue || a.EventStart >= startTime.Value)
                                 && (!endTime.HasValue || a.EventStart < endTime.Value)
                                 && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                && a.DetailId == detailId
                                && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                                 //&& (a.Line == line || string.IsNullOrEmpty(line))
                                 //&& (a.DetailId == detailId && a.Value.ToLower() == value)
                                 group a by a.Value into g
                                 select new { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), g.Key })
                            orderby (orderByMinutes ? b.Minutes : b.Occurences) descending
                            select new TopEventsRow { Client = Filter_Client, Level1 = b.Key, Minutes = b.Minutes, Occurences = b.Occurences }
                          ;
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
        }

        public static List<AllOccurrenceHistoryRow> getOccurrenceHistory(DateTime? startTime, DateTime? endTime, int detailId, string value, string type, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            List<AllOccurrenceHistoryRow> result = new List<AllOccurrenceHistoryRow>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from o in
                            (
                            from a in db.vw_DowntimeDetails
                            where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (!startTime.HasValue || a.EventStart >= startTime)
                            && (!endTime.HasValue || a.EventStart < endTime)
                            && (a.Line == line || string.IsNullOrEmpty(line))
                            && a.DetailId == detailId
                            && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                            group a by new { a.DowntimeId, a.Value } into g
                            //where !string.IsNullOrEmpty(g.Key.Level3)
                            select new { g.Key.DowntimeId, g.Key.Value, Occurrences = g.Count() }
                            )
                        where o.Occurrences > 0
                        orderby o.Occurrences descending
                        select o;

                foreach (var item in q.Take(5).ToList())
                {
                    AllOccurrenceHistoryRow aohr = new AllOccurrenceHistoryRow();

                    aohr.Level3 = item.Value;

                    switch (type.ToLower())
                    {
                        case "hours":
                            aohr.Datas = getLevelHoursOccurrenceData(startTime, endTime, detailId, value, line);
                            break;
                        case "day":
                            aohr.Datas = getLevelDayOccurrenceData(startTime, endTime, detailId, value, line);
                            break;
                        case "week":
                            aohr.Datas = getLevelWeekOccurrenceData(startTime, endTime, detailId, value, line);
                            break;
                        case "month":
                            aohr.Datas = getLevelMonthOccurrenceData(startTime, endTime, detailId, value, line);
                            break;
                        case "year":
                            aohr.Datas = getLevelYearOccurrenceData(startTime, endTime, detailId, value, line);
                            break;
                        default:
                            aohr.Datas = getLevelDayOccurrenceData(startTime, endTime, detailId, value, line);
                            break;
                    }
                    result.Add(aohr);

                }
            }
            return result;
        }

        private static Dictionary<string, int> getLevelHoursOccurrenceData(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.vw_DowntimeDetails
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && a.DetailId == detailId
                        && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
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

        private static Dictionary<string, int> getLevelDayOccurrenceData(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.vw_DowntimeDetails
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && a.DetailId == detailId
                        && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
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

        private static Dictionary<string, int> getLevelWeekOccurrenceData(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.vw_DowntimeDetails
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && a.DetailId == detailId
                        && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                        select a;

                var q1 = from a in q.ToList()
                         group a by DowntimeCollection_Demo.DateTimeHelper.GetWeekStart(a.EventStart.Value, DayOfWeek.Monday) into g
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

        private static Dictionary<string, int> getLevelMonthOccurrenceData(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.vw_DowntimeDetails
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && a.DetailId == detailId
                        && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
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

        private static Dictionary<string, int> getLevelYearOccurrenceData(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            Dictionary<string, int> result = new Dictionary<string, int>();
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.vw_DowntimeDetails
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStart >= startTime)
                        && (!endTime.HasValue || a.EventStart < endTime)
                        && a.DetailId == detailId
                        && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
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

        public static GoalReportRow GetGoals(DateTime? startTime, DateTime? endTime, int detailId, string value, int levelId, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                if(!string.IsNullOrEmpty(value))
                    value = value.ToLower();

                if (line != "all")
                {
                    var q = from a in db.vw_DowntimeDetails
                            where (!startTime.HasValue || a.EventStart >= startTime.Value)
                            && (!endTime.HasValue || a.EventStart < endTime.Value)
                            && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (a.Line == line || string.IsNullOrEmpty(line))
                            && (a.ReasonCodeID == levelId || levelId <= 0)
                            && a.DetailId == detailId
                            && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                            select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.DowntimeId, Level1 = a.Value, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                    ;


                    DateTime? sd = q.Min(o => o.EventStart);
                    DateTime? ed = q.Max(o => o.EventStart);

                    if (!sd.HasValue || !ed.HasValue) return new GoalReportRow { Downtimes = new GoalDowntimeRow(), Occurences = new GoalOccurencesRow() };
                    GoalReportRow grr = GetGoals(sd.Value, ed.Value);

                    return (grr == null ? new GoalReportRow { Downtimes = new GoalDowntimeRow(), Occurences = new GoalOccurencesRow() } : grr);
                }
                else
                {
                    var q = from a in db.vw_DowntimeDetails
                            where (!startTime.HasValue || a.EventStart >= startTime.Value)
                            && (!endTime.HasValue || a.EventStart < endTime.Value)
                            && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            //&& (a.Line == line || string.IsNullOrEmpty(line))
                            && (a.ReasonCodeID == levelId || levelId <= 0)
                            && a.DetailId == detailId
                            && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                            //&& (a.DetailId == detailId && a.Value.ToLower() == value)
                            select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.DowntimeId, Level1 = a.Value, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                    ;


                    DateTime? sd = q.Min(o => o.EventStart);
                    DateTime? ed = q.Max(o => o.EventStart);

                    if (!sd.HasValue || !ed.HasValue) return new GoalReportRow { Downtimes = new GoalDowntimeRow(), Occurences = new GoalOccurencesRow() };
                    GoalReportRow grr = GetGoals(sd.Value, ed.Value);

                    return (grr == null ? new GoalReportRow { Downtimes = new GoalDowntimeRow(), Occurences = new GoalOccurencesRow() } : grr);

                }

            }
        }


        public static GoalReportRow GetGoals(DateTime startTime, DateTime endTime)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
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

        public static List<EventRowWithAllColumns> GetEventRows(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.vw_DowntimeDetails
                        where (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && a.DetailId == detailId
                        && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                        select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.DowntimeId, Level1 = a.Value, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                ;
                List<EventRowWithAllColumns> rows = q.ToList();
                return rows;

            }
        }

        public static List<EventRowWithAllColumns> GetEventRowsByLine(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                if (!string.IsNullOrEmpty(value))
                    value = value.ToLower();

                if (line != "all")
                {
                    var q = from a in db.vw_DowntimeDetails
                            where (!startTime.HasValue || a.EventStart >= startTime.Value)
                            && (!endTime.HasValue || a.EventStart < endTime.Value)
                            && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (a.Line == line || string.IsNullOrEmpty(line))
                            && a.DetailId == detailId
                            && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                            select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.DowntimeId, Level1 = a.Value, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                    ;


                    List<EventRowWithAllColumns> rows = q.ToList();
                    return rows;
                }
                else
                {
                    var q = from a in db.vw_DowntimeDetails
                            where (!startTime.HasValue || a.EventStart >= startTime.Value)
                            && (!endTime.HasValue || a.EventStart < endTime.Value)
                            && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                            && (a.Line == line || string.IsNullOrEmpty(line))
                            && a.DetailId == detailId
                            && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                            select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.DowntimeId, Level1 = a.Value, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                    ;


                    List<EventRowWithAllColumns> rows = q.ToList();
                    return rows;
                }
            }
        }

        /*
        public static List<SpecialReportRow> TopEventsGroupByLevel3(DateTime? startTime, DateTime? endTime, int detailId, string value, int takeCount, bool orderByMinutes, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q1 = from c in
                             (from a in db.vw_DowntimeDetails
                              where (!startTime.HasValue || a.EventStart >= startTime.Value)
                                && (!endTime.HasValue || a.EventStart < endTime.Value)
                                && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                && (a.Line == line || string.IsNullOrEmpty(line))
                                && a.Value == value
                              join b in db.Details on a.DetailId equals b.Id
                              where b.Id == detailId
                              //&& !string.IsNullOrEmpty(b.Level2) 
                              //&& !string.IsNullOrEmpty(b.Level3)
                              select new { a, b })
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
         */

        public static List<SpecialReportRow> HoursReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRowsByLine(startTime, endTime, detailId, value, line);
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


        public static List<SpecialReportRow> DayReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRowsByLine(startTime, endTime, detailId, value, line);
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

        public static List<SpecialReportRow> WeekReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRowsByLine(startTime, endTime, detailId, value, line);


            var q = from o in rows
                    group o by DowntimeCollection_Demo.DateTimeHelper.GetWeekStart(o.EventStart.Value, DayOfWeek.Monday) into g
                    select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = g.Key.ToString(@"MM\/dd\/yyyy") };

            result = q.ToList();

            return result;
        }


        public static List<SpecialReportRow> MonthReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRowsByLine(startTime, endTime, detailId, value, line);

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

        public static List<SpecialReportRow> YearReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = GetEventRowsByLine(startTime, endTime, detailId, value, line);

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

        public static List<TopEventsRow> Hidden_TopEvents(DateTime? startTime, DateTime? endTime, int detailId, string value, int takeCount, bool orderByMinutes, string line)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                if (line != "all")
                {

                    var q = from b in
                                 (from a in db.DowntimeReasonSet
                                  where a.Client == Filter_Client && a.HideReasonInReports == true
                                  select a)
                             join c in db.vw_DowntimeDetails on b.ID equals c.ReasonCodeID
                             where (!startTime.HasValue || c.EventStart >= startTime.Value)
                                && (!endTime.HasValue || c.EventStart < endTime.Value)
                                && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                && (c.Line == line || string.IsNullOrEmpty(line))
                                && c.DetailId == detailId
                                && (string.IsNullOrEmpty(value) || c.Value.ToLower() == value)
                             group c by c.Value into g
                             select new TopEventsRow { Client = Filter_Client, Level1 = g.Key, Minutes = g.Sum(o => o.Minutes), Occurences = g.Count() };                                

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
                else
                {

                    var q = from b in
                                (from a in db.DowntimeReasonSet
                                 where a.Client == Filter_Client && a.HideReasonInReports == true
                                 select a)
                            join c in db.vw_DowntimeDetails on b.ID equals c.ReasonCodeID
                            where (!startTime.HasValue || c.EventStart >= startTime.Value)
                               && (!endTime.HasValue || c.EventStart < endTime.Value)
                               && (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                                // && (c.Line == line || string.IsNullOrEmpty(line))
                                && c.DetailId == detailId
                                && (string.IsNullOrEmpty(value) || c.Value.ToLower() == value)
                            group c by c.Value into g
                            select new TopEventsRow { Client = Filter_Client, Level1 = g.Key, Minutes = g.Sum(o => o.Minutes), Occurences = g.Count() };    


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
        }

        public static List<EventRowWithAllColumns> Hidden_GetEventRows(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            using (DB db = new DB(DBHelper.GetConnectionString(Filter_Client)))
            {
                var q = from a in db.vw_DowntimeDetails
                        where (!startTime.HasValue || a.EventStart >= startTime.Value)
                        && (!endTime.HasValue || a.EventStart < endTime.Value)
                        && (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && a.DetailId == detailId
                        && (string.IsNullOrEmpty(value) || a.Value.ToLower() == value)
                        join c in db.DowntimeReasonSet on a.ReasonCodeID equals c.ID
                        where (c.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && c.HideReasonInReports == true
                        select new EventRowWithAllColumns { Client = a.Client, Comment = a.Comment, EventStart = a.EventStart, EventStop = a.EventStop, Id = a.DowntimeId, Level1 = a.Value, Line = a.Line, Minutes = a.Minutes, ReasonCodeId = a.ReasonCodeID }
                ;

                List<EventRowWithAllColumns> rows = q.ToList();
                return rows;

            }
        }

        public static List<SpecialReportRow> Hidden_HoursReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, detailId, value, line);//GetEventRows(startTime, endTime, level1, line);
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

        public static List<SpecialReportRow> Hidden_DayReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, detailId, value, line);
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

        public static List<SpecialReportRow> Hidden_WeekReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, detailId, value, line);


            var q = from o in rows
                    group o by DowntimeCollection_Demo.DateTimeHelper.GetWeekStart(o.EventStart.Value, DayOfWeek.Monday) into g
                    select new SpecialReportRow { Minutes = g.Sum(o => o.Minutes), Occurences = g.Count(), Title = g.Key.ToString(@"MM\/dd\/yyyy") };

            result = q.ToList();

            return result;
        }

        public static List<SpecialReportRow> Hidden_MonthReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, detailId, value, line);

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


        public static List<SpecialReportRow> Hidden_YearReport(DateTime? startTime, DateTime? endTime, int detailId, string value, string line)
        {
            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            List<SpecialReportRow> result = new List<SpecialReportRow>();
            List<EventRowWithAllColumns> rows = Hidden_GetEventRows(startTime, endTime, detailId, value, line);

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



    }
}