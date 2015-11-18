using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.Security;
using DowntimeCollection_Demo;
using System.Data.Objects.DataClasses;
using System.Data;

namespace DCSDemoData
{
    public partial class MillardDashboardHelper
    {
        #region CountCase
        public static class CountCaseGoals
        {
            public static decimal Mark
            {
                get
                {
                    return 90m;
                }
            }
            public static int MaxCaseCount
            {
                get
                {
                    return 300;
                }
            }
            /// <summary>
            /// 每分钟指标
            /// </summary>
            public static decimal MinuteCases
            {
                get
                {
                    return 7.14m;
                }
            }

        }

        public static string ConvertToJsonString(object obj)
        {
            JsonSerializer js = new JsonSerializer();

            js.Converters.Add(new JavaScriptDateTimeConverter());
            System.IO.TextWriter tw = new System.IO.StringWriter();

            js.Serialize(tw, obj);

            return tw.ToString();
        }

        public static string convertTo12hrString(int hour)
        {
            string suffix = " AM";

            if (hour >= 12)
            {
                suffix = " PM";

                hour = hour - 12;
            }

            if (hour < 0)
                hour = hour * -1;
            else if (hour == 0)
                hour = 12;
            

            return hour.ToString() + suffix;

        }

        public static Dictionary<string, int> getLeft12HoursCountCase(DateTime clientTime)
        {
            DateTime d = clientTime.AddHours(-12);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0);
            DateTime? endTime = new DateTime(clientTime.AddHours(-1).Year, clientTime.AddHours(-1).Month, clientTime.AddHours(-1).Day, clientTime.AddHours(-1).Hour, 59, 59);
            Dictionary<string, int> result = new Dictionary<string, int>();
            Dictionary<int, int> collection = new Dictionary<int, int>();

