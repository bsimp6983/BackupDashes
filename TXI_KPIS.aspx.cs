using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DowntimeCollection_Demo.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DowntimeCollection_Demo
{
    public partial class TXI_KPIS : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string op = Request["op"];

                if (!string.IsNullOrEmpty(op))
                {
                    string xml = string.Empty;

                    switch (op)
                    {
                        case "calculation":
                        {
                            xml = getKPI();
                        }
                        break;
                    }

                    Response.Write(xml);
                    Response.End();
                }
            }
        }

        protected string getKPI()
        {
            DateTime sd;
            DateTime ed;

            string line = "Kiln_2";

            if(!string.IsNullOrEmpty(Request["line"]))
                line = Request["line"];

            if (!DateTime.TryParse(Request["sd"], out sd))
                return "false";

            if (!DateTime.TryParse(Request["ed"], out ed))
                return "false";
            
            List<string> dtNoses = new List<string>();
            List<string> nonDtNoses = new List<string>();

            dtNoses.Add("Breakdown");
            nonDtNoses.Add("Circumstantial");
            nonDtNoses.Add("Planned");

            return ConvertToJsonString(KPI.Calculate(sd, ed, "TXI", line, dtNoses, nonDtNoses));
        }

        protected string calculation()
        {
            /*
             * Reliability (RF) : Uptime(not downtime) / (Uptime(not downtime) + (Downtime - non downtime))
             * 1. 2. Mean Time Between Failure (MTBF) = Uptime(not downtime) / Downtime Occurrences
                2. 3. Utilization Factor (UF) = Uptime(not downtime) / The total of the time selected.
                Can you do me a favor in this first round and show the calculation so we can troubleshoot? So show the numbers that are being used.
             */

            decimal RF = 0.0m;
            decimal MTBF = 0.0m;
            decimal UF = 0.0m;

            decimal totalQueryMinutes = 0.00m;
            decimal uptime = 0.00m;
            decimal downtime = 0.00m;
            decimal nonDowntime = 0.00m;
            decimal downtimeOccurrences = 0.00m;
            decimal nonDowntimeOccurrences = 0.00m;
            decimal totalOccurrences = 0.00m;

            DateTime sd;
            DateTime ed;

            if(!DateTime.TryParse(Request["sd"], out sd))
                return "false";

            if(!DateTime.TryParse(Request["ed"], out ed))
                return "false";

            List<DowntimeData> downtimeData = new List<DowntimeData>();
            List<DowntimeData> nondowntimeData = new List<DowntimeData>();

            using (DB db = new DB(DBHelper.GetConnectionString()))
            {
                downtimeData = (from o in db.DowntimeDataSet
                                join r in db.DowntimeReasonSet
                                on o.ReasonCodeID equals r.ID
                                where o.Client == "TXI"
                                && o.Line == "Kiln_2"
                                && r.HideReasonInReports == false
                                && (o.EventStart >= sd || o.EventStop >= sd)
                                && (o.EventStart <= ed || o.EventStop <= ed)
                                && o.EventStart.HasValue
                                && o.EventStop.HasValue
                                orderby o.EventStart ascending
                                select o).ToList();

                nondowntimeData = (from o in db.DowntimeDataSet
                                join r in db.DowntimeReasonSet
                                on o.ReasonCodeID equals r.ID
                                where o.Client == "TXI"
                                && o.Line == "Kiln_2"
                                && r.HideReasonInReports == true
                                && (o.EventStart >= sd || o.EventStop >= sd)
                                && (o.EventStart <= ed || o.EventStop <= ed)
                                && o.EventStart.HasValue
                                && o.EventStop.HasValue
                                orderby o.EventStart ascending
                                select o).ToList();
            }

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

                if (uptime == 0 || totalOccurrences == 0)
                    return "false";

                if (rfDenominator > 0)
                    RF = uptime / rfDenominator;

                // 1. 2. Mean Time Between Failure (MTBF) = Uptime(not downtime) / Downtime Occurrences
                MTBF = uptime / downtimeOccurrences;

                //2. 3. Utilization Factor (UF) = Uptime(not downtime) / The total of the time selected.

                UF = uptime / totalQueryMinutes;
            }

            var obj = new
            {
                totalQueryMinutes = totalQueryMinutes,
                totalMinutes = totalQueryMinutes,
                uptime = uptime,
                downtime = downtime,
                nonDowntime = nonDowntime,
                downtimeOccurrences = downtimeOccurrences,
                nonDowntimeOccurrences = nonDowntimeOccurrences,
                totalOccurrences = totalOccurrences,
                RF = (RF * 100).ToString("#.##") + "%",
                MTBF = MTBF,
                UF = (UF * 100).ToString("#.##") + "%"
            };

            return ConvertToJsonString(obj);
        }

        protected decimal calculateDowntime(DateTime sd, DateTime ed, List<DowntimeData> downtimeData)
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

        public string ConvertToJsonString(object obj)
        {
            JsonSerializer js = new JsonSerializer();
            js.Converters.Add(new JavaScriptDateTimeConverter());
            System.IO.TextWriter tw = new System.IO.StringWriter();
            js.Serialize(tw, obj);
            return tw.ToString();

        }
        
    }
}