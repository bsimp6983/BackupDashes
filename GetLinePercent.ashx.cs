using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DCSDemoData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace DowntimeCollection_Demo
{
    /// <summary>
    /// GetPipelinePercent 的摘要说明
    /// </summary>
    public class GetPipelinePercent : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            HttpRequest request = context.Request;
            string client = request["client"];
            string d = request["time"];
            DateTime clientTime = DateTime.Now;

            DateTime.TryParse(d, out clientTime);

            List<string> currentLines = new List<string>();

            using (DB db = new DB())
            {
                var q = from o in db.LineStatus
                        where o.Client == client
                        select o;
                List<LineStatus> myList = q.ToList<LineStatus>();
                List<object> results = new List<object>();
                DateTime now = DateTime.Now;

                DateTime endTime;
                if (clientTime.Hour < 7)
                {
                    endTime = clientTime.AddDays(-1);
                }
                else
                {
                    endTime = clientTime;
                }

                List<ProductionSchedule> schedules = (from o in db.ProductionSchedules
                                                      where o.Client == client
                                                      select o).ToList();

                List<LineEfficiency> efficiencies = DiamondCrystaldashboardHelper.getLineEfficiencies(clientTime.AddDays(-1), clientTime.AddHours(5));

                DateTime? shiftTime = DiamondCrystaldashboardHelper.getUserShiftStart();
                DateTime? lightTime = shiftTime;

                bool hasLightFeature = DiamondCrystaldashboardHelper.hasLightsOnFeature();

                foreach (LineStatus ls in myList)
                {
                    if (!currentLines.Contains(ls.Line))
                    {
                        if (DateTime.Now.Day == lightTime.Value.Day && DiamondCrystaldashboardHelper.getLightStatus(ls.Line, schedules) == true && hasLightFeature == true)
                        {
                            shiftTime = DiamondCrystaldashboardHelper.getLightTime(ls.Line, schedules, hasLightFeature);

                            if (!shiftTime.HasValue)
                                shiftTime = lightTime.Value;
                        }

                        DateTime? startTime = new DateTime(endTime.Year, endTime.Month, endTime.Day, shiftTime.Value.Hour, 0, 0);

                        currentLines.Add(ls.Line);

                        DateTime lsDate = DiamondCrystaldashboardHelper.convertToTimeZone(clientTime, ls.Line);

                        decimal numerator;
                        decimal denominator;
                        //decimal re = DiamondCrystaldashboardHelper.getRunningEfficiency((DateTime)ls.EventTime, out numerator, out denominator);
                        decimal re = DiamondCrystaldashboardHelper.getRunningLineEfficiency(ls.Line, efficiencies.Where(o => o.Time >= startTime.Value).Where(o => o.Time <= lsDate).Where(o => o.Line.ToLower() == ls.Line.ToLower()).ToList(), out numerator, out denominator);

                        //if (re < 1)
                            re = re * 100;

                        results.Add(new
                        {
                            line = ls.Line,
                            percent = string.Format("{0:0.##}%", re),
                        });
                    }
                }
                context.Response.Write(ConvertToJsonString(results));
            }
        }
        public string ConvertToJsonString(object obj)
        {
            JsonSerializer js = new JsonSerializer();
            System.IO.TextWriter tw = new System.IO.StringWriter();
            js.Serialize(tw, obj);
            return tw.ToString();
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}