            using (DB db = new DB())
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
                        result.Add(DiamondCrystaldashboardHelper.SecondToHoursString(Convert.ToInt32(row.Key * 3600m), false), Convert.ToInt32(row.CountCase));
                    }
                    else
                    {
                        result.Add(DiamondCrystaldashboardHelper.SecondToHoursString(Convert.ToInt32(startTime.Value.AddHours(i).Hour * 3600m), false), 0);
                    }
                }

                return result;
            }
        }


        public static Dictionary<string, int> getLast12hrsCaseCounts(DateTime clientTime, decimal estCount, string line)
        {
            Dictionary<string, int> results = new Dictionary<string, int>();

            foreach (var item in getGraphCaseCounts(clientTime, line))
            {
                results.Add(convertTo12hrString(item.Key.EventStop.Value.Hour), (int)item.Value);
            }

            return results;
        }

        public static Dictionary<string, decimal> getLast12hrsCaseCounts(DateTime clientTime, out List<CCEfficiency> caseCounts, string line, bool increments)
        {
            Dictionary<string, decimal> results = new Dictionary<string, decimal>();

            List<CCEfficiency> list = new List<CCEfficiency>();

            Dictionary<CCEfficiency, decimal> tmpList = getGraphCaseCounts(clientTime, line, increments);

            foreach (var item in tmpList)
            {
                results.Add(convertTo12hrString(item.Key.EventStop.Value.Hour), (int)item.Value);
                list.Add(item.Key);
            }

            caseCounts = list;

            return results;
        }


        public static Dictionary<string, decimal> getLast12hrsCaseCounts(DateTime? startTime, DateTime? endTime, string client, string line, bool increments = true)
        {
            Dictionary<string, decimal> results = new Dictionary<string, decimal>();

            foreach (var item in getGraphCaseCounts(startTime, endTime, line, increments, client))
            {
                if(!results.Keys.Contains(item.Key.EventStop.Value.Hour.ToString()))
                    results.Add(item.Key.EventStop.Value.Hour.ToString(), item.Value);
            }

            return results;
        }


        public static Dictionary<string, decimal> getLast12hrsCaseCounts(DateTime? startTime, DateTime? endTime, string line, bool increments = true)
        {
            Dictionary<string, decimal> results = new Dictionary<string, decimal>();

            foreach (var item in getGraphCaseCounts(startTime, endTime, line, increments))
            {
                results.Add(item.Key.EventStop.Value.Hour.ToString(), item.Value);
            }

            return results;
        }

        public static List<CCEfficiency> getLast12hrsCaseCountList(DateTime? clientTime, string line)
        {
            List<CCEfficiency> list = new List<CCEfficiency>();

            foreach (var item in getGraphCaseCounts(clientTime.Value, line))
            {
                list.Add(item.Key);
            }

            return list;
        }

        public static List<CCEfficiency> getLast12hrsCaseCountList(DateTime? startTime, DateTime? endTime, string line)
        {
            List<CCEfficiency> list = new List<CCEfficiency>();

            Dictionary<CCEfficiency, decimal> cases = getGraphCaseCounts(startTime, endTime, line);

            foreach (var item in cases)
            {
                list.Add(item.Key);
            }
            
            return list;
        }

        public static Dictionary<CCEfficiency, decimal> getGraphCaseCounts(DateTime clientTime, string line, bool increments = true)
        {
            DateTime d = clientTime.AddHours(-12);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0);
            DateTime? endTime = new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);

            return getGraphCaseCounts(startTime, endTime, line, increments);
        }

        public static List<CaseCount> getCasesPerHourPerLine(DateTime? startTime, DateTime? endTime, string client, bool increments = true )
        {
            List<CaseCount> rows = new List<CaseCount>();
            List<CaseCount> results = new List<CaseCount>();

            List<ThroughputHistory> tpHistories = DCSDashboardDemoHelper.getClientThroughPutHistories(client);

            using (DB db = new DB())
            {
                rows = (from a in db.CaseCountSet
                        where (a.Client == client || string.IsNullOrEmpty(client))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        && a.Line == "Line_06"
                        orderby a.EventStop.Value ascending
                        select a).ToList();

                List<string> lines = (from o in rows
                                      select o.Line).Distinct().ToList();

                int firstDayOfYear = 0;
                int lastDayOfYear = 0;
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;

                if (startTime.HasValue)
                {
                    firstDayOfYear = startTime.Value.DayOfYear;
                    year = startTime.Value.Year;
                    month = startTime.Value.Month;
                }
                else
                {
                    CaseCount c = rows.First();

                    if (c != null)
                        firstDayOfYear = c.EventStop.Value.DayOfYear;
                }

                if (endTime.HasValue)
                    lastDayOfYear = endTime.Value.DayOfYear;
                else
                {
                    CaseCount c = rows.Last();

                    if (c != null)
                        lastDayOfYear = c.EventStop.Value.DayOfYear;
                }

                foreach (string line in lines)
                {
                    DateTime tmpDate = startTime.Value;
                    tmpDate = new DateTime(tmpDate.Year, tmpDate.Month, tmpDate.Day, 0, 0, 0);

                    for (int dayOfYear = firstDayOfYear; dayOfYear <= lastDayOfYear; dayOfYear++)
                    {
                        for (int hour = 0; hour < 24; hour++)
                        {
                            CaseCount CC = new CaseCount();
                            CC.Line = line;
                            CC.CaseCount1 = 0;
                            CC.Client = client;
                            CC.EventStop = tmpDate;

                            List<CaseCount> cases = (from o in rows
                                                     where o.Line == line
                                                     && o.EventStop.Value.Hour == hour && o.EventStop.Value.DayOfYear == dayOfYear
                                                     orderby o.EventStop ascending
                                                     select o).ToList();

                            List<ThroughputHistory> historyPuts = (from o in tpHistories
                                                     where o.Date.DayOfYear == dayOfYear
                                                     && o.Date.Hour == hour
                                                     orderby o.Date ascending
                                                     select o).ToList();
                            if (cases.Count == 0)
                            {
                                CC.CaseCount1 = 0;
                            }
                            else
                            {

                                if (historyPuts.Count > 0)
                                {
                                    int count = 0;

                                    ThroughputHistory lastHistoryPut = null;

                                    foreach (ThroughputHistory historyPut in historyPuts)
                                    {
                                        Throughput put = historyPut.Throughput;

                                        List<CaseCount> lowerCases = (from o in cases
                                                                      where o.EventStop.Value <= historyPut.Date
                                                                      select o).ToList();

                                        if (lowerCases.Count > 0)
                                        {

                                            CaseCount f = lowerCases.First();
                                            CaseCount l = lowerCases.Last();

                                            if (f != l && f != null)
                                            {
                                                count += l.CaseCount1 - f.CaseCount1;
                                            }
                                        }

                                        lastHistoryPut = historyPut;

                                    }

                                    List<CaseCount> restOfCases = (from o in cases
                                                                   where o.EventStop.Value >= lastHistoryPut.Date
                                                                   select o).ToList();

                                    if (restOfCases.Count > 0)
                                    {
                                        CaseCount first = restOfCases.First();
                                        CaseCount last = restOfCases.Last();

                                        if (first != last && first != null)
                                        {
                                            count += last.CaseCount1 - first.CaseCount1;
                                        }
                                    }

                                    CC.CaseCount1 = count;
                                }
                                else
                                {

                                    CaseCount first = cases.First();
                                    CaseCount last = cases.Last();

                                    if (first != last && first != null)
                                    {
                                        CC.CaseCount1 = last.CaseCount1 - first.CaseCount1;
                                    }

                                }

                            }

                            results.Add(CC);

                            tmpDate = tmpDate.AddHours(1);

                        }

                        //tmpDate = tmpDate.AddDays(1);
                    }

                }

                rows = rows.OrderBy(x => x.EventStop.Value.Day).ThenBy(x => x.EventStop.Value.Hour).ThenBy(x => x.EventStop.Value.Minute).ThenBy(x => x.CaseCount1).ToList();

            }

            return results;
        }

        public static Dictionary<CCEfficiency, decimal> getGraphCaseCounts(DateTime? startTime, DateTime? endTime, string line, bool increments = true, string client = "")
        {
            /*
                Dictionary<CaseCount, decimal> results = new Dictionary<CaseCount, decimal>();
                DateTime tmpDate = startTime.Value;

                int totalHours = endTime.Value.Subtract(startTime.Value).Hours;

                int totalDays = endTime.Value.Subtract(startTime.Value).Days;

                if (totalDays > 0)
                    totalHours = totalDays * 24;



                for (double x = 0; x <= totalHours; x++)
                {
                    CaseCount c = new CaseCount();
                    c.CaseCount1 = 0;
                    c.EventStop = new DateTime(tmpDate.Year, tmpDate.Month, tmpDate.Day, tmpDate.Hour, 30, 0);//Set to XX:30, so it won't interfere with actual casecounts (which uses end values in hour)

                    tmpDate = tmpDate.AddHours(1.00);

                    results.Add(c, 0);

                }
             */

            if (string.IsNullOrEmpty(line))
                line = Filter_Line;

            if (string.IsNullOrEmpty(client))
                client = Filter_Client;

            List<CCEfficiency> rows = new List<CCEfficiency>();
            Dictionary<CCEfficiency, decimal> results = new Dictionary<CCEfficiency, decimal>();

            List<ThroughputHistory> tpHistories = DCSDashboardDemoHelper.getThroughPutHistories(line);
            List<Throughput> throughputs = DCSDashboardDemoHelper.GetThroughPuts(line);

            using (DB db = new DB())
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == client || string.IsNullOrEmpty(client))
                        && (a.Line == line || string.IsNullOrEmpty(line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        orderby a.EventStop.Value ascending
                        select new CCEfficiency { CaseCount = a.CaseCount1, Line = a.Line, Client = a.Client, EventStart = a.EventStart.Value, EventStop = a.EventStop.Value, Id = a.Id };

                if (q != null)
                    rows = q.ToList();
                
                DateTime tmpDate = startTime.Value;

                int totalHours = endTime.Value.Subtract(startTime.Value).Hours;

                int totalDays = endTime.Value.Subtract(startTime.Value).Days;

                if (totalDays > 0)
                    totalHours = totalDays * 24;

                CCEfficiency veryLast = rows.LastOrDefault();

                for (double x = 0; x <= totalHours; x++)
                {
                    CCEfficiency c = new CCEfficiency();

                    c.EventStop = new DateTime(tmpDate.Year, tmpDate.Month, tmpDate.Day, tmpDate.Hour, 30, 0);//Set to XX:30, so it won't interfere with actual casecounts (which uses end values in hour)
                    c.CaseCount = 0;

                    List<CCEfficiency> cList = (from o in rows
                                                where (o.EventStop.Value.Hour == tmpDate.Hour && o.EventStop.Value.DayOfYear == tmpDate.DayOfYear && o.EventStop.Value.Year == tmpDate.Year)
                                                orderby o.EventStop.Value.Minute ascending
                                             select o).ToList();

                    decimal count = 0;

                    if (cList.Count > 0)
                    {
                        if (increments)
                        {
                            CCEfficiency firstCase = cList.FirstOrDefault();
                            CCEfficiency lastCase = cList.LastOrDefault();

                            if (firstCase != null && lastCase != null)
                            {
                                c.EventStop = new DateTime(tmpDate.Year, tmpDate.Month, tmpDate.Day, tmpDate.Hour, 30, 0);
                                
                                try
                                {

                                    int tp1Id = DiamondCrystaldashboardHelper.GetThroughputIdFromReference(tpHistories.Where(o => o.Date <= c.EventStop.Value).OrderByDescending(o => o.Date).Select(o => o.ThroughputReference).FirstOrDefault());
                                    int tp2Id = DiamondCrystaldashboardHelper.GetThroughputIdFromReference(tpHistories.Where(o => o.Date <= c.EventStop.Value).OrderByDescending(o => o.Date).Select(o => o.ThroughputReference).FirstOrDefault());
                                                                    
                                    Throughput tp1 = (from o in throughputs
                                                      where o.Id == tp1Id
                                                      select o).FirstOrDefault();

                                    Throughput tp2 = (from o in throughputs
                                                      where o.Id == tp2Id
                                                      select o).FirstOrDefault();
                                    
                                    ThroughputHistory th1 = ( from o in tpHistories
                                                              where o.Date <= firstCase.EventStop.Value
                                                              orderby o.Id descending
                                                              select o).FirstOrDefault();

                                    ThroughputHistory th2 = ( from o in tpHistories
                                                              where o.Date <= lastCase.EventStop.Value
                                                              orderby o.Id descending
                                                              select o).FirstOrDefault();
                                    
                                    int totalCases = lastCase.CaseCount - firstCase.CaseCount;

                                    decimal totalEst = (tp2 == null ? getEstimatedCount(tmpDate, line) : tp2.PerHour);

                                    if (veryLast != null)
                                    {
                                        if (lastCase.Id == veryLast.Id)
                                        {
                                            DateTime es = lastCase.EventStop.Value;

                                            DateTime bgOfHour = new DateTime(es.Year, es.Month, es.Day, es.Hour, 0, 0);

                                            decimal minutes = Convert.ToDecimal(Math.Round(lastCase.EventStop.Value.Subtract(bgOfHour).TotalMinutes, 2));

                                            totalEst = (totalEst / 60) * minutes;
                                        }
                                    }

                                    if (th1 != null && th2 != null)
                                    {
                                       // totalEst = tp2.PerHour;

                                        if (th2.Date.Hour == lastCase.EventStop.Value.Hour && th2.Date.DayOfYear == lastCase.EventStop.Value.DayOfYear && th2.Date.Year == lastCase.EventStop.Value.Year)
                                        {

                                            CCEfficiency tmpCC1 = (from o in cList
                                                                   where o.EventStop.Value <= th2.Date
                                                                   orderby o.EventStop.Value descending
                                                                   select o).FirstOrDefault();

                                            CCEfficiency tmpCC2 = (from o in cList
                                                                   where o.EventStop.Value >= th2.Date
                                                                   orderby o.EventStop.Value ascending
                                                                   select o).FirstOrDefault();

                                            if (tmpCC1 != null && tmpCC2 != null)
                                            {
                                                if (tmpCC1.Id != tmpCC2.Id)
                                                {
                                                    decimal min1 = 0.00m;

                                                    decimal min2 = 0.00m;

                                                    if (tmpCC1.EventStop.HasValue)
                                                        min1 = (decimal)tmpCC1.EventStop.Value.Subtract(firstCase.EventStop.Value).TotalMinutes;

                                                    if (tmpCC2.EventStop.HasValue)
                                                        min2 = (decimal)lastCase.EventStop.Value.Subtract(tmpCC2.EventStop.Value).TotalMinutes;

                                                    decimal cTp1 = (decimal)tp1.PerHour / 12m;
                                                    decimal cTp2 = (decimal)tp2.PerHour / 12m;

                                                    min1 = (min1 / 5) * cTp1;
                                                    min2 = (min2 / 5) * cTp2;

                                                    totalCases = (tmpCC1.CaseCount - firstCase.CaseCount) + (tmpCC2.CaseCount - tmpCC1.CaseCount) + (lastCase.CaseCount - tmpCC2.CaseCount);

                                                    if (tp1.Id != tp2.Id)
                                                    {
                                                        totalEst = min1 + min2;

                                                        c.Line = totalCases + "/" + totalEst + "|" + tp1.Name + "|" + tp2.Name;
                                                    }
                                                    else
                                                    {
                                                        //totalEst = tp1.PerHour;

                                                        c.Line = totalCases + "/" + totalEst + "|" + tp1.Name;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                c.Line = totalCases + "/" + totalEst + "|" + tp1.Name;
                                            }

                                        }
                                        else
                                        {

                                            c.Line = totalCases + "/" + totalEst + "|" + tp1.Name;
                                        }
                                    }
                                    else
                                        c.Line = totalCases + "/" + totalEst;


                                    if (totalEst == 0)
                                        totalEst++;

                                    count = ((decimal)totalCases / (decimal)totalEst) * 100M;

                                    c.CaseCount = Convert.ToInt32(totalCases);
                                    c.EstCases = totalEst;
                                    
                                    //(lastEst + firstEst) / (LEST + FEST)
                                    //LEST = Case counts that should've been produced within the time frame between firstcase and lastcase
                                    
                                }
                                catch (Exception)
                                {
                                    count = 0;
                                }

                                if (count < 0)
                                    count = count * -1;

                                //value = ((DateTime.Now.DayOfWeek == DayOfWeek.Monday && MillardDashboardTVHelper.getLightStatus() == false) ? 0 : value);
                                
                                if (count >= 0)
                                {
                                    /*
                                    var sum = from r in rows
                                                where r.EventStop.Value.Hour == currentCase.EventStop.Value.Hour
                                                && r.EventStop.Value.Day == currentCase.EventStop.Value.Day
                                                group r by r.EventStop.Value.Hour into g
                                                select new { Total = g.Sum(y => y.CaseCount1) }.Total;

                                    List<int> list = sum.ToList();

                                    currentCase.CaseCount1 = list.Max();
                                     */
                                    

                                    results.Add(c, count);
                                }

                            }
                        }
                        else
                        {
                            CCEfficiency lastCase = cList.ElementAt(cList.Count - 1);

                            if (lastCase != null)
                            {
                                decimal total = (from o in cList
                                             select o.CaseCount).Sum();


                                decimal lastEst = (decimal)getEstimatedCount(lastCase.EventStop.Value, line);

                                total = total / (lastEst != 0 ? lastEst : 1);

                                count = total * 100M;

                                c.CaseCount = Convert.ToInt32(total);
                                c.EstCases = lastEst;

                                results.Add(c, count);
                            }
                        }
                    }
                    else
                    {
                        results.Add(c, 0);
                    }



                    tmpDate = tmpDate.AddHours(1.00);

                }

                rows = rows.OrderBy(x => x.EventStop.Value.Day).ThenBy(x => x.EventStop.Value.Hour).ThenBy(x => x.EventStop.Value.Minute).ThenBy(x => x.CaseCount).ToList();

            }

            return results;
        }

        public static Dictionary<CaseCount, decimal> getGraphCaseCounts_old(DateTime? startTime, DateTime? endTime, decimal estCount)
        {
            List<CaseCount> rows = new List<CaseCount>();
            Dictionary<CaseCount, decimal> results = new Dictionary<CaseCount, decimal>();

            using (DB db = new DB())
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop <= endTime)
                        orderby a.EventStop.Value ascending
                        select a;

                if (q != null)
                    rows = q.ToList();



                int firHour = (rows.Count > 0 ? rows.ElementAt(0).EventStop.Value.Hour : 0);

                if (firHour == 24)
                    firHour = 0;

                int diff = endTime.Value.Hour - firHour;

                if (diff < 12)
                    firHour += 12 - diff;

                firHour = firHour - 1;//For an extra hour before. 

                if (firHour == -1)
                    firHour = 23;

                DateTime tmpDate = startTime.Value;

                for (double x = 0; x <= endTime.Value.Subtract(startTime.Value).Hours; x++)
                {

                    CaseCount c = new CaseCount();
                    c.EventStop = new DateTime(tmpDate.Year, tmpDate.Month, tmpDate.Day, tmpDate.Hour, 30, 0);//Set to XX:30, so it won't interfere with actual casecounts (which uses end values in hour)
                    c.CaseCount1 = 0;

                    tmpDate = tmpDate.AddHours(1.00);

                    rows.Add(c);
                }

                rows = rows.OrderBy(x => x.EventStop.Value.Day).ThenBy(x => x.EventStop.Value.Hour).ThenBy(x => x.EventStop.Value.Minute).ThenBy(x => x.CaseCount1).ToList();


                CaseCount firstCase = new CaseCount();
                CaseCount lastCase;

                for (int x = 0; x < rows.Count; x++)
                {
                    //Get Current case
                    CaseCount currentCase = rows.ElementAt(x);

                    //if (x == 96)
                    //    currentCase = rows.ElementAt(x);

                    if (x == 0)
                        firstCase = currentCase;//Sets just in case until overridden.

                    if (x > 0 && x < rows.Count)
                    {
                        //Get previous case
                        lastCase = rows.ElementAt(x - 1);


                        //If hour of previous case is less than current
                        if (lastCase.EventStop.Value.Hour < currentCase.EventStop.Value.Hour && lastCase.EventStop.Value.Day == currentCase.EventStop.Value.Day)
                            firstCase = currentCase;
                        else if (lastCase.EventStop.Value.Hour > currentCase.EventStop.Value.Hour && lastCase.EventStop.Value.Day < currentCase.EventStop.Value.Day)
                            firstCase = currentCase;


                        if ((firstCase.CaseCount1 == 0 && currentCase.CaseCount1 != 0) && (firstCase.EventStop.Value.Hour == currentCase.EventStop.Value.Hour))
                            firstCase = currentCase;

                        //If there is a next case in array
                        if ((x + 1) < rows.Count)
                        {
                            //Next case
                            CaseCount nextCase = rows.ElementAt(x + 1);

                            //If next case Hour is greater than current
                            if ((nextCase.EventStop.Value.Hour > currentCase.EventStop.Value.Hour && nextCase.EventStop.Value.Day == currentCase.EventStop.Value.Day) || (nextCase.EventStop.Value.Hour < currentCase.EventStop.Value.Hour && nextCase.EventStop.Value.Day > currentCase.EventStop.Value.Day)/* || (x == rows.Count && currentCase.EventStop.Value.Subtract(firstCase.EventStop.Value).Minutes > 10)*/)
                            {
                                if (nextCase.EventStop.Value.Hour > currentCase.EventStop.Value.Hour && currentCase.Id == 0 && !currentCase.EventStart.HasValue)
                                {
                                    var z = from o in rows
                                            where o.EventStop.Value.Hour == currentCase.EventStop.Value.Hour
                                            && o.EventStop.Value.Day == currentCase.EventStop.Value.Day
                                            select o;

                                    int count = z.ToList().Count;

                                    if (count >= 3)
                                    {
                                        currentCase = lastCase;
                                        lastCase = rows.ElementAt(x - 2);
                                    }
                                }

                                decimal value = 0;
                                decimal countDiff = lastCase.CaseCount1 - currentCase.CaseCount1;

                                if (countDiff < 0)
                                    countDiff = countDiff * -1;

                                /*

                                if ((lastCase.CaseCount1 == 0 && currentCase.CaseCount1 != 0) && (lastCase.EventStop.Value.Hour == currentCase.EventStop.Value.Hour))
                                    value = (decimal)((currentCase.CaseCount1 - firstCase.CaseCount1) / (decimal)estCount) * 100;
                                else if ((lastCase.CaseCount1 != 0 && currentCase.CaseCount1 == 0) && (lastCase.EventStop.Value.Hour == currentCase.EventStop.Value.Hour))
                                    value = (decimal)((lastCase.CaseCount1 - firstCase.CaseCount1) / (decimal)estCount) * 100;
                                else
                                    value = (decimal)((currentCase.CaseCount1 - firstCase.CaseCount1) / (decimal)estCount) * 100;
                                */

                                try
                                {
                                    int firstEst = getEstimatedCount(firstCase.EventStop.Value, Filter_Line);
                                    int lastEst = getEstimatedCount(currentCase.EventStop.Value, Filter_Line);

                                    decimal a = (decimal)currentCase.CaseCount1 / (decimal)lastEst;
                                    decimal b = (decimal)firstCase.CaseCount1 / (decimal)firstEst;

                                    if ((lastCase.CaseCount1 == 0 && currentCase.CaseCount1 != 0) && (lastCase.EventStop.Value.Hour == currentCase.EventStop.Value.Hour))
                                        value = (a - b) * 100M;
                                    else if ((lastCase.CaseCount1 != 0 && currentCase.CaseCount1 == 0) && (lastCase.EventStop.Value.Hour == currentCase.EventStop.Value.Hour))
                                        value = (b - a) * 100M;
                                    else
                                        value = (a - b) * 100M;
                                }
                                catch (Exception)
                                {
                                    value = 0;
                                }

                                if (value < 0)
                                    value = value * -1;

                                string hour = string.Empty;

                                //Converts 24 and 0 to 12, also converts 24hrs to 12hr time. 
                                if (currentCase.EventStop.Value.Hour < 12)
                                {
                                    int t = currentCase.EventStop.Value.Hour;

                                    if (t == 0)
                                        t = 12;

                                    hour = t + " AM";
                                }
                                else
                                {
                                    int t = currentCase.EventStop.Value.Hour - 12;

                                    if (t == 0)
                                        t = 12;

                                    hour = t + " PM";

                                }

                                //value = ((DateTime.Now.DayOfWeek == DayOfWeek.Monday && MillardDashboardTVHelper.getLightStatus() == false) ? 0 : value);

                                if (value < 1 && value > 0)
                                    value = value * 100;//turn into decimal from percent

                                if (value >= 0)
                                {
                                    /*
                                    var sum = from r in rows
                                              where r.EventStop.Value.Hour == currentCase.EventStop.Value.Hour
                                              && r.EventStop.Value.Day == currentCase.EventStop.Value.Day
                                              group r by r.EventStop.Value.Hour into g
                                              select new { Total = g.Sum(y => y.CaseCount1) }.Total;

                                    List<int> list = sum.ToList();

                                    currentCase.CaseCount1 = list.Max();
                                    */

                                    int originalCount = currentCase.CaseCount1;

                                    currentCase.CaseCount1 = Convert.ToInt32(estCount * (value / 100));


                                    results.Add(currentCase, value);
                                }

                            }
                        }
                        else//assuming end of array
                        {
                            decimal value = 0;
                            decimal countDiff = lastCase.CaseCount1 - currentCase.CaseCount1;

                            if (countDiff < 0)
                                countDiff = countDiff * -1;
                            try
                            {
                                int firstEst = getEstimatedCount(firstCase.EventStop.Value, Filter_Line);
                                int lastEst = getEstimatedCount(currentCase.EventStop.Value, Filter_Line);

                                decimal a = (decimal)currentCase.CaseCount1 / (decimal)lastEst;
                                decimal b = (decimal)firstCase.CaseCount1 / (decimal)firstEst;

                                if ((lastCase.CaseCount1 == 0 && currentCase.CaseCount1 != 0) && (lastCase.EventStop.Value.Hour == currentCase.EventStop.Value.Hour))
                                    value = (a - b) * 100M;
                                else if ((lastCase.CaseCount1 != 0 && currentCase.CaseCount1 == 0) && (lastCase.EventStop.Value.Hour == currentCase.EventStop.Value.Hour))
                                    value = (b - a) * 100M;
                                else
                                    value = (a - b) * 100M;
                            }
                            catch (Exception)
                            {
                                value = 0;
                            }


                            if (value < 0)
                                value = value * -1;

                            string hour = string.Empty;

                            //Converts 24 and 0 to 12, also converts 24hrs to 12hr time. 
                            if (currentCase.EventStop.Value.Hour < 12)
                            {
                                int t = currentCase.EventStop.Value.Hour;

                                if (t == 0)
                                    t = 12;

                                hour = t + " AM";
                            }
                            else
                            {
                                int t = currentCase.EventStop.Value.Hour - 12;

                                if (t == 0)
                                    t = 12;

                                hour = t + " PM";

                            }

                            //value = ((DateTime.Now.DayOfWeek == DayOfWeek.Monday && MillardDashboardTVHelper.getLightStatus() == false) ? 0 : value);

                            if (value < 1 && value > 0)
                                value = value * 100;//turn into decimal from percent

                            if (value >= 0)
                            {
                                /*
                                var sum = from r in rows
                                          where r.EventStop.Value.Hour == currentCase.EventStop.Value.Hour
                                          && r.EventStop.Value.Day == currentCase.EventStop.Value.Day
                                          group r by r.EventStop.Value.Hour into g
                                          select new { Total = g.Sum(y => y.CaseCount1) }.Total;

                                List<int> list = sum.ToList();

                                currentCase.CaseCount1 = list.Max();
                                */

                                int originalCount = currentCase.CaseCount1;

                                currentCase.CaseCount1 = Convert.ToInt32(estCount * (value / 100));


                                results.Add(currentCase, value);
                            }

                        }
                    }
                }


            }

            return results;
        }



        private static int getEstimatedCount(DateTime date, string line)
        {
            int results = getThroughputValue(date, line);

            if (results > 0)
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

        private static int getThroughputValue(DateTime date, string line)
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

        private static Throughput getThroughput(DateTime date, string line)
        {
            ThroughputHistory th = DCSDashboardDemoHelper.getThroughPutHistory(date, line);

            if (th != null)
            {
                Throughput tp = th.Throughput;

                if (tp == null)
                    tp = GetThroughputFromReference(th.ThroughputReference);

                if (tp != null)
                    return tp;

            }

            return null;
        }
        public static Dictionary<string, int> getHoursCountCase(DateTime? startTime, DateTime? endTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            using (DB db = new DB())
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop < endTime)
                        group a by a.EventStop.Value.Hour into g
                        select new { g.Key, CountCase = g.Average(o => o.CaseCount1) };

                var rows = q.ToList();
                for (decimal i = 0; i < 24m; i++)
                {
                    var dhr = (from o in rows
                               where (Convert.ToDecimal(o.Key) == i)
                               select o).FirstOrDefault();
                    if (dhr == null)
                    {
                        result.Add(DiamondCrystaldashboardHelper.SecondToHoursString(Convert.ToInt32(i * 3600m), false), 0);
                    }
                    else
                    {
                        result.Add(DiamondCrystaldashboardHelper.SecondToHoursString(Convert.ToInt32(i * 3600m), false), (int)dhr.CountCase);
                    }
                }

                return result;
            }
        }

        public static Dictionary<string, int> getDayCountCase(DateTime? startTime, DateTime? endTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            using (DB db = new DB())
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
                    result.Add(r_starttime.AddDays(i).ToString(@"MM\/dd"), r.Sum(x => x.CaseCount1));
                }

                return result;
            }
        }

        public static Dictionary<string, decimal> getDayCountCase(DateTime? startTime, DateTime? endTime, decimal estCount, string line, bool increments)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();

            using (DB db = new DB())
            {
                var q = getGraphCaseCounts(startTime, endTime, line, increments);

                var rows = q.ToList();

                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.Key.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.Key.EventStop).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q1 = from o in rows
                             where o.Key.EventStop.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                             select o;

                    var r = q1.ToList();
                    int sum = (int)r.Sum(x => x.Value);
                    int count = r.Count;
                    int total = (count > 0 ? sum / count : sum);

                    result.Add(r_starttime.AddDays(i).ToString(@"MM\/dd"), total);
                }

                return result;
            }
        }
        
        public static Dictionary<string, int> getDayCaseCounts(DateTime? startTime, DateTime? endTime, string line)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            using (DB db = new DB())
            {
                var q = getGraphCaseCounts(startTime, endTime, line);

                var rows = q.ToList();

                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.Key.EventStart).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.Key.EventStop).Value);

                for (var i = 0; i <= r_endTime.Subtract(r_starttime).TotalDays; i++)
                {
                    var q1 = from o in rows
                             where o.Key.EventStop.Value.ToString(@"MM\/dd\/yyyy") == r_starttime.AddDays(i).ToString(@"MM\/dd\/yyyy")
                             select o;

                    var r = q1.ToList();
                    result.Add(r_starttime.AddDays(i).ToString(@"MM\/dd"), (int)(r.Sum(x => x.Value) / (int)r.Count));
                }

                return result;
            }
        }

        public static Dictionary<string, int> getWeekCountCase(DateTime? startTime, DateTime? endTime)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            using (DB db = new DB())
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (!startTime.HasValue || a.EventStop >= startTime)
                        && (!endTime.HasValue || a.EventStop < endTime)
                        select a;

                var records = q.ToList();
                var maxDate = q.Max(x => x.EventStart);

                var q1 = from a in records
                         group a by FeedCommDashboard.DateTimeHelper.GetWeekStart(a.EventStop.Value, DayOfWeek.Monday) into g
                         select new { g.Key, Occurrence = g.Sum(x => x.CaseCount1) };

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

        public static Dictionary<string, decimal> getWeekCaseCounts(DateTime? startTime, DateTime? endTime, decimal estCount, string line, bool increments)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();

            using (DB db = new DB())
            {
                var q = getGraphCaseCounts(startTime, endTime, line, increments);

                var records = q.ToList();
                var maxDate = q.Max(x => x.Key.EventStop);

                var q1 = from a in records
                         group a by FeedCommDashboard.DateTimeHelper.GetWeekStart(a.Key.EventStop.Value, DayOfWeek.Monday) into g
                         select new { g.Key, Occurrence = (decimal)(g.Sum(x => x.Value) / g.Count())};

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

            using (DB db = new DB())
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

        public static Dictionary<string, decimal> getMonthCaseCounts(DateTime? startTime, DateTime? endTime, decimal estCount, string line, bool increments)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();

            using (DB db = new DB())
            {
                var q = getGraphCaseCounts(startTime, endTime, line, increments);

                var maxDate = q.Keys.Max(x => x.EventStart);
                var rows = q.ToList();


                DateTime r_starttime, r_endTime;
                r_starttime = (startTime.HasValue ? startTime.Value : rows.Min(o => o.Key.EventStop).Value);
                //查询时结束日期是多一天的
                r_endTime = (endTime.HasValue ? endTime.Value.AddDays(-1) : rows.Max(o => o.Key.EventStop).Value);

                foreach (var item in DateTimeHelper.SplitMonths(r_starttime, r_endTime))
                {
                    var q1 = from o in rows
                             where o.Key.EventStop.Value.ToString(@"MM\/yyyy") == item.ToString(@"MM\/yyyy")
                             select o;

                    var r = q1.ToList();
                    DateTime monthEnd = new DateTime(item.AddMonths(1).Year, item.AddMonths(1).Month, 1);
                    if (monthEnd > maxDate) monthEnd = maxDate.Value;
                    int div = (int)monthEnd.Subtract(item).TotalDays;
                    if (div == 0) div = 1;
                    result.Add(item.ToString("MMM,yyyy", new System.Globalization.CultureInfo("en-US")), Convert.ToInt32(r.Sum(x => x.Value) / (r.Count > 0 ? r.Count : 1)));
                }

                return result;
            }
        }


        public static List<CaseCount> getLast12hrsCaseCountsSinceShiftStart(DateTime shiftStart, DateTime clientTime)
        {
            DateTime? startTime = shiftStart;
            DateTime? endTime = new DateTime(clientTime.AddHours(-1).Year, clientTime.AddHours(-1).Month, clientTime.AddHours(-1).Day, clientTime.AddHours(-1).Hour, 59, 59);
            List<CaseCount> list = new List<CaseCount>();

            using (DB db = new DB())
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

        public static decimal getRunningEfficiencyFromEstimate(DateTime clientTime, out int numerator, out decimal denominator)
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

            DateTime? shiftTime = getShiftTime();

   //         if (DateTime.Now.Day == getLightTime().Value.Day && getLightStatus() == true)
    //            shiftTime = getLightTime();

            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, shiftTime.Value.Hour, 0, 0);
            DateTime? endTime = clientTime;//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);
            //
            List<CaseCount> list = getLast12hrsCaseCountsSinceShiftStart(startTime.Value, clientTime);

            int amount = list.Count();

            int total = 0;
            decimal estCount = MillardDashboardHelper.CountCaseGoals.MinuteCases;

            int totalMinutes = Convert.ToInt32(startTime.Value.Subtract(endTime.Value).TotalMinutes);

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

            numerator = total;
            denominator = est;

            if (denominator == 0)
                denominator = 1;

            decimal result = 0;

            if(denominator != 0)
                result = (decimal)(numerator / (decimal)denominator) * 100m;

            return result;

        }

        //public static DateTime getStatEventTime()
        //{
        //    using (DB db = new DB())
        //    {
        //        LineStats stat = (from o in db.LineStats
        //                          where o.Client == "MillardDCS" && o.Line == "1"
        //                          select o).FirstOrDefault();

        //        if (stat != null)
        //        {
        //            return stat.EventTime.Value;
        //        }
        //    }

        //    DateTime d = DateTime.Now;

        //    return new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
        //}



        public static DateTime? getShiftTime()
        {
            /*
            Guid UserId = (Membership.GetUser() != null ? (Guid)Membership.GetUser().ProviderUserKey : Guid.Empty);

            using (DB db = new DB())
            {
                DateTime? date = (from o in db.UserInfoSet
                                  where o.UserId == UserId
                                  select o.ShiftStart).FirstOrDefault();

                if (date != null)
                    return date;
            }
            */
            DateTime d = DateTime.Now;

            return new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
        }

        //public static DateTime? getLightTime()
        //{

        //    using (DB db = new DB())
        //    {
        //        DateTime? date = (from o in db.LineStats
        //                          where o.Client == "MillardDCS"
        //                          select o.EventTime).FirstOrDefault();

        //        List<object> objs = new List<object>();

        //        if (date != null)
        //            return date;

        //    }

        //    DateTime? d = getShiftTime();

        //    return d;
        //}
        /*
        public static decimal getEstimatedCount()
        {

            Guid UserId = (Membership.GetUser() != null ? (Guid)Membership.GetUser().ProviderUserKey : Guid.Empty);

            using (DB db = new DB())
            {
                UserInfo q = (from o in db.UserInfoSet
                              where o.UserId == UserId
                              select o).FirstOrDefault();

                if (q != null)
                    return q.EstimatedOutput;
            }

            return MillardDashboardHelper.CountCaseGoals.MinuteCases * 60;
        }
        */
        //public static decimal getYesterdayEfficiencyFromEstimate(out int numerator, out decimal denominator)
        //{
        //    DateTime d = DateTime.Now.AddDays(-1);
        //    DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, (DateTime.Now.DayOfWeek == DayOfWeek.Monday ? getLightTime() : getShiftTime()).Value.Hour, 0, 0);
        //    DateTime? endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 59, 59);//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);
        //    //
        //    using (DB db = new DB())
        //    {
        //        var q = from a in db.CaseCountSet
        //                where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
        //                && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
        //                && (!startTime.HasValue || a.EventStop >= startTime)
        //                && (!endTime.HasValue || a.EventStop <= endTime)
        //                select a;


        //        List<CaseCount> list = new List<CaseCount>();

        //        if (q != null)
        //            list = q.ToList();

        //        int amount = list.Count();

        //        int total = 0;
        //        decimal estCount = MillardDashboardHelper.CountCaseGoals.MinuteCases;

        //        int totalMinutes = Convert.ToInt32(startTime.Value.Subtract(endTime.Value).TotalMinutes);

        //        decimal est = totalMinutes * estCount;

        //        if (est < 0)
        //            est = est * -1;

        //        for (int x = 0; x < list.Count; x++)
        //        {
        //            if (x > 0)
        //            {
        //                int first = list.ElementAt(x - 1).CaseCount1;
        //                int sec = list.ElementAt(x).CaseCount1;

        //                int diff = first - sec;

        //                if (diff < 0)
        //                    diff = diff * -1;//Make positive number

        //                total += diff;
        //            }
        //        }

        //        numerator = total;
        //        denominator = est;

        //        if (denominator == 0)
        //            denominator++;

        //        decimal result = 0;

        //        if (denominator != 0)
        //            result = (decimal)(numerator / denominator) * 100m;

        //        return result;
        //    }
        //}
        //public static decimal getRunningEfficiency(DateTime clientTime, out int numerator, out decimal denominator)
        //{
        //    DateTime d;
        //    int startOfHours = 5;//7

        //    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)//if Monday
        //        startOfHours = getStatEventTime().Hour;

        //    if (clientTime.Hour < startOfHours)
        //    {
        //        d = clientTime.AddDays(-1);
        //    }
        //    else
        //    {
        //        d = clientTime;
        //    }
        //    DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, startOfHours, 0, 0);
        //    DateTime? endTime = clientTime;//new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, clientTime.Hour, 59, 59);
        //    //
        //    using (DB db = new DB())
        //    {
        //        var q = from a in db.CaseCountSet
        //                where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
        //                && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
        //                && (!startTime.HasValue || a.EventStop >= startTime)
        //                && (!endTime.HasValue || a.EventStop <= endTime)
        //                select a.CaseCount1;

        //        int v = 0, offset = 5;
        //        decimal par = CountCaseGoals.MinuteCases*offset;
        //        int total = Convert.ToInt32(Math.Abs(startTime.Value.Subtract(endTime.Value).TotalMinutes) / offset);
        //        if (q.Count() > 0) v = q.Sum(x => x);
        //        numerator = v;
        //        denominator = par * total;
        //        return (decimal)v / (decimal)(par * total) * 100m;
        //    }
        //}

        public static decimal getYesterdayEffiency(out int numerator, out decimal denominator)
        {
            numerator = 0;
            //denominator = 7200 * 100;
            denominator = CountCaseGoals.MinuteCases * 60 * 24;//7200 * 100;
            /*DateTime DateOfYesterday = DateTime.Now.Date.AddDays(-1);
            DateTime startTime = DateOfYesterday.AddHours(6.0);
            DateTime endTime = DateOfYesterday.AddHours(18.0);*/
            DateTime d = DateTime.Now.AddDays(-1);
            DateTime? startTime = new DateTime(d.Year, d.Month, d.Day, 6, 0, 0);
            DateTime? endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 59, 59);
            //
            using (DB db = new DB())
            {
                var q = from a in db.CaseCountSet
                        where (a.Client == Filter_Client || string.IsNullOrEmpty(Filter_Client))
                        && (a.Line == Filter_Line || string.IsNullOrEmpty(Filter_Line))
                        && (a.EventStop >= startTime)
                        && (a.EventStop <= endTime)
                        select a.CaseCount1;

                if (q.Count() == 0) return 0m;
                numerator = q.Sum(x => x);
                //return (decimal)numerator / (decimal)7200 * 100m;
                return (decimal)numerator / denominator * 100m;
            }
        }

        #endregion
    }

    public class DowntimeHistoryRow
    {
        public DateTime Time { get; set; }
        public string Level3 { get; set; }
        public decimal MinutesSum { get; set; }
        public int week { get; set; }
    }

    public class AllOccurrenceHistoryRow
    {
        public string Level3 { get; set; }
        public Dictionary<string, int> Datas { get; set; }
    }
}
