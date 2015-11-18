﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System.Text;
using DCSDemoData;
using DowntimeCollection_Demo.Classes;
using System.Web.Security;

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
        string Line = null;

        public void ProcessRequest(HttpContext context)
        {
            request=context.Request;
            context.Response.ContentType = "application/json";

            _currentClient = context.User.Identity.Name;//当前登录

            string op = request["op"];
            string level1, level2;
            int id;
            Line = request["line"];

            /*
            if (string.IsNullOrEmpty(_currentClient))
            {
                string userName = request["user"];
                string password = request["password"];

                if (Membership.ValidateUser(userName, password))
                    _currentClient = userName;
            }
             */

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
                    string comment = request.Form["comment"];
                    int reasonId = 0;

                    string[] ids = request.Form["id"].Split(',');

                    string response = "false";

                    bool changeMinutes = false;

                    if (ids.Length == 1)
                        changeMinutes = true;

                    foreach (string strId in ids)
                    {
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
                default:
                    break;
            }
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
                    cEmail = new ClientEmail();

                    cEmail.Client = _currentClient;
                    cEmail.SendDailyDigest = true;

                    db.AddToClientEmails(cEmail);

                    return (db.SaveChanges() > 0).ToString();
                }
                else
                {
                    cEmail.SendDailyDigest = true;

                    return (db.SaveChanges() > 0).ToString();
                }
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


                if (dt != null)
                {
                    if (!dt.EventStop.HasValue)
                    {
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

                if (dt != null)
                {
                    SwitchLineStatus(Line, false);

                    return ConvertToJsonString(new
                    {
                        ID = dt.ID,
                        EventStart = (dt.EventStart.HasValue ? dt.EventStart.Value.ToString("MM/dd/yyyy hh:mm:ss tt") : ""),
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

        public string UpdateDTWithNoStop()
        {
            DateTime es;

            int id = -1;

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

                if (dt != null)
                {
                    dt.EventStop = es;
                    db.SaveChanges();

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

        public string GetPerformanceChart()
        {
            string results = string.Empty;



            return results;
        }

        public string GetTotalScheduledMinutes()
        {
            DateTime sd;
            DateTime ed;
            double totalMinutes = 0;
            string reason = "Scheduled DT";

            if (!DateTime.TryParse(request.Params["sd"], out sd))
            {
                return null;
            }

            if (!DateTime.TryParse(request.Params["ed"], out ed))
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

                if (q != null)
                {
                    List<object> results = new List<object>();

                    List<DowntimeData> downtime = q.ToList();
                    double difference = 0;

                    foreach (DowntimeData dt in downtime)
                    {
                        double minutes = Convert.ToDouble(dt.Minutes.Value);

                        if (dt.EventStart < sd)
                        {
                            difference += sd.Subtract(dt.EventStart.Value).TotalMinutes;
                        }
                        
                        if (dt.EventStop > ed)
                        {
                            difference += dt.EventStop.Value.Subtract(ed).TotalMinutes;
                        }

                        double total = minutes - difference;

                        totalMinutes += minutes;

                    }

                    totalMinutes = totalMinutes - difference;
                }

            }

            return totalMinutes.ToString();

        }

        public string GetLines()
        {
            using(DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                var q = (from o in db.LineStatus
                         where o.Client == _currentClient
                         select o.Line).Distinct();

                if (q != null)
                {
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

            return ConvertToJsonString(string.Empty);
        }

        public string GetDowntime()
        {
            DateTime sd;
            DateTime ed;
            string group = request.Params["group"];
            string reasoncode = request.Params["reason"];
            
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

                if (q != null)
                {
                    List<object> results = new List<object>();

                    Dictionary<DateTime, object> dict = new Dictionary<DateTime, object>();

                    List<DowntimeData> downtime = q.ToList();

                    foreach (DowntimeData d in downtime)
                    {

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

                    results = dict.OrderBy(o => o.Key).Select(o => o.Value).ToList();

                    return ConvertToJsonString(results);
                }
            }

            return null;
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
                        orderby o.EventStop.Value ascending
                        select o;

                if (q != null)
                {
                    List<CaseCountModel> results = new List<CaseCountModel>();

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
                                                       where (o.EventStop.Value.Year == d.Year && o.EventStop.Value.Month == d.Month && o.EventStop.Value.Day == d.Day)
                                                       && o.EventStop.Value.Hour >= d.Hour && o.EventStop.Value.Hour <= d.Hour
                                                       select o).ToList();

                            if (tmpList.Count > 0)
                            {
                                CaseCount first = tmpList.First();
                                CaseCount last = tmpList.Last();


                                int tp1Id = DiamondCrystaldashboardHelper.GetThroughputIdFromReference(histories.Where(o => o.Date <= first.EventStop.Value).OrderByDescending(o => o.Date).Select(o => o.ThroughputReference).FirstOrDefault());

                                int tp2Id = DiamondCrystaldashboardHelper.GetThroughputIdFromReference(histories.Where(o => o.Date <= last.EventStop.Value).OrderByDescending(o => o.Date).Select(o => o.ThroughputReference).FirstOrDefault());

                                decimal perHour1 = 0M;
                                decimal perHour2 = 0M;

                                Throughput tp1 = (from o in throughputs
                                                  where o.Id == tp1Id
                                                  select o).FirstOrDefault();

                                Throughput tp2 = (from o in throughputs
                                                  where o.Id == tp2Id
                                                  select o).FirstOrDefault();

                                if (tp1 != null)
                                    perHour1 = tp1.PerHour;

                                if (tp2 != null)
                                    perHour2 = tp2.PerHour;

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

                                //results.Add(m1);

                                if(!dictionary.ContainsKey(first.EventStop.Value))
                                    dictionary.Add(first.EventStop.Value, m1);

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

                                //results.Add(m2);

                                if(!dictionary.ContainsKey(last.EventStop.Value))
                                    dictionary.Add(last.EventStop.Value, m2);
                            }

                            d = d.AddHours(1);
                        }
                    }

                    //results = results.OrderBy(o => o.EventStop).ToList();
                    results = dictionary.OrderBy(o => o.Key).Select(o => o.Value).ToList();

                    return ConvertToJsonString(results);
                }
            }

            return null;

        }

        public void TurnLights()
        {
            bool on = false;
            
            bool.TryParse(request.Form["on"], out on);

            if (string.IsNullOrEmpty(request.Form["on"]))
            {
                bool status = DiamondCrystaldashboardHelper.getLightsOn(Line);

                //Turn the opposite
                if (status)
                    on = false;
                else
                    on = true;

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
                if (p != null)
                    return p;

                return null;
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
                if (p != null)
                    return p;

                return null;
            }
        }

        public string GetThroughPuts()
        {
            List<Throughput> tpList = DCSDashboardDemoHelper.GetThroughPuts(Line);
            List<object> results = new List<object>();

            tpList = tpList.Where(o => o.Active == true).OrderBy(o => o.Name).ToList();

            foreach (Throughput tp in tpList)
            {
                results.Add(new
                {
                    Id = tp.Id,
                    Name = tp.Name,
                    Description = tp.Description,
                    PerHour = tp.PerHour
                });
            }

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
                    Id = op.Id,
                    Name = op.Name,
                    Description = op.Description,
                    Number = op.Number
                });
            }

            return ConvertToJsonString(results);
        }

        public string GetOption()
        {
            int number = -1;

            int.TryParse(request.Form["number"], out number);

            List<Options> opList = new List<Options>();

            if(number > -1)
                opList = DCSDashboardDemoHelper.GetOptions(number);

            List<object> results = new List<object>();

            foreach (Options op in opList)
            {
                results.Add(new
                {
                    Id = op.Id,
                    Name = op.Name,
                    Description = op.Description,
                    Number = op.Number
                });
            }

            return ConvertToJsonString(results);
        }

        public string CreateThroughPut()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int perHour = -1;

            int.TryParse(request.Form["perhour"], out perHour);

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(description) && perHour > -1)
            {
                int results = DCSDashboardDemoHelper.CreateThroughPuts(name, description, perHour);

                if (results >= 0)
                    return "SUCCESS";
            }

            return "FAILED";
        }

        public string CreateOption()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int number = -1;

            int.TryParse(request.Form["number"], out number);

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(description) && number > -1)
            {
                using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
                {
                    int results = DCSDashboardDemoHelper.CreateOption(name, description, number);

                    if (results >= 0)
                        return "SUCCESS";
                }
            }

            return "FAILED";
        }

        public string UpdateThroughPut()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int perHour = -1;
            int id = -1;

            int.TryParse(request.Form["perhour"], out perHour);
            int.TryParse(request.Form["id"], out id);

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(description) && perHour > -1 && id > -1)
            {
                using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
                {
                    Throughput tp = (from o in db.Throughput
                                     where o.Id == id
                                     select o).FirstOrDefault();

                    if (tp != null)
                    {
                        tp.Name = name;
                        tp.Description = description;
                        tp.PerHour = perHour;
                        
                        int results = db.SaveChanges();

                        if (results >= 0)
                            return "SUCCESS";
                    }
                }
            }

            return "FAILED";

        }

        public string UpdateOption()
        {
            string name = request.Form["name"];
            string description = request.Form["description"];
            int number = -1;
            int id = -1;

            int.TryParse(request.Form["id"], out id);
            int.TryParse(request.Form["number"], out number);

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(description) && id > -1 && number > -1)
            {
                using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
                {
                    Options tp = (from o in db.Options
                                     where o.Id == id
                                     select o).FirstOrDefault();

                    if (tp != null)
                    {
                        tp.Name = name;
                        tp.Description = description;
                        tp.Number = number;

                        int results = db.SaveChanges();

                        if (results >= 0)
                            return "SUCCESS";
                    }
                }
            }

            return "FAILED";

        }

        public string DeleteThroughPut()
        {
            int id = -1;

            int.TryParse(request.Form["id"], out id);

            if (id > -1)
            {
                using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
                {
                    Throughput tp = (from o in db.Throughput
                                     where o.Id == id
                                     select o).FirstOrDefault();

                    if (tp != null)
                    {
                        tp.Active = false;

                        int results = db.SaveChanges();

                        if (results >= 0)
                            return "SUCCESS";
                    }
                }
            }

            return "FAILED";

        }

        public string DeleteOption()
        {
            int id = -1;

            int.TryParse(request.Form["id"], out id);

            if (id > -1)
            {
                using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
                {
                    Options tp = (from o in db.Options
                                     where o.Id == id
                                     select o).FirstOrDefault();

                    if (tp != null)
                    {
                        db.DeleteObject(tp);
                        int results = db.SaveChanges();

                        if (results >= 0)
                            return "SUCCESS";
                    }
                }
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
                DowntimeReason dr = new DowntimeReason();
                foreach (var item in dd)
                {

                    var levels = item.Path.Split('>');
                    if (levels.Length < 3) Array.Resize(ref levels, 3);
                    if (string.IsNullOrEmpty(levels[0]) && string.IsNullOrEmpty(levels[1]) && string.IsNullOrEmpty(levels[2])) continue;

                    if (item.Id <= 0)
                    {
                        dr = new DowntimeReason();
                        dr.Level1 = levels[0];
                        dr.Level2 = levels[1];
                        dr.Level3 = levels[2];
                        dr.Client = _currentClient;
                        dr.HideReasonInReports = item.HideReasonInReports;
                        dr.Line = (!string.IsNullOrEmpty(Line) ? Line : "company-demo");
                        dr.IsChangeOver = item.IsChangeOver;
                        db.AddToDowntimeReasonSet(dr);
                    }
                    else
                    {
                        var q = from o in db.DowntimeReasonSet
                                where o.ID == item.Id
                                && o.Client == _currentClient
                                select o;
                        dr = q.FirstOrDefault<DowntimeReason>();
                        if (dr != null)
                        {
                            if ((string.IsNullOrEmpty(dr.Line) || dr.Line == "company-demo") && (!string.IsNullOrEmpty(Line) && Line != "company-demo"))//assumes it is a global reason code and they're adding a Line specific
                            {
                                DowntimeReason reason = new DowntimeReason();

                                reason.Level1 = levels[0];
                                reason.Level2 = levels[1];
                                reason.Level3 = levels[2];
                                reason.HideReasonInReports = item.HideReasonInReports;
                                reason.Line = (!string.IsNullOrEmpty(Line) ? Line : "company-demo");
                                reason.IsChangeOver = item.IsChangeOver;
                                reason.Client = _currentClient;

                                db.AddToDowntimeReasonSet(reason);
                            }
                            else
                            {
                                dr.Level1 = levels[0];
                                dr.Level2 = levels[1];
                                dr.Level3 = levels[2];
                                dr.HideReasonInReports = item.HideReasonInReports;
                                dr.Line = (!string.IsNullOrEmpty(Line) ? Line : "company-demo");
                                dr.IsChangeOver = item.IsChangeOver;
                            }
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
                        dr = q.FirstOrDefault<DowntimeReason>();
                        if (dr != null)
                        {
                            if(dr.Line == Line)//Don't want to accidently delete another Line's reason code
                                db.DeleteObject(dr);
                        }
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
                    return q.ToList<DowntimeReason>();
                }
                else
                {
                    List<DowntimeReason> reasons = new List<DowntimeReason>();

                    var q = from o in db.DowntimeReasonSet
                            where o.Client == _currentClient
                            && (o.Line == Line || o.Line == "company-demo")
                            orderby o.Level1, o.Level2, o.Level3
                            select o;

                    List<DowntimeReason> allReasons = q.ToList<DowntimeReason>();

                    List<DowntimeReason> companyDemoReasons = (from o in allReasons
                                                               where o.Line == "company-demo" || o.Line == null || o.Line == ""
                                                               select o).ToList();

                    List<DowntimeReason> lineReasons = (from o in allReasons
                                                        where o.Line == Line
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
                            if(!reasons.Contains(lineReason))
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
            int id = 0;
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
                else
                {
                    var p = (from o in db.ThroughputHistory
                             where o.Date >= dd.EventStop
                             && o.Client == _currentClient
                             orderby o.Date descending
                             select new { Tp = (o.Throughput != null ? o.Throughput.Id : 0), Op1 = (o.Options != null ? o.Options.Id : 0), Op2 = (o.Options1 != null ? o.Options1.Id : 0) }).FirstOrDefault();

                    int tpId = 0;
                    int opId = 0;
                    int op2Id = 0;

                    if (p != null)
                    {
                        tpId = p.Tp;
                        opId = p.Op1;
                        op2Id = p.Op2;
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

            string reasonCode, comment, line,client;
            DateTime startdatetime, enddatetime;
            decimal minutes;
            reasonCode=request.Form["reasonCode"];
            comment=request.Form["comment"];
            line=request.Form["line"];

            line = line.Replace('#', ' ');

            client = _currentClient;

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

            enddatetime=startdatetime.AddMinutes(Convert.ToDouble(minutes));


            bool result = false;
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                int count = 0;

                /*
                if (id > 0)
                {
                    DowntimeData dt = (from o in db.DowntimeDataSet
                                       where o.ID == id
                                       select o).FirstOrDefault();

                    if (dt != null)
                    {
                        startdatetime = dt.EventStart.Value;//Update STartTime
                        dt.EventStop = enddatetime;
                        dt.Minutes = minutes;
                        dt.Comment = comment;
                        dt.ReasonCode = reasonCode;
                        dt.ReasonCodeID = reasonId;

                        SwitchLineStatus(line, true);

                        count = 1;//We already added a dt
                    }
                }
                 */

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
                    db.AddToDowntimeDataSet(dd);
                }

                if (throughputId > 0)
                {
                    DCSDashboardDemoHelper.CreateThroughPutHistory(throughputId, option1Id, option2Id, enddatetime, line);
                }


                int updatedRecords = db.SaveChanges();

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
                sd = DateTime.Now.AddHours(-12);//补时差2小时
                ed = DateTime.Now.AddHours(2);

                /*
                if (_currentClient.ToLower() == "diamondcrystal")
                {
                    sd = DateTime.Now.AddHours(-6).AddHours(2);//补时差2小时
                    ed = DateTime.Now.AddHours(2);

                }
                else
                {
                    sd = DateTime.Now.AddHours(-12);//补时差2小时
                    ed = DateTime.Now.AddHours(2);
                }
                 */
            }

            /*
            if (sd.HasValue)
            {
                sd = sd.Value;//.AddHours(2);
            }
            */

            if (ed.HasValue)
            {
                stopd = ed.Value;// new DateTime(ed.Value.Year, ed.Value.Month, ed.Value.Day, 23, 59, 59);//.AddHours(2);
            }

            List<object> result = new List<object>();
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                List<DowntimeReason> reasons = (from o in db.DowntimeReasonSet
                                                where o.Client == _currentClient
                                                && o.Line == Line
                                                select o).ToList();

                if (string.IsNullOrEmpty(line))
                {
                    var q = from o in db.DowntimeDataSet
                            where o.Client == _currentClient
                            && (!sd.HasValue || o.EventStart >= sd.Value)
                            && ((ed.HasValue ? (o.EventStart.Value <= stopd) : (o.EventStop.Value >= stopd)))//(o.EventStop.Value >= stopd)
                            && ((o.Line == null || o.Line == "" || o.Line == "company-demo"))
                            orderby o.EventStart ascending
                            select o;


                    foreach (var item in q.ToList<DowntimeData>())
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
                    var q = from o in db.DowntimeDataSet
                            where o.Client == _currentClient
                            && (!sd.HasValue || o.EventStart >= sd.Value)
                            && ((ed.HasValue ? (o.EventStart.Value <= stopd) : (o.EventStop.Value >= stopd)))//(o.EventStop.Value >= stopd)
                            && (o.Line == Line)
                            orderby o.EventStart ascending
                            select o;


                    foreach (var item in q.ToList<DowntimeData>())
                    {
                        bool? isChangeOver = (from o in reasons
                                             where o.ID == item.ReasonCodeID
                                             select o.IsChangeOver).FirstOrDefault();

                        if (isChangeOver == true)
                        {
                            Throughput tp = GetLatestThroughput(item.EventStop.Value);
                            item.ReasonCode = item.ReasonCode + " - " + (tp != null ? tp.Name : "");
                        }
                      
                        result.Add(
                            new
                            {
                                ID = item.ID,
                                EventStart = item.EventStart.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                                EventStop = (item.EventStop.HasValue ? item.EventStop.Value.ToString(@"MM\/dd\/yyyy HH:mm:ss") : ""),
                                ReasonCode = (string.IsNullOrEmpty(item.ReasonCode) ? null : item.ReasonCode),
                                ReasonCodeID = (item.ReasonCodeID.HasValue ? item.ReasonCodeID.Value : 0),
                                Line = item.Line,
                                Minutes = (item.Minutes.HasValue ? item.Minutes : 0),
                                Client = item.Client,
                                Comment = (string.IsNullOrEmpty(item.Comment) ? null : item.Comment),
                                EventStartDisplay = item.EventStart.Value.ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                                EventStopDisplay = (item.EventStop.HasValue ? item.EventStop.Value.ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")) : ""),
                                IsChangeOver = (isChangeOver.HasValue ? isChangeOver : false)
                            });
                    }
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
                         group o by o.Level1 into g
                         select new
                         {
                             g.Key,
                             Id=g.Min(o=>o.ID),
                             IsChangeOver = g.Select(o => o.IsChangeOver).FirstOrDefault()
                         };

                //var q = from o in db.DowntimeReasonSet
                //        where !string.IsNullOrEmpty(o.Level1)
                //            &&string.IsNullOrEmpty(o.Level2)
                //            &&string.IsNullOrEmpty(o.Level3)
                //        select new { o.ID, o.Level1 };

                foreach (var item in q.ToList())
                {
                    result.Add(new { ID = item.Id, Level1 = item.Key, IsChangeOver = item.IsChangeOver });
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
                        where o.Level1 == level1
                        && o.Client == _currentClient
                        && (o.Line == null || o.Line == "" || o.Line == Line || o.Line == "company-demo")
                        group o by o.Level2 into g
                        select new
                        {
                            g.Key,
                            Id = g.Min(o => o.ID),
                            IsChangeOver = g.Select(o => o.IsChangeOver).FirstOrDefault()
                        };

                //var q = from o in db.DowntimeReasonSet
                //        where o.Level1 == level1
                //           && !string.IsNullOrEmpty(o.Level2)
                //           && string.IsNullOrEmpty(o.Level3)
                //        select new { o.ID, o.Level2 };

                foreach (var item in q.ToList())
                {
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        result.Add(new { ID = item.Id, Level2 = item.Key, IsChangeOver = item.IsChangeOver });
                    }
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
                        where o.Level1 == level1 && o.Level2==level2
                        && o.Client == _currentClient
                        && !string.IsNullOrEmpty(o.Level3)
                        && (o.Line == null || o.Line == "" || o.Line == Line || o.Line == "company-demo")
                        select new {o.ID , o.Level3, o.IsChangeOver};

                var list = q.ToList();
                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(item.Level3))
                    {
                        result.Add(new { ID = item.ID, Level3 = item.Level3, IsChangeOver = item.IsChangeOver });
                    }
                }
                
            }
            return result;
        }

        private bool Stop(DateTime start,DateTime stop)
        {
             bool result = false;
             DowntimeData newdd = new DowntimeData() { EventStart = start, EventStop = stop, Minutes = Convert.ToDecimal(stop.Subtract(start).TotalSeconds) / 60 };
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                db.AddToDowntimeDataSet(newdd);
                db.SaveChanges();
                result = true;

            }
            return result;
           
        }

        private string delRecords()
        {
            string ids = request["ids"];
            if (string.IsNullOrEmpty(ids)) return "Invald args.";

            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                int id;
                foreach (var item in ids.Split(','))
                {
                    if (int.TryParse(item, out id))
                    {
                        var q = from o in db.DowntimeDataSet
                                where o.ID == id
                                select o;
                        var row = q.FirstOrDefault();
                        if (row != null)
                        {
                            db.DeleteObject(row);
                        }
                    }
                }
                db.SaveChanges();
            }
            return "true";
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

            enddatetime = startdatetime.AddMinutes(Convert.ToDouble(minutes));

            bool result = false;
            using (DB db = new DB(DBHelper.GetConnectionString(_currentClient)))
            {
                DowntimeData dd = null;
                var q = from o in db.DowntimeDataSet
                        where o.ID == id
                        select o;
                dd = q.FirstOrDefault();
                if (dd != null)
                {

                    //dd.EventStart = startdatetime;
                    //dd.EventStop = enddatetime;
                    //dd.Minutes = (minutes != -1 ? minutes : dd.Minutes);

                    if (dd.EventStop.HasValue == false)
                    {

                        if (!DateTime.TryParse(request.Form["enddatetime"], out enddatetime))
                        {
                            context.Response.Write("end date time is empty.");
                            return "false";
                        }

                        dd.EventStop = enddatetime;
                        dd.Minutes = Convert.ToDecimal(dd.EventStop.Value.Subtract(dd.EventStart.Value).TotalMinutes);

                        changeMinutes = false;

                        SwitchLineStatus(line, true);
                    }

                    dd.ReasonCodeID = reasonId;
                    dd.ReasonCode = reasonCode;
                    dd.Line = line;
                    dd.Comment = comment;

                    if(changeMinutes == true)
                        dd.Minutes = minutes;

                    dd.Client = client;
                    db.SaveChanges();
                    result = true;

                    DowntimeReason reason = DCSDashboardDemoHelper.getReason(reasonId);

                    if (reason != null)
                    {
                        if (reason.IsChangeOver && throughputId > 0)
                        {
                            DCSDashboardDemoHelper.CreateThroughPutHistory(throughputId, option1Id, option2Id, dd.EventStop.Value, line);
                        }
                    }
                }
                else
                {
                    result = false;
                }

            }

            return result.ToString().ToLower();
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
    }

}
