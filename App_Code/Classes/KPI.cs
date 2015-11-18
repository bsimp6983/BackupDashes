using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DowntimeCollection_Demo.Classes
{
    public class KPI
    {
        public static List<DowntimeData> DowntimeData = new List<DowntimeData>();
        public static List<DowntimeData> NonDowntimeData = new List<DowntimeData>();

        public KPI()
        {

        }

        public KPI(DateTime sd, DateTime ed, string client, string line, List<string> dtNoses, List<string> nonDtNoses)
        {
            KPI.Calculate(sd, ed, client, line, dtNoses, nonDtNoses, this);
        }

        protected static void QueryDowntimeData(DateTime sd, DateTime ed, string client, List<string> dtNoses, List<string> nonDtNoses)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DowntimeData = (from o in db.DowntimeDataSet
                                join p in db.NatureOfStoppages
                                on o.ID equals p.DowntimeId
                                join c in db.Options
                                on p.OptionId equals c.Id
                                where o.Client == client
                                 && (o.EventStart >= sd || o.EventStop >= sd)
                                 && (o.EventStart <= ed || o.EventStop <= ed)
                                && o.EventStart.HasValue
                                && o.EventStop.HasValue
                                && dtNoses.Contains(c.Name)
                                orderby o.EventStart ascending
                                select o).ToList();

                NonDowntimeData = (from o in db.DowntimeDataSet
                                   join p in db.NatureOfStoppages
                                   on o.ID equals p.DowntimeId
                                   join c in db.Options
                                   on p.OptionId equals c.Id
                                   where o.Client == client
                                    && (o.EventStart >= sd || o.EventStop >= sd)
                                    && (o.EventStart <= ed || o.EventStop <= ed)
                                   && o.EventStart.HasValue
                                   && o.EventStop.HasValue
                                   && nonDtNoses.Contains(c.Name)
                                   orderby o.EventStart ascending
                                   select o).ToList();

            }

        }

        protected static void QueryDowntimeData(DateTime sd, DateTime ed, string client, string line, List<string> dtNoses, List<string> nonDtNoses)
        {
            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                DowntimeData = (from o in db.DowntimeDataSet
                                join p in db.NatureOfStoppages
                                on o.ID equals p.DowntimeId
                                join c in db.Options
                                on p.OptionId equals c.Id
                                where o.Client == client
                                && o.Line == line
                                 && (o.EventStart >= sd || o.EventStop >= sd)
                                 && (o.EventStart <= ed || o.EventStop <= ed)
                                && o.EventStart.HasValue
                                && o.EventStop.HasValue
                                && dtNoses.Contains(c.Name)
                                orderby o.EventStart ascending
                                select o).ToList();

                NonDowntimeData = (from o in db.DowntimeDataSet
                                   join p in db.NatureOfStoppages
                                   on o.ID equals p.DowntimeId
                                   join c in db.Options
                                   on p.OptionId equals c.Id
                                   where o.Client == client
                                && o.Line == line
                                    && (o.EventStart >= sd || o.EventStop >= sd)
                                    && (o.EventStart <= ed || o.EventStop <= ed)
                                   && o.EventStart.HasValue
                                   && o.EventStop.HasValue
                                   && nonDtNoses.Contains(c.Name)
                                   orderby o.EventStart ascending
                                   select o).ToList();

            }

        }

        public static Dictionary<string, KPI> Calculate(DateTime sd, DateTime ed, string client, List<string> lines, List<string> dtNoses, List<string> nonDtNoses)
        {
            Dictionary<string, KPI> kpis = new Dictionary<string, KPI>();

            QueryDowntimeData(sd, ed, client, dtNoses, nonDtNoses);

            foreach (string line in lines)
            {
                kpis.Add(line, KPI.Calculate(sd, ed, DowntimeData.Where(o => o.Line == line).ToList(), NonDowntimeData.Where(o => o.Line == line).ToList()));
            }

            return kpis;
        }

        public static int getFirstQuarterMonth(int month)
        {
            //First Quarter = 6/1 - 8/31
            //Second Quarter = 9/1 - 11/31
            //Third Quarter = 12/1 - 02/31
            //Fourth Quarter = 03/01 - 05/31

            if (month >= 0)
            {
                if (month == 0)
                    month = 1;

                if (month >= 12 || month <= 2)
                    return 12;

                if (month >= 6 && month <= 8)
                    return 6;

                if (month >= 9 && month <= 11)
                    return 9;

                if (month >= 3 && month <= 5)
                    return 3;
            }

            return 1;
        }

        public static Dictionary<string, Dictionary<string, KPI>> CalculateFiscal(string client, List<string> lines, List<string> dtNoses, List<string> nonDtNoses)
        {
            Dictionary<string, Dictionary<string, KPI>> kpis = new Dictionary<string, Dictionary<string, KPI>>();

            DateTime d = DateTime.Now;

            DateTime today = new DateTime(d.Year, d.Month, d.Day, 23, 59, 59);

            DateTime begOfFiscalYear = new DateTime(d.Year, 6, 1);
            DateTime endOfFiscalYear = new DateTime(d.Year + 1, 5, 1);
            DateTime begOfMonth = new DateTime(d.Year, d.Month, 1);
            DateTime begOfQuarter = new DateTime(d.Year, getFirstQuarterMonth(d.Month), 1);

            DateTime sd = begOfFiscalYear;

            if (begOfQuarter < sd)
                sd = begOfQuarter;

            QueryDowntimeData(sd, today, client, dtNoses, nonDtNoses);

            foreach (string line in lines)
            {
                KPI mtd = KPI.Calculate(begOfMonth, today, DowntimeData.Where(o => o.Line == line).ToList(), NonDowntimeData.Where(o => o.Line == line).ToList());
                KPI qtd = KPI.Calculate(begOfQuarter, today, DowntimeData.Where(o => o.Line == line).ToList(), NonDowntimeData.Where(o => o.Line == line).ToList());
                KPI ytd = KPI.Calculate(begOfFiscalYear, today, DowntimeData.Where(o => o.Line == line).ToList(), NonDowntimeData.Where(o => o.Line == line).ToList());

                kpis.Add(line, new Dictionary<string, KPI>()
                {
                    {"mtd", mtd},
                    {"qtd", qtd},
                    {"ytd", ytd}                    
                });
            }

            return kpis;            
        }

        public static KPI Calculate(DateTime sd, DateTime ed, string client, string line, List<string> dtNoses, List<string> nonDtNoses, KPI kpi = null)
        {
            QueryDowntimeData(sd, ed, client, dtNoses, nonDtNoses);

            return KPI.Calculate(sd, ed, DowntimeData.Where(o => o.Line == line).ToList(), NonDowntimeData.Where(o => o.Line == line).ToList(), kpi);
        }

        public static KPI Calculate(DateTime sd, DateTime ed, List<DowntimeData> downtimeData, List<DowntimeData> nondowntimeData, KPI kpi = null)
        {
            if (kpi == null)
                kpi = new KPI();

            decimal RF = 0.0m;
            decimal MTBF = 0.0m;
            decimal UF = 0.0m;

            decimal totalQueryMinutes = 0.00m;
            //decimal totalMinutes = 0.00m;
            decimal uptime = 0.00m;
            decimal downtime = 0.00m;
            decimal nonDowntime = 0.00m;
            decimal downtimeOccurrences = 0.00m;
            decimal nonDowntimeOccurrences = 0.00m;
            decimal totalOccurrences = 0.00m;

            if (downtimeData.Count > 0)
                downtimeData = downtimeData.Where(o => o.EventStart >= sd || o.EventStop >= sd).Where(o => o.EventStart <= ed || o.EventStop <= ed).ToList();

            if (nondowntimeData.Count > 0)
                nondowntimeData = nondowntimeData.Where(o => o.EventStart >= sd || o.EventStop >= sd).Where(o => o.EventStart <= ed || o.EventStop <= ed).ToList();

            if (downtimeData.Count > 0)
            {
                totalQueryMinutes = Convert.ToDecimal(ed.Subtract(sd).TotalMinutes);
                uptime = 0.00m;
                downtime = calculateDowntime(sd, ed, downtimeData);
                nonDowntime = 0.00m;
                downtimeOccurrences = 0.00m;

                DateTime firstStart = downtimeData.Where(o => o.EventStart.HasValue).Min(o => o.EventStart).Value;
                DateTime lastStart = downtimeData.Where(o => o.EventStart.HasValue).Max(o => o.EventStart).Value;

                if (nondowntimeData.Count > 0)
                    nonDowntime = calculateDowntime(sd, ed, nondowntimeData);

                uptime = totalQueryMinutes - downtime - nonDowntime;

                downtimeOccurrences = downtimeData.Count;
                nonDowntimeOccurrences = nondowntimeData.Count;

                totalOccurrences = downtimeOccurrences + nonDowntimeOccurrences;

                decimal rfDenominator = uptime + downtime;

                if (rfDenominator > 0)
                    RF = uptime / rfDenominator;

                // 1. 2. Mean Time Between Failure (MTBF) = Uptime(not downtime) / Downtime Occurrences
                MTBF = uptime / downtimeOccurrences;

                //2. 3. Utilization Factor (UF) = Uptime(not downtime) / The total of the time selected.

                UF = uptime / totalQueryMinutes;
            }

            kpi.TotalQueryMinutes = totalQueryMinutes;
            kpi.TotalMinutes = totalQueryMinutes;
            kpi.Uptime = uptime;
            kpi.Downtime = downtime;
            kpi.NonDowntime = nonDowntime;
            kpi.DowntimeOccurrences = downtimeOccurrences;
            kpi.NonDowntimeOccurrences = nonDowntimeOccurrences;
            kpi.TotalOccurrences = totalOccurrences;
            kpi.RF = RF;
            kpi.MTBF = MTBF;
            kpi.UF = UF;

            return kpi;
        }

        protected static decimal calculateDowntime(DateTime sd, DateTime ed, List<DowntimeData> downtimeData)
        {
            decimal dt = 0.00m;

            foreach (DowntimeData item in downtimeData)
            {
                decimal minutes = Convert.ToDecimal(item.EventStop.Value.Subtract(item.EventStart.Value).TotalMinutes);

                if (item.EventStart < sd)
                {
                    decimal diff = Convert.ToDecimal(sd.Subtract(item.EventStart.Value).TotalMinutes);

                    minutes -= diff;
                }

                if (item.EventStop > ed)
                {
                    decimal diff = Convert.ToDecimal(ed.Subtract(item.EventStop.Value).TotalMinutes);

                    minutes -= diff;
                }

                dt += minutes;
            }


            return dt;
        }

        public decimal TotalQueryMinutes { get; set; }
        public decimal TotalMinutes { get; set; }
        public decimal Uptime { get; set; }
        public decimal Downtime { get; set; }
        public decimal NonDowntime { get; set; }
        public decimal DowntimeOccurrences { get; set; }
        public decimal NonDowntimeOccurrences { get; set; }
        public decimal TotalOccurrences { get; set; }

        public decimal RF { get; set; }
        public decimal MTBF { get; set; }
        public decimal UF { get; set; }
    }
}