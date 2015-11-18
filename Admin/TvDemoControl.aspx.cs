using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;

namespace DowntimeCollection_Demo.Admin
{
    public partial class TvDemoControl : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url = WebConfigurationManager.AppSettings["expire url"];

            string op = Request["op"];
            if (!string.IsNullOrEmpty(op))
            {
                string result = string.Empty;
                switch (op)
                {
                    case "stopEvent":
                        result = stopEvent();
                        break;
                    //case "updateEvent":
                    //    result = updateEvent();
                    //    break;
                    case "clearLine":
                        result = clearLine();
                        break;

                }

                Response.Write(result);
                Response.End();
            }

        }

        protected string clearLine()
        {
            bool result = false;
            string line = Request.Form["line"];

            if (!string.IsNullOrEmpty(line) && line != "all")
            {
                using (DB db = new DB())
                {
                    List<DowntimeData> list = (from o in db.DowntimeDataSet
                                               where o.Client == "admin"
                                               && o.Line == line
                                               select o).ToList();

                    if (list.Count() > 0)
                    {
                        foreach (DowntimeData dt in list)
                        {
                            db.DeleteObject(dt);
                        }
                    }

                    result = db.SaveChanges() > 0;
                }
            }

            return result.ToString();
        }

        protected string stopEvent()
        {
            int time = 0;

            int.TryParse(Request.Form["time"], out time);

            string line = Request.Form["line"];
            int intLine = 0;
            int.TryParse(line, out intLine);

            decimal minutes = time;

            DateTime startdatetime = DateTime.Now;
            DateTime enddatetime = DateTime.Now.AddMinutes((double)minutes);

            /*
            DowntimeReason downtime = DCSDashboardDemoHelper.GetRandomReason("admin");

            string reason = downtime.Level1;

            if (!string.IsNullOrEmpty(downtime.Level2))
                reason += " > " + downtime.Level2;

            if (!string.IsNullOrEmpty(downtime.Level3))
                reason += " > " + downtime.Level3;

            string comment = DCSDashboardDemoHelper.GetRandomComment(reason);

            int reasonId = downtime.ID;

            */

            bool result = false;
            using (DB db = new DB())
            {
                DowntimeData dd = new DowntimeData();
                dd.EventStart = startdatetime;
                dd.EventStop = enddatetime;
                dd.Minutes = minutes;
                dd.Line = line;
                dd.Client = "admin";
                db.AddToDowntimeDataSet(dd);


                result = db.SaveChanges() > 0;
            }

      //      stopEventLine();

            return result.ToString();
        }

        //protected string stopEventLine()
        //{
        //    int time = 0;

        //    int.TryParse(Request.Form["time"], out time);

        //    string line = "0";

        //    line = Request.Form["line"];

        //    decimal minutes = time;

        //    DateTime startdatetime = DateTime.Now.AddMinutes(-(double)minutes);

        //    bool result = false;
        //    using (DB db = new DB())
        //    {
        //        var q = from o in db.LineStats
        //                where o.Line == line
        //                select o;
        //        LineStats d = q.FirstOrDefault();
        //        d.Status = false;
        //        d.EventTime = startdatetime;
        //        d.Counter = 0;

        //        result = db.SaveChanges() > 0;
        //    }

        //    return result.ToString();
        //}

        //protected string updateEvent()
        //{
        //    int time = 0;

        //    int.TryParse(Request.Form["time"], out time);

        //    string line = Request.Form["line"];

        //    decimal minutes = time;

        //    DateTime startdatetime = DateTime.Now.AddMinutes(-(double)minutes);

        //    bool result = false;
        //    using (DB db = new DB())
        //    {
        //        var q = from o in db.LineStats
        //                where o.Line == line
        //                select o;
        //        LineStats d = q.FirstOrDefault();
        //        d.Status = true;
        //        d.EventTime = startdatetime;
        //        d.Counter++;

        //        result = db.SaveChanges() > 0;
        //    }

        //    return result.ToString();
        //}
    }

}