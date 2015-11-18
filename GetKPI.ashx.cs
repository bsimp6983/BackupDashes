using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using DowntimeCollection_Demo.Classes;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// Summary description for GetKPI
    /// </summary>
    public class GetKPI : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.AddHeader("Cache-Control", "private, must-revalidate, max-age=0");

            HttpRequest request = context.Request;
            string client = context.User.Identity.Name;
            /*
             * d. The launch pad will show the KPIs as fiscal year-to-date, quarter-to-date 
                and month-to-date.
             */

            if (!string.IsNullOrEmpty(client))
            {
                List<string> lines = DCSDashboardDemoHelper.getLines();
                List<string> dtNoses = new List<string>();
                List<string> nonDtNoses = new List<string>();

                if (client.ToLower() == "txi")
                {
                    dtNoses.Add("Breakdown");
                    nonDtNoses.Add("Circumstantial");
                    nonDtNoses.Add("Planned");
                }
                else
                {

                    List<Options> opts = DCSDashboardDemoHelper.GetOptions(true);

                    dtNoses = opts.Select(o => o.Name).ToList();

                }


                //string strMtd = ConvertToJsonString(mtd);
                //string strQtd = ConvertToJsonString(qtd);
                //string strYtd = ConvertToJsonString(ytd);

                context.Response.Write(ConvertToJsonString(KPI.CalculateFiscal(client, lines, dtNoses, nonDtNoses)));
            }

        }

        public int getFirstQuarterMonth(int month)
        {
            if (month >= 0)
            {
                if (month == 0)
                    month = 1;

                if (month > 1 && month <= 3)
                    return 1;

                if (month > 3 && month <= 6)
                    return 3;

                if (month > 6 && month <= 9)
                    return 6;

                if (month > 9 && month <= 12)
                    return 1;
            }

            return 1;
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