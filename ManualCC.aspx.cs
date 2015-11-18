using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Objects.DataClasses;
using System.Data;

namespace DowntimeCollection_Demo.Styles
{
    public partial class ManualCC : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            if(User.Identity.Name != "lebelge")
                Response.Redirect(DCSDashboardDemoHelper.BaseVirtualAppPath + "\\Login.aspx");
            */
             string op = Request["op"];
             if (!string.IsNullOrEmpty(op))
             {
                 string xml = string.Empty;
                 switch (op)
                 {
                     case "GetCaseCounts":
                         xml = GetCaseCounts();
                         break;
                     //case "CreateCC":
                     //    xml = CreateCC();
                     //    break;
                     case "UpdateCC":
                         xml = UpdateCC();
                         break;
                 }

                 Response.Write(xml);
                 Response.End();
             }

        }

        public static Options GetOptionFromReference(EntityReference reference)
        {
            EntityKeyMember[] keys = reference.EntityKey.EntityKeyValues;
            int id = 0;

            if (keys[0].Key.ToLower() == "id")
                id = (int)keys[0].Value;

            return DCSDashboardDemoHelper.GetOption(id);
        }

        public static Throughput GetThroughputFromReference(EntityReference reference)
        {
            EntityKeyMember[] keys = reference.EntityKey.EntityKeyValues;
            int id = 0;

            if (keys[0].Key.ToLower() == "id")
                id = (int)keys[0].Value;

            return DCSDashboardDemoHelper.GetThroughPut(id);
        }

        protected Dictionary<DateTime, ManualCaseCount> GetManualCaseCounts(DateTime d, string line)
        {


            DateTime endDate = new DateTime(d.Year, d.Month, d.Day, d.Hour, 00, 00);
            DateTime startDate = endDate.AddDays(-7);

            List<CaseCount> caseCounts = DCSDashboardDemoHelper.GetCaseCounts(startDate, endDate, "lebelge");
            ThroughputHistory th = DCSDashboardDemoHelper.getLatestThroughPutHistory(line);

            Throughput tPut = null;
            Options Op1 = null;
            Options Op2 = null;

            List<ThroughputHistory> histories = DCSDashboardDemoHelper.getThroughPutHistories(line);

            //List<object> results = new List<object>();

            Dictionary<DateTime, ManualCaseCount> results = new Dictionary<DateTime, ManualCaseCount>();

            d = startDate;
            d = new DateTime(d.Year, d.Month, d.Day, d.Hour, 00, 00);

            //Add ChangeOver if there is one
            /*
            if (th != null)
            {
                DateTime thDate = th.Date;
                thDate = new DateTime(thDate.Year, thDate.Month, thDate.Day, thDate.Hour, thDate.Minute, 00);

                Throughput put = th.Throughput;
                Options op1 = th.Options;
                Options op2 = th.Options1;

                if (th.Throughput == null)
                    put = GetThroughputFromReference(th.ThroughputReference);

                if (th.Options == null)
                    op1 = GetOptionFromReference(th.OptionsReference);

                if (th.Options1 == null)
                    op2 = GetOptionFromReference(th.Options1Reference);

                tPut = put;
                op1 = Op1;
                op2 = Op2;
                /*
                string mold = (tPut != null ? tPut.Name : "");
                string customer = (Op1 != null ? Op1.Name : "");
                string sku = (Op2 != null ? Op2.Name : "");

                results.Add(thDate, new ManualCaseCount
                {
                    date = th.Date.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    value = 0,
                    hour = th.Date.Hour,
                    cases = 0,
                    customer = customer,
                    mold = mold,
                    skew = sku,
                    ischangeover = true
                });
                    
            }
             */

            double diff = startDate.Subtract(endDate).TotalHours;

            if (diff < 0)
                diff = diff * -1;

            for (double x = 0; x < diff; x++)
            {
                ThroughputHistory tmpTh = (from o in histories
                                           where o.Date <= d
                                           orderby o.Date descending
                                           select o).FirstOrDefault();

                if (th == null)
                    th = tmpTh;

                DateTime thDate = d;

                if (tmpTh != null)
                {
                    if (tmpTh.Id != th.Id || (tPut == null || Op1 == null || Op2 == null))
                    {
                        th = tmpTh;
                        thDate = th.Date;
                        thDate = new DateTime(thDate.Year, thDate.Month, thDate.Day, thDate.Hour, thDate.Minute, 00);

                        tPut = th.Throughput;
                        Op1 = th.Options;
                        Op2 = th.Options1;

                        if (th.Throughput == null)
                            tPut = GetThroughputFromReference(th.ThroughputReference);

                        if (th.Options == null)
                            Op1 = GetOptionFromReference(th.OptionsReference);

                        if (th.Options1 == null)
                            Op2 = GetOptionFromReference(th.Options1Reference);
                    }
                }

                string mold = (tPut != null ? tPut.Name : "");
                string customer = (Op1 != null ? Op1.Name : "");
                string sku = (Op2 != null ? Op2.Name : "");
                bool changeOver = (th != null ? (th.Date == d ? true : false) : false);

                if (tmpTh != null)
                {
                    if (!results.Keys.Contains(thDate))
                    {
                        results.Add(thDate, new ManualCaseCount
                        {
                            date = thDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            value = 0,
                            hour = 0,
                            cases = 0,
                            customer = customer,
                            mold = mold,
                            skew = sku,
                            ischangeover = changeOver
                        });
                    }
                }

                if (!results.Keys.Contains(d))
                {
                    results.Add(d, new ManualCaseCount
                    {
                        date = d.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        value = 0,
                        hour = 0,
                        cases = 0,
                        customer = customer,
                        mold = mold,
                        skew = sku,
                        ischangeover = changeOver
                    });
                }
                d = d.AddHours(1d);
            }

            Dictionary<DateTime, int> caseCounters = new Dictionary<DateTime, int>();

            foreach (CaseCount cc in caseCounts)
            {
                DateTime et = cc.EventStop.Value;
                et = new DateTime(et.Year, et.Month, et.Day, et.Hour, et.Minute, 00);
                DateTime tmpDate = new DateTime(et.Year, et.Month, et.Day, et.Hour, 00, 00);

                if (results.Keys.Contains(et) && et != tmpDate)//Assuming CaseCount
                {
                    if (!caseCounters.Keys.Contains(et))
                    {
                        List<CaseCount> cList = (from o in caseCounts
                                                 where (o.EventStop.Value.Hour == et.Hour && o.EventStop.Value.DayOfYear == et.DayOfYear && o.EventStop.Value.Year == et.Year && o.EventStop.Value.Minute >= et.Minute)
                                                 orderby o.EventStop.Value.Minute ascending
                                                 select o).ToList();
                        int count = 0;

                        if (cList.Count > 0)
                        {
                            CaseCount firstCase = cList.ElementAt(0);
                            CaseCount lastCase = cList.ElementAt(cList.Count - 1);

                            if (firstCase != null && lastCase != null)
                            {
                                count = lastCase.CaseCount1 - firstCase.CaseCount1;
                            }
                        }

                        caseCounters.Add(et, count);
                    }

                    results[et] = new ManualCaseCount
                    {
                        date = et.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        value = cc.CaseCount1,
                        hour = cc.EventStart.Value.Hour,
                        cases = caseCounters[et],
                        customer = results[et].customer,
                        mold = results[et].mold,
                        skew = results[et].skew,
                        ischangeover = results[et].ischangeover
                    };


                }
                else if (results.Keys.Contains(tmpDate))
                {
                    et = tmpDate;//Because I copied and pasted. 

                    if (!caseCounters.Keys.Contains(et))
                    {

                        ThroughputHistory tmpTh = (from o in histories
                                                   where (o.Date.DayOfYear == et.DayOfYear && o.Date.Year == et.Year && o.Date.Hour == et.Hour)
                                                   orderby o.Date descending
                                                   select o).FirstOrDefault();

                        DateTime tmpDt = new DateTime(et.Year, et.Month, et.Day, et.Hour, 59, 00);

                        if (tmpTh != null)
                            tmpDt = tmpTh.Date;



                        List<CaseCount> cList = (from o in caseCounts
                                                 where (o.EventStop.Value.Hour == et.Hour && o.EventStop.Value.DayOfYear == et.DayOfYear && o.EventStop.Value.Year == et.Year && (o.EventStop.Value.Minute >= et.Minute && o.EventStop.Value.Minute < tmpDt.Minute))
                                                 orderby o.EventStop.Value.Minute ascending
                                                 select o).ToList();
                        int count = 0;

                        if (cList.Count > 0)
                        {
                            CaseCount firstCase = cList.ElementAt(0);
                            CaseCount lastCase = cList.ElementAt(cList.Count - 1);

                            if (firstCase != null && lastCase != null)
                            {
                                count = lastCase.CaseCount1 - firstCase.CaseCount1;
                            }
                        }

                        caseCounters.Add(et, count);
                    }

                    results[tmpDate] = new ManualCaseCount
                    {
                        date = tmpDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        value = cc.CaseCount1,
                        hour = cc.EventStart.Value.Hour,
                        cases = (caseCounters.Keys.Contains(tmpDate) ? caseCounters[tmpDate] : 0),
                        customer = results[tmpDate].customer,
                        mold = results[tmpDate].mold,
                        skew = results[tmpDate].skew,
                        ischangeover = results[tmpDate].ischangeover
                    };
                }
                else
                {

                    ThroughputHistory tmpTh = (from o in histories
                                               where o.Date <= et
                                               orderby o.Id descending
                                               select o).FirstOrDefault();


                    if (th != null && tmpTh != null)
                    {
                        if (tmpTh.Id != th.Id || (tPut == null || Op1 == null || Op2 == null))
                        {
                            th = tmpTh;

                            DateTime thDate = th.Date;
                            thDate = new DateTime(thDate.Year, thDate.Month, thDate.Day, thDate.Hour, thDate.Minute, 00);

                            tPut = th.Throughput;
                            Op1 = th.Options;
                            Op2 = th.Options1;

                            if (th.Throughput == null)
                                tPut = GetThroughputFromReference(th.ThroughputReference);

                            if (th.Options == null)
                                Op1 = GetOptionFromReference(th.OptionsReference);

                            if (th.Options1 == null)
                                Op2 = GetOptionFromReference(th.Options1Reference);
                        }
                    }

                    string mold = (tPut != null ? tPut.Name : "");
                    string customer = (Op1 != null ? Op1.Name : "");
                    string sku = (Op2 != null ? Op2.Name : "");

                    results.Add(tmpDate, new ManualCaseCount
                    {
                        date = cc.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        value = cc.CaseCount1,
                        hour = cc.EventStart.Value.Hour,
                        cases = cc.CaseCount1,
                        customer = customer,
                        mold = mold,
                        skew = sku,
                        ischangeover = false
                    });
                }

            }
            /*
            foreach (DateTime Date in results.Keys)
            {
                DateTime tmpDate = new DateTime(Date.Year, Date.Month, Date.Day, Date.Hour, 00, 00);

                if (tmpDate != Date)//assumes Date is a ChangeOver
                {
                    if (results.Keys.Contains(tmpDate) && results.Keys.Contains(Date))
                    {
                        ManualCaseCount changeOver = results[Date];


                        List<CaseCount> cList = (from o in caseCounts
                                                 where (o.EventStop.Value.Hour == Date.Hour && o.EventStop.Value.DayOfYear == Date.DayOfYear && o.EventStop.Value.Year == Date.Year && o.EventStop.Value.Minute >= Date.Minute)
                                                 orderby o.EventStop.Value.Minute ascending
                                                 select o).ToList();

                        int sum = cList.Sum(o => o.CaseCount1);
                    }
                }
            }
            */

            return results;

        }

        protected Dictionary<DateTime, ManualCaseCount> GetCalculatedCaseCounts(DateTime d, string line)
        {
            List<KeyValuePair<DateTime, ManualCaseCount>> lst = GetManualCaseCounts(d, line).ToList();

            lst.Sort(
                    delegate(KeyValuePair<DateTime, ManualCaseCount> firstPair,
                    KeyValuePair<DateTime, ManualCaseCount> secondPair)
                    {
                        return firstPair.Key.CompareTo(secondPair.Key);
                    }
                );

            for (int x = 0; x < lst.Count; x++)
            {
                if (x > 0)
                {
                    if (lst[x].Value.cases > 0)
                        lst[x].Value.cases += lst[x - 1].Value.cases;
                }
            }

            return lst.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

        }

        protected string GetCaseCounts()
        {
            
            DateTime d = DateTime.Now;

            DateTime.TryParse(Request.Form["date"], out d);

            Dictionary<DateTime, ManualCaseCount> lst = GetCalculatedCaseCounts(d, "company-demo");

            var values = lst.Select(x => x.Value);//results.OrderBy(x => x.Key).Select(x => x.Value);

            return DCSDashboardDemoHelper.ConvertToJsonString(values);
        }
        /*
        protected string UpdateCC()
        {
            int casecount = -1;

            DateTime d;
            DateTime? date = null; 

            if(DateTime.TryParse(Request.Form["date"], out d))
                date = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 00);

            
            int.TryParse(Request.Form["counts"], out casecount);

            if (casecount > -1 && date.HasValue)
            {
                
                using (DB db = new DB())
                {
                    List<ThroughputHistory> thList = DCSDashboardDemoHelper.getThroughPutHistories();

                    ThroughputHistory th = (from o in thList
                                            where (o.Date.DayOfYear == date.Value.DayOfYear && o.Date.Year == date.Value.Year && o.Date.Hour == date.Value.Hour)
                                            orderby o.Date descending
                                            select o).FirstOrDefault();


                    DateTime tmpDate = new DateTime(d.Year, d.Month, d.Day, d.Hour, 00, 00);

                    tmpDate = tmpDate.AddHours(1d);

                    if (th != null)
                    {
                        if (th.Date != date)
                            tmpDate = th.Date;
                    }


                    List<CaseCount> cList = DCSDashboardDemoHelper.GetCaseCounts(date, tmpDate, "lebelge");

                    
                    foreach (CaseCount cc in cList)
                    {
                        int id = cc.Id;

                        if (cc.EventStop.Value >= d && cc.EventStop.Value.Hour == d.Hour)
                        {
                            CaseCount tmpCase = (from o in db.CaseCountSet
                                                 where o.Id == id
                                                 select o).FirstOrDefault();

                            db.DeleteObject(tmpCase);
                        }
                    }


                    int diff = (int)d.Subtract(tmpDate).TotalMinutes;
                    
                    if (diff < 0)
                        diff = diff * -1;

                    decimal val = (decimal)casecount / ((decimal)diff / 5M);

                    decimal total = 0;
                    decimal lastCC = 0;
                    decimal counter = 0;

                    CaseCount lastCaseCount = (from o in db.CaseCountSet
                                                where o.EventStop.Value < date
                                                orderby o.EventStop.Value descending
                                                select o).FirstOrDefault();

                    if (lastCaseCount != null)
                        lastCC = lastCaseCount.CaseCount1;

                    total = lastCC;

                    DateTime end = date.Value.AddHours(1d);

                    diff = diff / 5;

                    List<CaseCount> tmpList = new List<CaseCount>();

                    if (val > 0)
                    {
                        for (int x = 0; x < diff; x++)
                        {
                            if (date.Value.Hour == end.Hour)
                                break;


                            total += val;
                            counter += val;

                            CaseCount cc = new CaseCount();
                            cc.CaseCount1 = Convert.ToInt32(total);
                            cc.Client = "lebelge";
                            cc.EventStart = date;
                            cc.EventStop = date;
                            cc.Line = "company-demo";

                            tmpList.Add(cc);

                            date = date.Value.AddMinutes(5d);
                            
                        }
                    }

                    if (tmpList.Count > 0)
                    {
                        tmpList.ElementAt(tmpList.Count - 1).CaseCount1 = tmpList.ElementAt(0).CaseCount1 + casecount;

                        foreach (CaseCount cc in tmpList)
                        {
                            db.AddToCaseCountSet(cc);
                        }
                    }

             

                    if (db.SaveChanges() >= 0)
                        return "Success" + d.ToString("MM/dd/yyyy hh:mm:ss tt");
                    else
                        return "ERROR";


                }

            }

            return "ERROR";

        }
        */

        protected string UpdateCC()
        {
            int casecount = -1;

            DateTime d;
            DateTime? date = null;

            if (DateTime.TryParse(Request.Form["date"], out d))
                date = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 00);


            int.TryParse(Request.Form["counts"], out casecount);

            if (casecount > -1 && date.HasValue)
            {

                using (DB db = new DB())
                {
                    List<ThroughputHistory> thList = DCSDashboardDemoHelper.getThroughPutHistories("company-demo");

                    ThroughputHistory th = (from o in thList
                                            where (o.Date.DayOfYear == date.Value.DayOfYear && o.Date.Year == date.Value.Year && o.Date.Hour == date.Value.Hour)
                                            orderby o.Date descending
                                            select o).FirstOrDefault();


                    DateTime tmpDate = new DateTime(d.Year, d.Month, d.Day, d.Hour, 00, 00);

                    tmpDate = tmpDate.AddHours(1d);

                    if (th != null)
                    {
                        if (th.Date != date)
                            tmpDate = th.Date;
                    }


                    List<CaseCount> cList = DCSDashboardDemoHelper.GetCaseCounts(date, tmpDate, "lebelge");


                    foreach (CaseCount cc in cList)
                    {
                        int id = cc.Id;

                        if (cc.EventStop.Value >= d && cc.EventStop.Value.Hour == d.Hour)
                        {
                            CaseCount tmpCase = (from o in db.CaseCountSet
                                                 where o.Id == id
                                                 select o).FirstOrDefault();

                            db.DeleteObject(tmpCase);
                        }
                    }


                    int diff = (int)d.Subtract(tmpDate).TotalMinutes;

                    if (diff < 0)
                        diff = diff * -1;

                    CaseCount lastCaseCount = (from o in db.CaseCountSet
                                               where o.EventStop.Value < date && o.Client == DCSDashboardDemoHelper.Filter_Client
                                               orderby o.EventStop.Value descending
                                               select o).FirstOrDefault();

                    DateTime lastHr = date.Value.AddHours(-1);

                    Dictionary<DateTime, ManualCaseCount> tmpCList = GetCalculatedCaseCounts(date.Value, "company-demo");

                    decimal lastCC = 0;

                    if (tmpCList.Count > 0)
                    {
                        casecount = casecount - tmpCList[lastHr].cases;
                    }

                    decimal val = (casecount) / ((decimal)diff / 5M);

                    if (lastCaseCount != null)
                        lastCC = lastCaseCount.CaseCount1;

                    decimal total = 0;
                    decimal counter = 0;

                    total = lastCC;

                    DateTime end = date.Value.AddHours(1d);

                    diff = diff / 5;

                    List<CaseCount> tmpList = new List<CaseCount>();

                    if (val > 0)
                    {
                        for (int x = 0; x < diff; x++)
                        {
                            if (date.Value.Hour == end.Hour)
                                break;


                            total += val;
                            counter += val;

                            CaseCount cc = new CaseCount();
                            cc.CaseCount1 = Convert.ToInt32(total);
                            cc.Client = "lebelge";
                            cc.EventStart = date;
                            cc.EventStop = date;
                            cc.Line = "company-demo";

                            tmpList.Add(cc);

                            date = date.Value.AddMinutes(5d);

                        }
                    }

                    if (tmpList.Count > 0)
                    {
                        tmpList.ElementAt(tmpList.Count - 1).CaseCount1 = tmpList.ElementAt(0).CaseCount1 + casecount;

                        foreach (CaseCount cc in tmpList)
                        {
                            db.AddToCaseCountSet(cc);
                        }
                    }

                    if (db.SaveChanges() >= 0)
                         return "Success" + d.ToString("MM/dd/yyyy hh:mm:ss tt");
                    else
                        return "ERROR";
                }

            }

            return "ERROR";

        }
        /*
        protected string CreateCC()
        {
            int hour = -1;
            int lastValue = -1;
            int casecount = -1;

            DateTime date = DateTime.Now;

            DateTime.TryParse(Request.Form["date"], out date);

            int.TryParse(Request.Form["hour"], out hour);
            int.TryParse(Request.Form["lastvalue"], out lastValue);

            int.TryParse(Request.Form["value"], out casecount);

            if (hour >= 0 && lastValue >= 0 && casecount >= 0)
            {
                using (DB db = new DB())
                {
                    DateTime d = date;

                    DateTime start = new DateTime(d.Year, d.Month, d.Day, hour, 01, d.Second);
                    DateTime end = new DateTime(d.Year, d.Month, d.Day, hour, 59, 59);

                    List<CaseCount> cList = DCSDashboardDemoHelper.GetCaseCounts(start, end, "lebelge");

                    int c = -1;
                    int results = -1;

                    if (cList.Count > 0)
                    {
                        foreach (CaseCount cc in cList)
                        {
                            int id = cc.Id;

                            CaseCount tmpCase = (from o in db.CaseCountSet
                                                 where o.Id == id
                                                 select o).FirstOrDefault();

                            db.DeleteObject(tmpCase);
                        }
                    }



                    CaseCount firstCC = new CaseCount();
                    firstCC.EventStart = start;
                    firstCC.EventStop = start;
                    firstCC.Client = "lebelge";
                    firstCC.CaseCount1 = (c != -1 ? c : lastValue);
                    firstCC.Line = "company-demo";

                    CaseCount lastCC = new CaseCount();
                    lastCC.EventStart = end;
                    lastCC.EventStop = end;
                    lastCC.CaseCount1 = casecount;
                    lastCC.Client = "lebelge";
                    lastCC.Line = "company-demo";

                    db.AddToCaseCountSet(firstCC);
                    db.AddToCaseCountSet(lastCC);

                    results = db.SaveChanges();

                    if(results > 0)
                        return "Success";
                    else if(results == 0)
                        return "No Changes made";
                    else if(results < 0)
                        return "ERROR";

                }
            }

            return "ERROR";
        }
         */
    }
}