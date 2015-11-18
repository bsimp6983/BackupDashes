using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using DCSDemoData;
using DowntimeCollection_Demo.Classes;
using System.Web.Security;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.Web.Caching;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// $codebehindclassname$ 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Service : IHttpHandler
    {
        private HttpRequest request;
        private string _currentClient;
        string Line;

        public void ProcessRequest(HttpContext context)
        {
            request=context.Request;
            context.Response.ContentType = "application/json";

            _currentClient = context.User.Identity.Name;//当前登录

            string op = request["op"];
            string level1, level2;
            Line = request["line"];

            if (string.IsNullOrEmpty(Line))
            {
                Line = "company-demo";

                if (_currentClient.ToLower() == "millarddcs")
                    Line = "line1";
            }
            else if (Line.Trim() == "company-demo" && _currentClient.ToLower() == "millarddcs")
            {
                Line = "line1";
            }

            Line = Line.Replace('#', ' ').Trim();
            
            if (string.IsNullOrEmpty(op)) return;
            if (string.IsNullOrEmpty(_currentClient)) return;

            switch (op.ToLower())
            {
                case "getuser":
                    context.Response.Write(GetUser());
                    break;
                case "sendclientdailyemail":
                    context.Response.Write(SendClientDailyEmail());
                    break;
                case "loadgriddata":
                    context.Response.Write(ConvertToJsonString(LoadGridData()));
                    break;
                case "level1":
                    context.Response.Write(ConvertToJsonString(GetLevel1()));
                    break;
                case "level2":
                    level1 = request.Form["level1"];
                    if (string.IsNullOrEmpty(level1)) return;
                    context.Response.Write(ConvertToJsonString(GetLevel2(level1)));
                    break;
                case "level3":
                    level1 = request.Form["level1"];
                    level2 = request.Form["level2"];
                    if (string.IsNullOrEmpty(level1) || string.IsNullOrEmpty(level2)) return;
                    context.Response.Write(ConvertToJsonString(GetLevel3(level1,level2)));
                    break;
                case "stop":
                    DateTime start, stop;
                    if (DateTime.TryParse(request.Form["start"], out start) && DateTime.TryParse(request.Form["stop"], out stop))
                    {
                        context.Response.Write(ConvertToJsonString(Stop(start,stop)));
                    }
                    break;
                case "save":
                    string reasonCode =request.Form["reasonCode"];

                    string[] ids = request.Form["id"].Split(',');

                    string response = "false";

                    bool changeMinutes = ids.Length == 1;

                    foreach (string strId in ids)
                    {
                        int id;
                        int reasonId;
                        if (int.TryParse(strId, out id) && (!string.IsNullOrEmpty(reasonCode)) && int.TryParse(request.Form["reasonId"], out reasonId))
                        {
                            response = Save(id, context, changeMinutes);
                        }
                    }

                    context.Response.Write(response);

                    break;
                case "getleveldatasourcelength":
                    context.Response.Write(getLevelDataSourceLength(request.Form["level1"], request.Form["level2"]));
                    break;
                case "addnewevent":
                    addNewEvent(context);
                    break;
                case "getevent":
                    context.Response.Write(ConvertToJsonString(getEvent()));
                    break;
                case "gettreetable":
                    context.Response.Write(ConvertToJsonString(getTreeTable()));
                    break;
                case "saveall":
                    context.Response.Write(saveAllPostRows());
                    break;
                case "delrecords":
                    context.Response.Write(delRecords());
                    break;
                case "mergerecords":
                    context.Response.Write(mergeRecords());
                    break;
                case "getoptioninfo":
                    context.Response.Write(GetOptionInfo());
                    break;
                case "getthroughputs":
                    context.Response.Write(GetThroughPuts());
                    break;
                case "getoptions":
                    context.Response.Write(GetOptions());
                    break;
                case "getoption":
                    context.Response.Write(GetOption());
                    break;
                case "createthroughput":
                    context.Response.Write(CreateThroughPut());
                    break;
                case "createoption":
                    context.Response.Write(CreateOption());
                    break;
                case "updatethroughput":
                    context.Response.Write(UpdateThroughPut());
                    break;
                case "updateoption":
                    context.Response.Write(UpdateOption());
                    break;
                case "deletethroughput":
                    context.Response.Write(DeleteThroughPut());
                    break;
                case "deleteoption":
                    context.Response.Write(DeleteOption());
                    break;
                case "getlatestthroughput":
                    {
                        Throughput tp = GetLatestThroughput();

                        context.Response.Write((tp != null ? tp.Id.ToString() : ""));
                    }
                    break;
                case "turnlights":
                    TurnLights();
                    break;
                case "getlights":
                    context.Response.Write(GetLights());
                    break;
                case "downtime":
                    context.Response.Write(GetDowntime());
                    break;
                case "casecount":
                    context.Response.Write(GetCaseCount());
                    break;
                case "lines":
                    context.Response.Write(GetLines());
                    break;
                case "scheduledminutes":
                    context.Response.Write(GetTotalScheduledMinutes());
                    break;
                case "performancechart":
                    context.Response.Write(GetPerformanceChart());
                    break;
                case "getlatestdtwithnostop":
                    context.Response.Write(GetLatestDTWithNoStop());
                    break;
                case "createdtwithnostop":
                    context.Response.Write(CreateDTWithNoStop());
                    break;
                case "updatedtwithnostop":
                    context.Response.Write(UpdateDTWithNoStop());
                    break;
                case "kpi":
                    context.Response.Write(CalculateKPI());
                    break;
                case "updateoptioninfo":
                    context.Response.Write(UpdateOptionInfo());
                    break;
                case "checkpassword":
                    context.Response.Write(CheckPassword());
                    break;
                case "getadminpassword":
                    context.Response.Write(GetAdminPassword());
                    break;
                case "splitdowntime":
                    context.Response.Write(SplitDowntime(Convert.ToInt32(request["downtimeId"]), Convert.ToDecimal(request["minutes"])));
                    break;
            }
        }

        private string GetUser()
        {
            UserInfo info = DCSDashboardDemoHelper.GetUserInfo();

            ThriveUser thriveUser = new ThriveUser();

            MembershipUser user = Membership.GetUser();

            if (user == null) return ConvertToJsonString(thriveUser);

            thriveUser.Email = user.Email;
            thriveUser.Username = user.UserName;
            thriveUser.GuidKey = (Guid) user.ProviderUserKey;

            if (info != null)
            {
                thriveUser.hideHelper = info.HideHelper;
                thriveUser.hidePanel = info.HidePanel;

            }
            else
            {
                thriveUser.hideHelper = false;
                thriveUser.hidePanel = false;
            }

            return ConvertToJsonString(thriveUser);
        }

        public string GetAdminPassword()
        {
            return DCSDashboardDemoHelper.GetAdminPassword();
        }

        public string CheckPassword()
        {
            bool passed = false;

            string password = request.Form["password"];

            string adminPassword = DCSDashboardDemoHelper.GetAdminPassword();

            if (!string.IsNullOrEmpty(adminPassword))
            {
                if (password == adminPassword)
                    passed = true;
            }
            else//Assumed it hasn't been set. 
            {
                passed = true;
            }

            return passed.ToString();
        }

        public string UpdateOptionInfo()
        {
            
            string op1Name = request.Params["opt1Name"];
            string op1Required = request.Params["opt1Required"];
            string op1Enabled = request.Params["opt1Enabled"];

            string op2Name = request.Params["opt2Name"];
            string op2Required = request.Params["opt2Required"];
            string op2Enabled = request.Params["opt2Enabled"];

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                OptionInfo opt1 = (from o in db.OptionInfoes
                                   where o.Number == 1
                                   && o.Client == _currentClient
                                   select o).FirstOrDefault();

                OptionInfo opt2 = (from o in db.OptionInfoes
                                   where o.Number == 2
                                   && o.Client == _currentClient
                                   select o).FirstOrDefault();

                if (opt1 == null)
                {
                    opt1 = new OptionInfo()
                    {
                        Client = _currentClient,
                        Number = 1
                    };

                    db.AddToOptionInfoes(opt1);
                }

                if (opt2 == null)
                {
                    opt2 = new OptionInfo()
                    {
                        Client = _currentClient,
                        Number = 2
                    };

                    db.AddToOptionInfoes(opt2);
                }

                opt1.IsRequired = Convert.ToBoolean(op1Required);
                opt1.Enabled = Convert.ToBoolean(op1Enabled);
                opt1.Name = op1Name;

                opt2.IsRequired = Convert.ToBoolean(op2Required);
                opt2.Enabled = Convert.ToBoolean(op2Enabled);
                opt2.Name = op2Name;

                return (db.SaveChanges() > 0).ToString();
            }

        }

        public static KPI CalculateKPI(DateTime sd, DateTime ed, string client, string line)
        {
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

            KPI kpi = new KPI(sd, ed, client, line, dtNoses, nonDtNoses);

            return kpi;
        }

        public string CalculateKPI()
        {
            DateTime sd;
            DateTime ed;

            if (!DateTime.TryParse(request.Params["sd"], out sd))
            {
                return null;
            }

            if (!DateTime.TryParse(request.Params["ed"], out ed))
            {
                return null;
            }

            return ConvertToJsonString(CalculateKPI(sd, ed, _currentClient, Line));
        }

        public string SendClientDailyEmail()
        {
            if (string.IsNullOrEmpty(_currentClient))
                return string.Empty;

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                ClientEmail cEmail = (from o in db.ClientEmails
                                      where o.Client == _currentClient
                                      select o).FirstOrDefault();

                if (cEmail == null)
                {
                    cEmail = new ClientEmail {Client = _currentClient, SendDailyDigest = true};

                    db.AddToClientEmails(cEmail);

                    return (db.SaveChanges() > 0).ToString();
                }

                cEmail.SendDailyDigest = true;
                return (db.SaveChanges() > 0).ToString();
            }
        }

        public string GetLatestDTWithNoStop()
        {

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                DowntimeData dt = (from o in db.DowntimeDataSet
                                   where o.Client == _currentClient
                                    && (o.Line == Line)
                                   && o.EventStop.Value == null
                                   orderby o.EventStart ascending
                                   select o).FirstOrDefault();


                if (dt == null)
                    return ConvertToJsonString(new
                    {
                        ID = -1,
                        EventStart = "",
                        EventStop = "",
                        ReasonCodeID = -1,
                        ReasonCode = "",
                        Minutes = 0,
                        Line = Line,
                        Comment = ""
                    });
                if (!dt.EventStop.HasValue)
                {
                    if (dt.EventStart != null)
                        return ConvertToJsonString(new
                        {
                            ID = dt.ID,
                            EventStart = dt.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            EventStop = "",
                            ReasonCodeID = dt.ReasonCodeID,
                            ReasonCode = dt.ReasonCode,
                            Minutes = (dt.Minutes.HasValue ? dt.Minutes.Value : 0),
                            Line = dt.Line,
                            Comment = dt.Comment
                        });
                }
            }

            return ConvertToJsonString(new
            {
                ID = -1,
                EventStart = "",
                EventStop = "",
                ReasonCodeID = -1,
                ReasonCode = "",
                Minutes = 0,
                Line = Line,
                Comment = ""
            });

        }

        public string CreateDTWithNoStop()
        {
            DateTime sd;

            if (!DateTime.TryParse(request.Params["sd"], out sd))
            {
                return null;
            }


            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                DowntimeData dt = new DowntimeData();

                dt.Client = _currentClient;
                dt.EventStart = sd;
                dt.EventStop = null;
                dt.Line = Line;
                dt.Minutes = 0;

                db.AddToDowntimeDataSet(dt);

                db.SaveChanges();

                SwitchLineStatus(Line, false);

                return ConvertToJsonString(new
                {
                    ID = dt.ID,
                    EventStart = (dt.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt")),
                    EventStop = "",
                    ReasonCodeID = dt.ReasonCodeID,
                    ReasonCode = dt.ReasonCode,
                    Minutes = (dt.Minutes.Value),
                    Line = dt.Line,
                    Comment = dt.Comment
                });
            }
        }

        public string UpdateDTWithNoStop()
        {
            DateTime es;

            int id;

            if (!DateTime.TryParse(request.Params["es"], out es))
            {
                return null;
            }

            if (!int.TryParse(request.Params["id"], out id))
            {
                return null;
            }


            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                DowntimeData dt = (from o in db.DowntimeDataSet
                                   where o.ID == id
                                   select o).FirstOrDefault();

                if (dt == null)
                    return ConvertToJsonString(new
                    {
                        ID = -1,
                        EventStart = "",
                        EventStop = "",
                        ReasonCodeID = -1,
                        ReasonCode = "",
                        Minutes = 0,
                        Line = Line,
                        Comment = ""
                    });
                dt.EventStop = es;
                db.SaveChanges();

                if (dt.EventStart != null && dt.Minutes != null)
                    return ConvertToJsonString(new
                    {
                        ID = dt.ID,
                        EventStart = dt.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                        EventStop = "",
                        ReasonCodeID = dt.ReasonCodeID,
                        ReasonCode = dt.ReasonCode,
                        Minutes = dt.Minutes.Value,
                        Line = dt.Line,
                        Comment = dt.Comment
                    });

                return ConvertToJsonString(new {});
            }
        }

        public string GetPerformanceChart()
        {
            return string.Empty;
        }

        public string GetTotalScheduledMinutes()
        {
            DateTime sd;
            DateTime ed;
            double totalMinutes = 0;
            const string reason = "Scheduled DT";

            if (!DateTime.TryParse(request.Params["sd"], out sd) || !DateTime.TryParse(request.Params["ed"], out ed))
            {
                return null;
            }


            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeDataSet
                        where o.Client == _currentClient
                        && (((o.EventStart >= sd) && (o.EventStart <= ed)) || ((o.EventStop >= sd) && (o.EventStop <= ed)))
                        && (o.Line == Line)
                        && o.ReasonCode == reason
                        orderby o.EventStart ascending
                        select o;

                List<DowntimeData> downtime = q.ToList();
                double difference = 0;

                foreach (DowntimeData dt in downtime)
                {
                    if (dt.Minutes == null) continue;

                    double minutes = Convert.ToDouble(dt.Minutes.Value);

                    if (dt.EventStart < sd)
                    {
                        if (dt.EventStart != null) difference += sd.Subtract(dt.EventStart.Value).TotalMinutes;
                    }
                        
                    if (dt.EventStop > ed)
                    {
                        if (dt.EventStop != null) difference += dt.EventStop.Value.Subtract(ed).TotalMinutes;
                    }

                    totalMinutes += minutes;
                }

                totalMinutes = totalMinutes - difference;
            }

            return totalMinutes.ToString(CultureInfo.InvariantCulture);

        }

        public string GetLines()
        {
            using(DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = (from o in db.LineStatus
                         where o.Client == _currentClient
                         select o.Line).Distinct();

                List<object> results = new List<object>();

                List<string> lines = q.ToList();

                foreach (string line in lines)
                {
                    results.Add(new 
                    {
                        Line = line
                    });
                }

                return ConvertToJsonString(results);
            }
        }

        public string GetDowntime()
        {
            DateTime sd;
            DateTime ed;
            string group = request.Params["group"];

            if (!DateTime.TryParse(request.Params["sd"], out sd))
            {
                return null;
            }

            if (!DateTime.TryParse(request.Params["ed"], out ed))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(group))
            {
                switch (group.ToLower())
                {
                    case "hours":
                    {
                        //limit 48hours
                        if (ed.Subtract(sd).TotalHours > 48)
                        {
                            sd = ed.AddHours(-48);
                        }
                    }
                    break;
                    case "days":
                    {
                        if ((ed.Subtract(sd).TotalDays) > 30)
                        {
                            sd = ed.AddDays(-30);
                        }

                    }
                    break;
                    case "weeks":
                    {
                        //limit 52 weeks
                        if ((ed.Subtract(sd).TotalDays / 7) > 52)
                        {
                            sd = ed.AddDays(-(52 * 7));
                        }
                    }
                    break;
                    case "months":
                    {
                        //limit 36 months
                        if (sd < ed.AddYears(-3))
                        {
                            sd = ed.AddYears(-3);
                        }
                    }
                    break;
                    case "timeline":
                    {
                        if (sd < ed.AddYears(-1))
                        {
                            sd = ed.AddYears(-1);
                        }
                    }
                    break;
                }
            }

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeDataSet
                        where o.Client == _currentClient
                        && (o.EventStart >= sd)
                        && (o.EventStart <= ed)
                        && (o.Line == Line)
                        orderby o.EventStart ascending
                        select o;


                Dictionary<DateTime, object> dict = new Dictionary<DateTime, object>();
                List<DowntimeData> downtime = q.ToList();

                foreach (DowntimeData d in downtime)
                {
                    if (!d.EventStop.HasValue || !d.EventStart.HasValue) continue;

                    if (!dict.ContainsKey(d.EventStop.Value))
                    {
                        dict.Add(d.EventStop.Value, new
                        {
                            Id = d.ID,
                            EventStart = d.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            EventStop = d.EventStop.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                            Comment = d.Comment,
                            Line = d.Line,
                            Minutes = d.Minutes,
                            ReasonCode = d.ReasonCode,
                            ReasonCodeId = d.ReasonCodeID,
                            Client = d.Client

                        });
                    }
                }

                var results = dict.OrderBy(o => o.Key).Select(o => o.Value).ToList();

                return ConvertToJsonString(results);

            }
        }

        public string GetCaseCount()
        {
            DateTime sd;
            DateTime ed;
            string group = request.Params["group"];

            if (!DateTime.TryParse(request.Params["sd"], out sd))
            {
                return null;
            }

            if (!DateTime.TryParse(request.Params["ed"], out ed))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(group))
            {
                switch (group.ToLower())
                {
                    case "hours":
                        {
                            //limit 48hours
                            if (ed.Subtract(sd).TotalHours > 48)
                            {
                                sd = ed.AddHours(-48);
                            }
                        }
                        break;
                    case "days":
                        {
                            if ((ed.Subtract(sd).TotalDays) > 30)
                            {
                                sd = ed.AddDays(-30);
                            }

                        }
                        break;
                    case "weeks":
                        {
                            //limit 52 weeks
                            if ((ed.Subtract(sd).TotalDays / 7) > 52)
                            {
                                sd = ed.AddDays(-(52 * 7));
                            }
                        }
                        break;
                    case "months":
                        {
                            //limit 36 months
                            if (sd < ed.AddYears(-3))
                            {
                                sd = ed.AddYears(-3);
                            }
                        }
                        break;
                }
            }

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.CaseCountSet
                        where o.Client == _currentClient
                        && (o.EventStop >= sd)
                        && (o.EventStop <= ed)
                        && (o.Line == Line)
                        select o;

                List<CaseCount> casecount = q.ToList();

                Dictionary<DateTime, CaseCountModel> dictionary = new Dictionary<DateTime, CaseCountModel>();

                if (casecount.Count > 0)
                {

                    List<ThroughputHistory> histories = DCSDashboardDemoHelper.getThroughPutHistories(Line);
                    List<Throughput> throughputs = DCSDashboardDemoHelper.GetThroughPuts(Line);

                    DateTime start = casecount.First().EventStop.Value;
                    DateTime stop = casecount.Last().EventStop.Value;

                    int diff = (int)stop.Subtract(start).TotalHours;

                    if (diff < 0)
                        diff *= -1;

                    DateTime d = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0);

                    for (int x = 0; x < diff; x++)
                    {
                        List<CaseCount> tmpList = (from o in casecount
                            where o.EventStop != null && ((o.EventStop.Value.Year == d.Year && o.EventStop.Value.Month == d.Month && o.EventStop.Value.Day == d.Day)
                                                          && o.EventStop.Value.Hour >= d.Hour && o.EventStop.Value.Hour <= d.Hour)
                            select o).ToList();

                        if (tmpList.Count > 0)
                        {
                            CaseCount first = tmpList.First();
                            CaseCount last = tmpList.Last();


                            int tp1Id = DiamondCrystaldashboardHelper.GetThroughputIdFromReference(histories.Where(o => first.EventStop != null && o.Date <= first.EventStop.Value).OrderByDescending(o => o.Date).Select(o => o.ThroughputReference).FirstOrDefault());

                            DiamondCrystaldashboardHelper.GetThroughputIdFromReference(histories.Where(o => last.EventStop != null && o.Date <= last.EventStop.Value).OrderByDescending(o => o.Date).Select(o => o.ThroughputReference).FirstOrDefault());

                            decimal perHour1 = 0M;

                            Throughput tp1 = (from o in throughputs
                                where o.Id == tp1Id
                                select o).FirstOrDefault();

                            if (tp1 != null)
                                perHour1 = tp1.PerHour;

                            if (first.EventStop != null)
                            {
                                CaseCountModel m1 = new CaseCountModel
                                {
                                    Id = first.Id,
                                    EventStart = (first.EventStart.HasValue ? first.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt") : "null"),
                                    EventStop = first.EventStop.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                    Line = first.Line,
                                    CaseCount = first.CaseCount1,
                                    Client = first.Client,
                                    Throughput = perHour1

                                };

                                if(!dictionary.ContainsKey(first.EventStop.Value))
                                    dictionary.Add(first.EventStop.Value, m1);
                            }

                            if (last.EventStop != null)
                            {
                                CaseCountModel m2 = new CaseCountModel
                                {
                                    Id = last.Id,
                                    EventStart = (last.EventStart.HasValue ? last.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt") : "null"),
                                    EventStop = last.EventStop.Value.ToString("MM/dd/yyyy hh:mm:ss tt"),
                                    Line = last.Line,
                                    CaseCount = last.CaseCount1,
                                    Client = last.Client,
                                    Throughput = perHour1

                                };

                                if(!dictionary.ContainsKey(last.EventStop.Value))
                                    dictionary.Add(last.EventStop.Value, m2);
                            }
                        }

                        d = d.AddHours(1);
                    }
                }

                //results = results.OrderBy(o => o.EventStop).ToList();
                var results = dictionary.OrderBy(o => o.Key).Select(o => o.Value).ToList();

                return ConvertToJsonString(results);
            }
        }

        public void TurnLights()
        {
            bool on;
            
            bool.TryParse(request.Form["on"], out on);

            if (string.IsNullOrEmpty(request.Form["on"]))
            {
                bool status = DiamondCrystaldashboardHelper.getLightsOn(Line);

                //Turn the opposite
                @on = !status;
            }

            DiamondCrystaldashboardHelper.turnLights(on, Line);

        }

        public string GetLights()
        {
            return DiamondCrystaldashboardHelper.getLightsOn(Line).ToString();
        }

        public Throughput GetLatestThroughput()
        {
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {

                var p = (from o in db.ThroughputHistory
                         where o.Date <= DateTime.Now
                         && o.Client == _currentClient
                         orderby o.Date descending, o.Id descending
                         select o.Throughput).FirstOrDefault();
                return p;
            }
        }

        public Throughput GetLatestThroughput(DateTime date)
        {
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var p = (from o in db.ThroughputHistory
                         where o.Date <= date
                         && o.Client == _currentClient
                         orderby o.Date descending, o.Id descending
                         select o.Throughput).FirstOrDefault();
                return p;
            }
        }

        public string GetThroughPuts()
        {
            List<Throughput> tpList = DCSDashboardDemoHelper.GetThroughPuts(Line);
            List<object> results = new List<object>();

            tpList = tpList.Where(o => o.Active).OrderBy(o => o.Name).ToList();

            foreach (Throughput tp in tpList)
            {
                results.Add(new
                {
                    tp.Id, tp.Name, tp.Description, tp.PerHour
                });
            }

            return ConvertToJsonString(results);

        }

        public string GetOptionInfo()
        {
            List<OptionInfo> opInfoList = DCSDashboardDemoHelper.GetOptionInfo();
            List<Options> opList = DCSDashboardDemoHelper.GetOptions();
            bool showDisabled;
            bool getChildren;

            bool.TryParse(request.Params["showdisabled"], out showDisabled);
            bool.TryParse(request.Params["getchildren"], out getChildren);

            List<object> results = (from op in opInfoList  where op.Enabled || (!op.Enabled && showDisabled)
                select new
                {
                    Id = op.Id, Name = (!string.IsNullOrEmpty(op.Name) ? op.Name : op.Number.ToString()), Number = op.Number, Enabled = op.Enabled, IsRequired = op.IsRequired, Options = (getChildren ? (from b in opList
                        where b.Number == op.Number
                        select new
                        {
                            b.Id, b.Number, b.Description, b.Client, b.Name
                        }).ToArray() : null)
                }).Cast<object>().ToList();

            return ConvertToJsonString(results);

        }

        public string GetOptions()
        {

            List<Options> opList = DCSDashboardDemoHelper.GetOptions();
            List<object> results = new List<object>();

            foreach (Options op in opList)
            {
                results.Add(new
                {
                    op.Id,
                    op.Name,
                    op.Description,
                    op.Number
                });
            }

            return ConvertToJsonString(results);
        }

        public string GetOption()
        {
            int number;

            int.TryParse(request.Form["number"], out number);

            List<Options> opList = new List<Options>();

            if(number > -1)
                opList = DCSDashboardDemoHelper.GetOptions(number);

            List<object> results = new List<object>();

            foreach (Options op in opList)
            {
                results.Add(new
                {
                    op.Id,
                    op.Name,
                    op.Description,
                    op.Number
                });
            }

            return ConvertToJsonString(results);
        }

        public string CreateThroughPut()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int perHour;

            int.TryParse(request.Form["perhour"], out perHour);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) || perHour <= -1) return "FAILED";
            int results = DCSDashboardDemoHelper.CreateThroughPuts(name, description, perHour);

            return results >= 0 ? "SUCCESS" : "FAILED";
        }

        public string CreateOption()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int number;

            int.TryParse(request.Form["number"], out number);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) || number <= -1) return "FAILED";

            int results = DCSDashboardDemoHelper.CreateOption(name, description, number);

            if (results >= 0)
                return "SUCCESS";

            return "FAILED";
        }

        public string UpdateThroughPut()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int perHour;
            int id;

            int.TryParse(request.Form["perhour"], out perHour);
            int.TryParse(request.Form["id"], out id);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) || perHour <= -1 || id <= -1)
                return "FAILED";

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                Throughput tp = (from o in db.Throughput
                    where o.Id == id
                    select o).FirstOrDefault();


                if (tp == null) return "FAILED";

                tp.Name = name;
                tp.Description = description;
                tp.PerHour = perHour;
                        
                int results = db.SaveChanges();

                if (results >= 0)
                    return "SUCCESS";
            }

            return "FAILED";

        }

        public string UpdateOption()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int number;
            int id;

            int.TryParse(request.Form["id"], out id);
            int.TryParse(request.Form["number"], out number);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) || id <= -1 || number <= -1)
                return "FAILED";

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                Options tp = (from o in db.Options
                    where o.Id == id
                    select o).FirstOrDefault();

                if (tp == null) return "FAILED";
                tp.Name = name;
                tp.Description = description;
                tp.Number = number;

                int results = db.SaveChanges();

                if (results >= 0)
                    return "SUCCESS";
            }

            return "FAILED";

        }

        public string DeleteThroughPut()
        {
            int id;

            int.TryParse(request.Form["id"], out id);

            if (id <= -1) return "FAILED";

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                Throughput tp = (from o in db.Throughput
                    where o.Id == id
                    select o).FirstOrDefault();

                if (tp == null) return "FAILED";
                tp.Active = false;

                int results = db.SaveChanges();

                if (results >= 0)
                    return "SUCCESS";
            }

            return "FAILED";

        }

        public string DeleteOption()
        {
            int id;

            int.TryParse(request.Form["id"], out id);

            if (id <= -1) return "FAILED";
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                Options tp = (from o in db.Options
                    where o.Id == id
                    select o).FirstOrDefault();

                if (tp == null) return "FAILED";
                db.DeleteObject(tp);
                int results = db.SaveChanges();

                if (results >= 0)
                    return "SUCCESS";
            }

            return "FAILED";

        }

        public string ConvertToJsonString(object obj)
        {
            JsonSerializer js = new JsonSerializer();
            js.Converters.Add(new JavaScriptDateTimeConverter());
            System.IO.TextWriter tw = new System.IO.StringWriter();
            js.Serialize(tw, obj);
            return tw.ToString();

        }
        
        private string saveAllPostRows()
        {
            List<PostRowData> dd = getPostRowDatas();
            if (dd == null) return "no data";

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                DowntimeReason dr;
                foreach (var item in dd)
                {

                    var levels = item.Path.Split('>');
                    if (levels.Length < 3) Array.Resize(ref levels, 3);
                    if (string.IsNullOrEmpty(levels[0]) && string.IsNullOrEmpty(levels[1]) && string.IsNullOrEmpty(levels[2])) continue;

                    if (item.Id <= 0)
                    {
                        dr = new DowntimeReason
                        {
                            Level1 = levels[0],
                            Level2 = levels[1],
                            Level3 = levels[2],
                            Duration = item.Duration,
                            Client = _currentClient,
                            HideReasonInReports = item.HideReasonInReports,
                            Line = (!string.IsNullOrEmpty(Line) ? Line : "company-demo"),
                            IsChangeOver = item.IsChangeOver
                        };
                        db.AddToDowntimeReasonSet(dr);
                    }
                    else
                    {
                        var q = from o in db.DowntimeReasonSet
                                where o.ID == item.Id
                                && o.Client == _currentClient
                                select o;
                        dr = q.FirstOrDefault();

                        if (dr == null) continue;

                        if ((string.IsNullOrEmpty(dr.Line) || dr.Line == "company-demo") && (!string.IsNullOrEmpty(Line) && Line != "company-demo"))//assumes it is a global reason code and they're adding a Line specific
                        {
                            DowntimeReason reason = new DowntimeReason
                            {
                                Level1 = levels[0],
                                Level2 = levels[1],
                                Level3 = levels[2],
                                Duration = item.Duration,
                                HideReasonInReports = item.HideReasonInReports,
                                Line = (!string.IsNullOrEmpty(Line) ? Line : "company-demo"),
                                IsChangeOver = item.IsChangeOver,
                                Client = _currentClient
                            };

                            db.AddToDowntimeReasonSet(reason);
                        }
                        else
                        {
                            dr.Level1 = levels[0];
                            dr.Level2 = levels[1];
                            dr.Level3 = levels[2];
                            dr.Duration = item.Duration;
                            dr.HideReasonInReports = item.HideReasonInReports;
                            dr.Line = (!string.IsNullOrEmpty(Line) ? Line : "company-demo");
                            dr.IsChangeOver = item.IsChangeOver;
                        }
                    }
                }

                //delete rows
                foreach (var item in getPostRowDatasDeleteRows())
                {
                     var q = from o in db.DowntimeReasonSet
                                where o.ID == item
                                && o.Client == _currentClient
                                select o;
                        dr = q.FirstOrDefault();
                    if (dr == null) continue;
                    if(dr.Line == Line)//Don't want to accidently delete another Line's reason code
                        db.DeleteObject(dr);
                }

                db.SaveChanges();
                return "true";
            }
        }

        public List<PostRowData> getPostRowDatas()
        {
            JsonSerializer js = new JsonSerializer();
            js.Converters.Add(new JavaScriptDateTimeConverter());
            JsonTextReader tr = new JsonTextReader(new System.IO.StringReader(request.Form["data"]));
            object obj=js.Deserialize(tr, typeof(List<PostRowData>));
            try
            {
                return (List<PostRowData>)obj;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public List<int> getPostRowDatasDeleteRows()
        {
            List<int> result = new List<int>();
            string s=request.Form["dels"];
            if (string.IsNullOrEmpty(s)) return new List<int>();

            foreach (var item in s.Split(','))
            {
                result.Add(Convert.ToInt32(item));   
            }
            return result;
        }

        public List<DowntimeReason> getTreeTable()
        {
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                if (string.IsNullOrEmpty(Line) || Line == "company-demo")
                {
                    var q = from o in db.DowntimeReasonSet
                            where o.Client == _currentClient
                            && (o.Line == null || o.Line == "" || o.Line == Line)
                            orderby o.Level1, o.Level2, o.Level3
                            select o;
                    return q.ToList();
                }
                else
                {
                    List<DowntimeReason> reasons = new List<DowntimeReason>();

                    var q = from o in db.DowntimeReasonSet
                            where o.Client == _currentClient
                            && (o.Line == Line || o.Line == "company-demo")
                            orderby o.Level1, o.Level2, o.Level3
                            select o;

                    List<DowntimeReason> allReasons = q.ToList();

                    List<DowntimeReason> companyDemoReasons = (from o in allReasons
                                                               where o.Line == "company-demo" || o.Line == null || o.Line == ""
                                                               select o).ToList();

                    List<DowntimeReason> lineReasons = (from o in allReasons
                                                        where !(from c in companyDemoReasons
                                                                select c.ID)
                                                                .Contains(o.ID)
                                                        select o).ToList();

                    foreach (DowntimeReason lineReason in lineReasons)
                    {
                        DowntimeReason existingReason = (from o in reasons
                                                         where o.Level1 == lineReason.Level1
                                                         && ((string.IsNullOrEmpty(lineReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == lineReason.Level2)
                                                         && ((string.IsNullOrEmpty(lineReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == lineReason.Level3)
                                                         && o.Line == lineReason.Line
                                                         select o).FirstOrDefault();

                        if (existingReason == null)
                            reasons.Add(lineReason);
                    }

                    foreach (DowntimeReason companyDemoReason in companyDemoReasons)
                    {
                        DowntimeReason lineReason = (from o in allReasons
                                                     where o.Level1 == companyDemoReason.Level1
                                                     && ((string.IsNullOrEmpty(companyDemoReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == companyDemoReason.Level2)
                                                     && ((string.IsNullOrEmpty(companyDemoReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == companyDemoReason.Level3)
                                                     && o.Line != companyDemoReason.Line
                                                     select o).FirstOrDefault();

                        if (lineReason == null)//One doesn't exist
                        {
                            if(!reasons.Contains(null))
                                reasons.Add(companyDemoReason);
                        }
                        else
                        {
                            if (!reasons.Contains(lineReason))
                                reasons.Add(companyDemoReason);
                        }
                    }

                    return reasons;
                }
            }
        }

        public string getLevelDataSourceLength(string level1, string level2)
        {
            if(string.Equals(level2,"undefined",StringComparison.CurrentCultureIgnoreCase))level2=string.Empty;

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {

                if (!string.IsNullOrEmpty(level1) && !string.IsNullOrEmpty(level2))
                {
                    var q = from o in db.DowntimeReasonSet
                            where o.Level1 == level1
                                 && o.Level2==level2
                                 && !string.IsNullOrEmpty(o.Level3)
                                 && o.Client == _currentClient
                                 && (o.Line == null || o.Line == "" || o.Line == Line || o.Line == "company-demo")
                            select o;


                    return q.Count().ToString();
                }
                else
                {
                    var q = from o in db.DowntimeReasonSet
                            where o.Level1 == level1
                                && (!string.IsNullOrEmpty(o.Level2))
                                && o.Client == _currentClient
                                && (o.Line == null || o.Line == "" || o.Line == Line || o.Line == "company-demo")
                            select o;


                    return q.Count().ToString();
                }

            }
        }

        private object getEvent()
        {
            int id;
            int.TryParse(request.Form["id"], out id);
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeDataSet
                        where o.ID == id
                        && o.Client == _currentClient
                        && (o.Line == null || o.Line == "" || o.Line == Line)
                        select o;

                DowntimeData dd = q.FirstOrDefault<DowntimeData>();
                if (dd == null)
                {
                    return null;
                }

                int tpId = 0;
                int opId = 0;
                int op2Id = 0;

                var reasonCode = (from o in db.DowntimeReasonSet
                    where o.ID == dd.ReasonCodeID
                    select o).FirstOrDefault();

                if (reasonCode == null)
                    return new
                    {
                        ID = dd.ID,
                        EventStart = dd.EventStart.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                        EventStop = (dd.EventStop.HasValue ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss") : ""),
                        ReasonCode = dd.ReasonCode,
                        ReasonCodeID = dd.ReasonCodeID,
                        Line = dd.Line,
                        Minutes = dd.Minutes,
                        Client = dd.Client,
                        Comment = dd.Comment,
                        EventStartDisplay =
                            dd.EventStart.Value.ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                        EventStopDisplay =
                            (dd.EventStop.HasValue
                                ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy hh:mmtt",
                                    CultureInfo.GetCultureInfo("en-US"))
                                : ""),
                        Throughput = tpId,
                        Option1 = opId,
                        Option2 = op2Id
                    };
                if (reasonCode.IsChangeOver)
                {
                    var p = (from o in db.ThroughputHistory
                        where o.Date <= dd.EventStop
                              && o.Client == _currentClient
                              && o.Line == dd.Line
                        orderby o.Date descending, o.Id descending
                        select new { Tp = (o.Throughput != null ? o.Throughput.Id : 0), Op1 = (o.Options != null ? o.Options.Id : 0), Op2 = (o.Options1 != null ? o.Options1.Id : 0) }).FirstOrDefault();

                    if (p == null)
                        return new
                        {
                            ID = dd.ID,
                            EventStart = dd.EventStart.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                            EventStop =
                                (dd.EventStop.HasValue ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss") : ""),
                            ReasonCode = dd.ReasonCode,
                            ReasonCodeID = dd.ReasonCodeID,
                            Line = dd.Line,
                            Minutes = dd.Minutes,
                            Client = dd.Client,
                            Comment = dd.Comment,
                            EventStartDisplay =
                                dd.EventStart.Value.ToString(@"MM\/dd\/yyyy hh:mmtt",
                                    CultureInfo.GetCultureInfo("en-US")),
                            EventStopDisplay =
                                (dd.EventStop.HasValue
                                    ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy hh:mmtt",
                                        CultureInfo.GetCultureInfo("en-US"))
                                    : ""),
                            Throughput = tpId,
                            Option1 = opId,
                            Option2 = op2Id
                        };
                    tpId = p.Tp;
                    opId = p.Op1;
                    op2Id = p.Op2;
                }
                else
                {
                    var p = (from o in db.NatureOfStoppages
                        join b in db.Options
                            on o.OptionId equals b.Id
                        where o.DowntimeId == dd.ID
                        select new { OptionId = b.Id, Number = b.Number, Id = o.Id, Name = b.Name }).ToList();

                    if (p.Count <= 0)
                        return new
                        {
                            ID = dd.ID,
                            EventStart = dd.EventStart.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                            EventStop =
                                (dd.EventStop.HasValue ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss") : ""),
                            ReasonCode = dd.ReasonCode,
                            ReasonCodeID = dd.ReasonCodeID,
                            Line = dd.Line,
                            Minutes = dd.Minutes,
                            Client = dd.Client,
                            Comment = dd.Comment,
                            EventStartDisplay =
                                dd.EventStart.Value.ToString(@"MM\/dd\/yyyy hh:mmtt",
                                    CultureInfo.GetCultureInfo("en-US")),
                            EventStopDisplay =
                                (dd.EventStop.HasValue
                                    ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy hh:mmtt",
                                        CultureInfo.GetCultureInfo("en-US"))
                                    : ""),
                            Throughput = tpId,
                            Option1 = opId,
                            Option2 = op2Id
                        };
                    foreach (var o in p)
                    {
                        switch (o.Number)
                        {
                            case 1:
                                opId = o.OptionId;
                                break;
                            case 2:
                                op2Id = o.OptionId;
                                break;
                        }
                    }
                }

                return new
                {
                    ID = dd.ID,
                    EventStart = dd.EventStart.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                    EventStop = (dd.EventStop.HasValue ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss") : ""),
                    ReasonCode = dd.ReasonCode,
                    ReasonCodeID = dd.ReasonCodeID,
                    Line = dd.Line,
                    Minutes = dd.Minutes,
                    Client = dd.Client,
                    Comment = dd.Comment,
                    EventStartDisplay = dd.EventStart.Value.ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                    EventStopDisplay = (dd.EventStop.HasValue ? dd.EventStop.Value.ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")) : ""),
                    Throughput = tpId,
                    Option1 = opId,
                    Option2 = op2Id
                };
            }
        }

        private void SwitchLineStatus(string line, bool what)
        {

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                LineStatus stat = (from o in db.LineStatus
                                   where o.Line == line && o.Client == _currentClient
                                   select o).FirstOrDefault();

                if (stat != null)
                    stat.Status = what;

                db.SaveChanges();

            }
        }

        private void addNewEvent(HttpContext context)
        {
            //reasonId&reasonCode&comment&startdatetime&minutes&line
            int reasonId, occurences;

            int throughputId, option1Id, option2Id = -1;

            DateTime startdatetime;
            decimal minutes;
            var reasonCode = request.Form["reasonCode"];
            var comment = request.Form["comment"];
            var line = request.Form["line"];

            line = line.Replace('#', ' ');

            var client = _currentClient;

            int.TryParse(request.Form["Occurences"], out occurences);
            if (occurences == 0) occurences = 1;


            int.TryParse(request.Form["throughput"], out throughputId);
            int.TryParse(request.Form["option1"], out option1Id);
            int.TryParse(request.Form["option2"], out option2Id);

            if(!DateTime.TryParse(request.Form["startdatetime"],out startdatetime))
            {
                context.Response.Write("start date time is empty.");
                return;
            }


            if(!int.TryParse(request.Form["reasonId"],out reasonId))
            {
                context.Response.Write("reason id is empty.");
                return;
            }

            if(!decimal.TryParse(request.Form["minutes"],out minutes))
            {
                context.Response.Write("minutes id is empty.");
                return;
            }

            if(string.IsNullOrEmpty(line))
            {
                context.Response.Write("line is empty.");
                return;
            }

            if(string.IsNullOrEmpty(reasonCode))
            {
                context.Response.Write("reasonCode is empty.");
                return;
            }

            var enddatetime = startdatetime.AddMinutes(Convert.ToDouble(minutes));


            bool result = false;
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                int count = 0;

                DowntimeReason reason = (from o in db.DowntimeReasonSet
                                         where o.ID == reasonId
                                         select o).FirstOrDefault();

                if (reason == null)
                    context.Response.Write("Reason Code doesn't exist");

                if (throughputId > 0 && reason.IsChangeOver)
                {
                    var throughput = (from o in db.Throughput where o.Id == throughputId select o).FirstOrDefault();
                    reasonCode = string.Format("{0} [{1}]", reasonCode, throughput.Name);
                }

                List<DowntimeData> dts = new List<DowntimeData>();

                for (int cpnum = count; cpnum < occurences; cpnum++)
                {
                    DowntimeData dd = new DowntimeData();
                    dd.EventStart = startdatetime;
                    dd.EventStop = enddatetime;
                    dd.Minutes = minutes;
                    dd.ReasonCodeID = reasonId;
                    dd.ReasonCode = reasonCode;
                    dd.Line = line;
                    dd.Comment = comment;
                    dd.Client = client;
                    dd.IsCreatedByAcromag = false;
                    db.AddToDowntimeDataSet(dd);

                    dts.Add(dd);
                }

                int updatedRecords = db.SaveChanges();

                if (throughputId > 0 && reason.IsChangeOver)
                {
                    DCSDashboardDemoHelper.CreateThroughPutHistory(throughputId, option1Id, option2Id, enddatetime, line);
                }
                else if(option1Id > 0 || option2Id > 0)
                {
                    foreach (DowntimeData dt in dts)
                    {
                        if (dt.ID <= 0) continue;

                        if (option1Id > 0)
                            DCSDashboardDemoHelper.CreateNOS(dt.ID, option1Id);

                        if (option2Id > 0)
                            DCSDashboardDemoHelper.CreateNOS(dt.ID, option2Id);
                    }
                }

                if (updatedRecords > 0)
                    result = true;
                else if (updatedRecords == 0 && count == 1)
                    result = true;
                else if (updatedRecords == 0 && count == 0)
                    result = false;

            }
            
            context.Response.Write(result.ToString().ToLower());
        }

        private List<object> LoadGridData()
        {
            DateTime? sd = null, ed = null;
            DateTime tmp;
            if (DateTime.TryParse(request["startDate"], out tmp))
            {
                sd = tmp;
            }

            if (DateTime.TryParse(request["endDate"], out tmp))
            {
                ed = tmp;
            }

            string line = string.Empty;
            
            line = request["line"];

            // 远程电脑与服务器差两个小时
            // 远程电脑上的 10:00，到服务器上是 8:00
            DateTime stopd = DateTime.Now.AddDays(-7).AddHours(2);

            //默认8小时内的
            if (!sd.HasValue && !ed.HasValue)
            {
                sd = DateTime.Now.AddHours(-7);
                ed = DateTime.Now.AddHours(17);

            }

            if (ed.HasValue)
            {
                stopd = ed.Value;
            }

            List<object> result = new List<object>();
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {


                if (string.IsNullOrEmpty(line))
                {
                    List<DowntimeReason> reasons = (from o in db.DowntimeReasonSet
                                                    where o.Client == _currentClient
                                                    && o.Line == Line
                                                    select o).ToList();

                    var q = from o in db.DowntimeDataSet
                            where o.Client == _currentClient
                            && (!sd.HasValue || o.EventStart >= sd.Value)
                            && ((ed.HasValue ? (o.EventStart.Value <= stopd) : (o.EventStop.Value >= stopd)))//(o.EventStop.Value >= stopd)
                            && ((o.Line == null || o.Line == "" || o.Line == "company-demo"))
                            orderby o.EventStart ascending
                            select o;


                    foreach (var item in q.ToList())
                    {
                        bool? isChangeOver = (from o in reasons
                                             where o.ID == item.ReasonCodeID
                                             select o.IsChangeOver).FirstOrDefault();



                        if(isChangeOver == true)
                        {
                            Throughput tp = GetLatestThroughput(item.EventStop.Value);
                            item.ReasonCode = item.ReasonCode + " - " + (tp != null ? tp.Name : "");
                        }


                        result.Add(
                            new
                            {
                                ID = item.ID,
                                EventStart = item.EventStart.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                                EventStop = item.EventStop.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                                ReasonCode = item.ReasonCode,
                                ReasonCodeID = item.ReasonCodeID,
                                Line = item.Line,
                                Minutes = item.Minutes,
                                Client = item.Client,
                                Comment = item.Comment,
                                EventStartDisplay = item.EventStart.Value.ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                                EventStopDisplay = item.EventStop.Value.ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                                IsChangeOver = (isChangeOver.HasValue ? isChangeOver : false)
                            });
                    }
                }
                else
                {
                    Stopwatch section = new Stopwatch();
                    section.Start();
                    List<DownTimeDataRecord> items = null;
                    items = ed.HasValue ? DAL.GetDowntimeData(_currentClient, Line, sd, stopd) : DAL.GetDowntimeDataStopAfterEndDate(_currentClient, Line, sd, stopd);
                    section.Stop();

                    section.Restart();
                    object historiesObj = HttpContext.Current.Cache["Histories"+_currentClient];
                    if (historiesObj == null)
                    {
                        historiesObj = DAL.GetHistories(_currentClient);
                        HttpContext.Current.Cache.Add("Histories" + _currentClient, historiesObj, null, DateTime.Now.AddMinutes(2), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                    }

                    List<ThroughputHistory> histories = (List<ThroughputHistory>)historiesObj;

                    
                    section.Stop();
                    section.Restart();

                    foreach (var item in items)
                    {
                        if (item.IsChangeOver != true) continue;
                        Throughput tp = (from o in histories where o.Line == item.Line && o.Date <= DateTime.Parse(item.EventStop) orderby o.Date descending select o.Throughput).FirstOrDefault();

                        if (tp != null)
                        {
                            item.ReasonCode = item.ReasonCode + " - " + (tp.Name);
                        }
                    }
                    result.AddRange(items);


                    section.Stop();
                }
            }
            return result;
        }

        private List<object> GetLevel1()
        {
            List<object> result = new List<object>();
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeReasonSet
                        where o.Client == _currentClient
                        && (o.Line == null || o.Line == "" || o.Line == Line || o.Line == "company-demo")
                        && o.Level1 != null && o.Level1 != ""
                        orderby o.Level1, o.Level2, o.Level3
                        select o;

                List<DowntimeReason> reasons = new List<DowntimeReason>();

                List<DowntimeReason> allReasons = q.ToList();

                List<DowntimeReason> companyDemoReasons = (from o in allReasons
                                                           where o.Line == "company-demo" || o.Line == null || o.Line == ""
                                                           select o).ToList();

                List<DowntimeReason> lineReasons = (from o in allReasons
                                                    where !(from c in companyDemoReasons
                                                            select c.ID)
                                                            .Contains(o.ID)
                                                    select o).ToList();

                foreach (DowntimeReason lineReason in lineReasons)
                {
                    DowntimeReason existingReason = (from o in reasons
                                                     where o.Level1 == lineReason.Level1
                                                     && ((string.IsNullOrEmpty(lineReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == lineReason.Level2)
                                                     && ((string.IsNullOrEmpty(lineReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == lineReason.Level3)
                                                     && o.Line == lineReason.Line
                                                     select o).FirstOrDefault();

                    if (existingReason == null)
                        reasons.Add(lineReason);
                }

                foreach (DowntimeReason companyDemoReason in companyDemoReasons)
                {
                    DowntimeReason lineReason = (from o in allReasons
                                                 where o.Level1 == companyDemoReason.Level1
                                                 && ((string.IsNullOrEmpty(companyDemoReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == companyDemoReason.Level2)
                                                 && ((string.IsNullOrEmpty(companyDemoReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == companyDemoReason.Level3)
                                                 && o.Line != companyDemoReason.Line
                                                 select o).FirstOrDefault();

                    if (lineReason == null)//One doesn't exist
                    {
                        if (!reasons.Contains(lineReason))
                            reasons.Add(companyDemoReason);
                    }
                    else
                    {
                        if (!reasons.Contains(lineReason))
                            reasons.Add(companyDemoReason);
                    }
                }

                var q1 = from o in reasons
                        group o by o.Level1 into g
                        select new
                        {
                            g.Key,
                            Id = g.Min(o => o.ID),
                            IsChangeOver = g.Select(o => o.IsChangeOver).FirstOrDefault(),
                            IsPlanned = g.Select(o => o.HideReasonInReports).FirstOrDefault(),
                            Duration = g.Select(o => o.Duration).FirstOrDefault()
                        };

                foreach (var item in q1.ToList())
                {
                    result.Add(new { ID = item.Id, Level1 = item.Key, IsChangeOver = item.IsChangeOver, IsPlanned = reasons.Where(o => o.Level1 == item.Key && o.Level2 == null).Select(o => o.HideReasonInReports).FirstOrDefault(), Duration = item.Duration });
                }
            }

            return result;
        }

        private List<object> GetLevel2(string level1)
        {
            List<object> result = new List<object>();
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeReasonSet
                        where o.Client == _currentClient
                        && (o.Line == null || o.Line == "" || o.Line == Line || o.Line == "company-demo")
                        && o.Level1 == level1
                        && o.Level2 != null && o.Level2 != ""
                        orderby o.Level1, o.Level2, o.Level3
                        select o;

                List<DowntimeReason> reasons = new List<DowntimeReason>();

                List<DowntimeReason> allReasons = q.ToList<DowntimeReason>();

                List<DowntimeReason> companyDemoReasons = (from o in allReasons
                                                           where o.Line == "company-demo" || o.Line == null || o.Line == ""
                                                           select o).ToList();
                
                List<DowntimeReason> lineReasons = (from o in allReasons
                                                    where !(from c in companyDemoReasons
                                                            select c.ID)
                                                            .Contains(o.ID)
                                                    select o).ToList();

                foreach (DowntimeReason lineReason in lineReasons)
                {
                    DowntimeReason existingReason = (from o in reasons
                                                     where o.Level1 == lineReason.Level1
                                                     && ((string.IsNullOrEmpty(lineReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == lineReason.Level2)
                                                     && ((string.IsNullOrEmpty(lineReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == lineReason.Level3)
                                                     && o.Line == lineReason.Line
                                                     select o).FirstOrDefault();

                    if (existingReason == null)
                        reasons.Add(lineReason);
                }

                foreach (DowntimeReason companyDemoReason in companyDemoReasons)
                {
                    DowntimeReason lineReason = (from o in allReasons
                                                 where o.Level1 == companyDemoReason.Level1
                                                 && ((string.IsNullOrEmpty(companyDemoReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == companyDemoReason.Level2)
                                                 && ((string.IsNullOrEmpty(companyDemoReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == companyDemoReason.Level3)
                                                 && o.Line != companyDemoReason.Line
                                                 select o).FirstOrDefault();

                    if (lineReason == null)//One doesn't exist
                    {
                        if (!reasons.Contains(null))
                            reasons.Add(companyDemoReason);
                    }
                    else
                    {
                        if (!reasons.Contains(lineReason))
                            reasons.Add(companyDemoReason);
                    }
                }

                var q1 = from o in reasons
                         group o by o.Level2 into g
                         select new
                         {
                             g.Key,
                             Id = g.Min(o => o.ID),
                             IsChangeOver = g.Select(o => o.IsChangeOver).FirstOrDefault(),
                             IsPlanned = g.Select(o => o.HideReasonInReports).FirstOrDefault(),
                             Duration = g.Select(o => o.Duration).FirstOrDefault()
                         };

                foreach (var item in q1.ToList())
                {
                    result.Add(new { ID = item.Id, Level2 = item.Key, IsChangeOver = item.IsChangeOver, IsPlanned = reasons.Where(o => o.Level2 == item.Key && o.Level3 == null).Select(o => o.HideReasonInReports).FirstOrDefault(), Duration = item.Duration });
                }
            }
            return result;
        }

        private List<object> GetLevel3(string level1, string level2)
        {
            List<object> result = new List<object>();
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeReasonSet
                        where o.Client == _currentClient
                        && (o.Line == null || o.Line == "" || o.Line == Line || o.Line == "company-demo")
                        && o.Level1 == level1
                        && o.Level2 == level2
                        && !string.IsNullOrEmpty(o.Level2)
                        && o.Level3 != null && o.Level3 != ""
                        orderby o.Level1, o.Level2, o.Level3
                        select o;

                List<DowntimeReason> reasons = new List<DowntimeReason>();

                List<DowntimeReason> allReasons = q.ToList<DowntimeReason>();

                List<DowntimeReason> companyDemoReasons = (from o in allReasons
                                                           where o.Line == "company-demo" || o.Line == null || o.Line == ""
                                                           select o).ToList();
                
                List<DowntimeReason> lineReasons = (from o in allReasons
                                                    where !(from c in companyDemoReasons
                                                            select c.ID)
                                                            .Contains(o.ID)
                                                    select o).ToList();

                foreach (DowntimeReason lineReason in lineReasons)
                {
                    DowntimeReason existingReason = (from o in reasons
                                                     where o.Level1 == lineReason.Level1
                                                     && ((string.IsNullOrEmpty(lineReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == lineReason.Level2)
                                                     && ((string.IsNullOrEmpty(lineReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == lineReason.Level3)
                                                     && o.Line == lineReason.Line
                                                     select o).FirstOrDefault();

                    if (existingReason == null)
                        reasons.Add(lineReason);
                }

                foreach (DowntimeReason companyDemoReason in companyDemoReasons)
                {
                    DowntimeReason lineReason = (from o in allReasons
                                                 where o.Level1 == companyDemoReason.Level1
                                                 && ((string.IsNullOrEmpty(companyDemoReason.Level2) && string.IsNullOrEmpty(o.Level2)) || o.Level2 == companyDemoReason.Level2)
                                                 && ((string.IsNullOrEmpty(companyDemoReason.Level3) && string.IsNullOrEmpty(o.Level3)) || o.Level3 == companyDemoReason.Level3)
                                                 && o.Line != companyDemoReason.Line
                                                 select o).FirstOrDefault();

                    if (lineReason == null)//One doesn't exist
                    {
                        if (!reasons.Contains(null))
                            reasons.Add(companyDemoReason);
                    }
                    else
                    {
                        if (!reasons.Contains(lineReason))
                            reasons.Add(companyDemoReason);
                    }
                }

                var q1 = from o in reasons
                          select new {o.ID , o.Level3, o.IsChangeOver, o.HideReasonInReports, o.Duration};

                var list = q1.ToList();

                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(item.Level3))
                    {
                        result.Add(new { ID = item.ID, Level3 = item.Level3, IsChangeOver = item.IsChangeOver, IsPlanned = item.HideReasonInReports, Duration = item.Duration });
                    }
                }
                
            }
            return result;
        }

        private bool Stop(DateTime start,DateTime stop)
        {
            DowntimeData newdd = new DowntimeData() { EventStart = start, EventStop = stop, Minutes = Convert.ToDecimal(stop.Subtract(start).TotalSeconds) / 60 };
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                db.AddToDowntimeDataSet(newdd);
                db.SaveChanges();
            }
            return true;
           
        }

        private string delRecords()
        {
            string ids = request["ids"];
            if (string.IsNullOrEmpty(ids)) return "Invald args.";

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                foreach (var item in ids.Split(','))
                {
                    int id;
                    if (!int.TryParse(item, out id)) continue;

                    var q = from o in db.DowntimeDataSet
                        where o.ID == id
                        select o;
                    var row = q.FirstOrDefault();
                    if (row != null)
                    {
                        db.DeleteObject(row);
                    }
                }
                db.SaveChanges();
            }
            return "true";
        }

        private string mergeRecords()
        {
            string ids = request["ids"];
            if (string.IsNullOrEmpty(ids)) return "Invald args.";
            
            var sum = getSum(ids);
            var idArray = ids.Split(',');

            if (idArray.Length <= 0) return sum;

            var firstSum = getSum(idArray[0]);
            updateSum(idArray[0], Convert.ToString(Convert.ToDouble(sum) - Convert.ToDouble(firstSum), CultureInfo.InvariantCulture), sum);
            var deleteIds = ids.Substring(ids.IndexOf(',') + 1);
            deleteSumRecords(deleteIds);

            return sum;
        }


        private string getSum(string ids)
        {
            
            string result;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["default"].ToString()))
            {
                SqlCommand com = new SqlCommand(string.Format("SELECT SUM(Minutes) AS Sum FROM DowntimeData WHERE (ID IN ({0}))",ids), con);
                
                con.Open();
                result = com.ExecuteScalar().ToString();

            }
            return result;
        }

        private string updateSum(string id, string duration, string originalMinutes)
        {

            string result = "";

            float minF = float.Parse(duration);
            int sec =(int) minF * 60;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["default"].ToString()))
            {
                SqlCommand com = new SqlCommand(string.Format("update DowntimeData set eventStop=DATEADD(ss, {2}, EventStop), Minutes = {0} WHERE Id ={1}", originalMinutes, id, sec), con);



                con.Open();
                int count = com.ExecuteNonQuery();
                if (count > 0)
                {
                    result = "true";
                }

            }
            return result;
        }

        private string deleteSumRecords(string ids)
        {

            string result = "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["default"].ToString()))
            {
                SqlCommand com = new SqlCommand(string.Format("delete DowntimeData  WHERE Id in ({0})",  ids), con);

                con.Open();
                int count = com.ExecuteNonQuery();
                if (count > 0)
                {
                    result = "true";
                }

            }
            return result;
        }
        private string Save(int id,HttpContext context, bool changeMinutes = false)
        {
            int reasonId;
            string reasonCode, comment, line, client;
            DateTime startdatetime, enddatetime;
            decimal minutes;

            int throughputId, option1Id, option2Id = -1;

            reasonCode = request.Form["reasonCode"];
            comment = request.Form["comment"];
            line = request.Form["line"];

            client = _currentClient;

            line = line.Replace('#', ' ');


            int.TryParse(request.Form["throughput"], out throughputId);
            int.TryParse(request.Form["option1"], out option1Id);
            int.TryParse(request.Form["option2"], out option2Id);

            if (!DateTime.TryParse(request.Form["startdatetime"], out startdatetime))
            {
                context.Response.Write("start date time is empty.");
                return "false";
            }


            if (!int.TryParse(request.Form["reasonId"], out reasonId))
            {
                context.Response.Write("reason id is empty.");
                return "false";
            }

            if (request.Form["minutes"] != "tv")
            {
                if (!decimal.TryParse(request.Form["minutes"], out minutes))
                {
                    context.Response.Write("minutes id is empty.");
                    return "false";
                }
            }
            else
            {
                minutes = -1;
            }

            if (string.IsNullOrEmpty(line))
            {
                context.Response.Write("line is empty.");
                return "false";
            }

            if (string.IsNullOrEmpty(reasonCode))
            {
                context.Response.Write("reasonCode is empty.");
                return "false";
            }

            startdatetime.AddMinutes(Convert.ToDouble(minutes));

            bool result;
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeDataSet
                        where o.ID == id
                        select o;
                var dd = q.FirstOrDefault();
                if (dd == null) return false.ToString().ToLower();
                DowntimeReason reason = DCSDashboardDemoHelper.getReason(reasonId);

                if (dd.EventStop.HasValue == false)
                {

                    if (!DateTime.TryParse(request.Form["enddatetime"], out enddatetime))
                    {
                        context.Response.Write("end date time is empty.");
                        return "false";
                    }

                    dd.EventStop = enddatetime;
                    if (dd.EventStart != null)
                        dd.Minutes = Convert.ToDecimal(dd.EventStop.Value.Subtract(dd.EventStart.Value).TotalMinutes);

                    changeMinutes = false;

                    SwitchLineStatus(line, true);
                }

                dd.ReasonCodeID = reasonId;
                dd.ReasonCode = reasonCode;
                dd.Line = line;
                dd.Comment = comment;

                if(changeMinutes)
                    dd.Minutes = minutes;

                dd.Client = client;

                if (reason.HideReasonInReports && dd.Minutes > reason.Duration && reason.Duration > 0)//If Planned && duration is greater than 0
                {
                    DowntimeChild child1 = (from o in db.DowntimeChildren
                        where o.ParentDowntimeId == dd.ID
                        select o).FirstOrDefault();

                    if (child1 == null)
                    {
                        if (dd.EventStart != null)
                        {
                            dd.EventStop = dd.EventStart.Value.AddMinutes(reason.Duration);
                            // Tim mark
                            

                            DowntimeData dd2 = new DowntimeData
                            {
                                Client = dd.Client,
                                ReasonCode = null,
                                ReasonCodeID = null,
                                Line = dd.Line,
                                Minutes = dd.Minutes - reason.Duration,
                                Comment = string.Empty,
                                EventStart = dd.EventStop
                            };
                            dd2.EventStop = dd.EventStop.Value.AddMinutes(Convert.ToDouble(dd2.Minutes));

                            child1 = new DowntimeChild {ParentDowntimeId = dd.ID};

                            dd.Minutes = reason.Duration;//Change after setting dd2's minutes

                            db.AddToDowntimeDataSet(dd2);
                            db.SaveChanges();

                            child1.DowntimeId = dd2.ID;
                            child1.ReasonCodeId = reason.ID;

                            db.AddToDowntimeChildren(child1);
                        }

                        db.SaveChanges();
                    }

                }
                else
                {
                    db.SaveChanges();
                }

                result = true;

                if (reason.IsChangeOver && throughputId > 0)
                {
                    DCSDashboardDemoHelper.CreateThroughPutHistory(throughputId, option1Id, option2Id, dd.EventStop.Value, line);
                }
                else if (option1Id > 0 || option2Id > 0)
                {
                    if (option1Id > 0)
                        DCSDashboardDemoHelper.CreateNOS(dd.ID, option1Id);

                    if (option2Id > 0)
                        DCSDashboardDemoHelper.CreateNOS(dd.ID, option2Id);
                }
            }

            return result.ToString().ToLower();
        }

        private string SplitDowntime(int downtimeId, decimal minutes)
        {
            using (var db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = from o in db.DowntimeDataSet
                    where o.ID == downtimeId
                    select o;

                DowntimeData dd = q.FirstOrDefault();
                if (dd == null) return "false";

                var seconds = Convert.ToInt32(minutes*60);
                if (dd.EventStop != null)
                {
                    dd.EventStop = dd.EventStop.Value.Subtract(new TimeSpan(0, 0, 0, seconds));
                    dd.Minutes -= minutes;

                    DowntimeData dd2 = new DowntimeData
                    {
                        Client = dd.Client,
                        ReasonCode = null,
                        ReasonCodeID = null,
                        Line = dd.Line,
                        Minutes = minutes,
                        Comment = string.Empty,
                        EventStart = dd.EventStop,
                        EventStop = dd.EventStop.Value.AddSeconds(Convert.ToInt32(minutes*60))
                    };

                    db.AddToDowntimeDataSet(dd2);
                }
                db.SaveChanges();
            }

            return "true";
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }


    public class Leave3Row
    {
        public int ID;
        public string Level3;
    }

    public class PostRowData
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public Boolean HideReasonInReports { get; set; }
        public Boolean IsChangeOver { get; set; }
        public string Line { get; set; }
        public int Duration { get; set; }
    }
}
