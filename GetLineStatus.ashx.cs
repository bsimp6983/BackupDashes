using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Text;
using DCSDemoData;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// GetPipelineStatus 的摘要说明
    /// </summary>
    public class GetPipelineStatus : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.AddHeader("Cache-Control", "private, must-revalidate, max-age=0");

            HttpRequest request = context.Request;
            string client = request["client"];

            List<string> currentLines = new List<string>();

            using (DB db = new DB())
            {
                var q = from o in db.LineStatus
                        where o.Client == client
                        select o;
                List<LineStatus> myList = q.ToList<LineStatus>();
                List<object> results = new List<object>();
                DateTime now=DateTime.Now;

                foreach (LineStatus ls in myList)
                {
                    if (!currentLines.Contains(ls.Line))
                    {

                        currentLines.Add(ls.Line);

                        TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById(ls.Timezone.Trim());
                        DateTime newNow = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Local, hwZone);

                        if (!ls.EventTime.HasValue)
                            continue;

                        DateTime eventTime = ls.EventTime.Value;//TimeZoneInfo.ConvertTime(ls.EventTime.Value, TimeZoneInfo.Local, hwZone);

                        if (!string.IsNullOrEmpty(ls.FromTimezone))
                        {
                            if (ls.FromTimezone.Trim() != ls.Timezone.Trim())
                            {
                                TimeZoneInfo fromZone = TimeZoneInfo.FindSystemTimeZoneById(ls.FromTimezone.Trim());
                                eventTime = TimeZoneInfo.ConvertTime(ls.EventTime.Value, TimeZoneInfo.Local, fromZone);
                            }
                        }
                        else
                        {
                            eventTime = TimeZoneInfo.ConvertTime(ls.EventTime.Value, TimeZoneInfo.Local, hwZone);
                        }

                        //TimeSpan span = newNow.TimeOfDay;

                        //if (ls.EventTime != null)
                        //    span = newNow - (DateTime)ls.EventTime;

                        //String minute = (span.Minutes.ToString().Length > 1) ? span.Minutes.ToString() : ("0" + span.Minutes.ToString());
                        //String second = (span.Seconds.ToString().Length > 1) ? span.Seconds.ToString() : ("0" + span.Seconds.ToString());
                        //String spanString = (span.Days * 24 + span.Hours).ToString() + ":" + minute + ":" + second;

                        TimeSpan s = newNow.Subtract(eventTime);

                        string hr = s.Hours.ToString();

                        if (s.Hours < 10)
                            hr = "0" + s.Hours.ToString();

                        string min = s.Minutes.ToString();

                        if (s.Minutes < 10)
                            min = "0" + s.Minutes.ToString();

                        string sec = s.Seconds.ToString();

                        if (s.Seconds < 10)
                            sec = "0" + s.Seconds.ToString();

                        results.Add(new
                        {
                            line = ls.Line,
                            time = hr + ":" + min + ":" + sec,
                            status = ls.Status
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