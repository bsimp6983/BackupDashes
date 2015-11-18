using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Web.Security;
using DowntimeCollection_Demo.Classes;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// DSCWS 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class DSCWS : System.Web.Services.WebService
    {
        [WebMethod]
        public bool Ping(string client, string line, bool s)
        {
            using (DB db = new DB())
            {
                AscommStatus status = (from o in db.AscommStatuses
                                       where o.Client == client
                                       && o.Line == line
                                       select o).FirstOrDefault();

                if (status != null)
                {
                    status.LastPing = DateTime.Now;
                    status.Status = s;
                    status.Line = line;
                }
                else
                {
                    status.Client = client;
                    status.LastPing = DateTime.Now;
                    status.Status = s;
                    status.Line = line;

                    db.AddToAscommStatuses(status);
                }

                return db.SaveChanges() > 0;                
            }
        }

        [WebMethod]
        public int UpdateGlobalDowntimeThreshold(string client, int seconds)
        {
            using (DB db = new DB())
            {
                List<LineSetup> setups = (from o in db.LineSetups
                                   where o.DataCollectionNode.Client == client
                                   select o).ToList();

                if (setups.Count > 0)
                {
                    foreach(LineSetup setup in setups)
                        setup.DowntimeThreshold = seconds;

                    db.SaveChanges();

                    return seconds;
                }

                return -1;
            }
        }

        [WebMethod]
        public int UpdateDowntimeThreshold(string client, string line, int seconds)
        {
            using (DB db = new DB())
            {
                LineSetup setup = (from o in db.LineSetups
                                       where o.DataCollectionNode.Client == client
                                       && o.Line == line
                                       select o).FirstOrDefault();

                if (setup != null)
                {
                    setup.DowntimeThreshold = seconds;

                    db.SaveChanges();

                    return seconds;
                }

                return -1;
            }
        }

        [WebMethod]
        public bool CreateDowntimeDataWithString(string start, string stop, decimal? minutes, string reasonCode, int? reasonCodeID, string comment, string line, string client)
        {
            bool result = false;
            DateTime eventStart;
            DateTime eventStop;

            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(stop))
                return false;

            if (!DateTime.TryParse(start, out eventStart))
                return false;

            if (!DateTime.TryParse(stop, out eventStop))
                return false;

            if (minutes.HasValue == false)
                minutes = Convert.ToInt32(eventStop.Subtract(eventStart).TotalMinutes);

            using (DB db = new DB())
            {
                DowntimeData latestDowntime = (from o in db.DowntimeDataSet
                                               where o.Client == client && o.Line == line
                                               orderby o.EventStop.Value descending
                                               select o).FirstOrDefault();


                DateTime? latestDate = null;

                if (latestDowntime != null)
                    latestDate = latestDowntime.EventStop;

                bool create = !latestDate.HasValue;

                DowntimeData dd = new DowntimeData();

                if (latestDate.HasValue)
                {
                    create = latestDate.Value < eventStop;
                }

                if (create)
                {
                    //dd.EventStart = eventStart;
                    //dd.EventStop = eventStop;
                    dd.EventStart = eventStart;
                    dd.EventStop = eventStop;

                    dd.Minutes = minutes;
                    dd.ReasonCodeID = reasonCodeID;
                    dd.ReasonCode = reasonCode;
                    dd.Line = line;
                    dd.Comment = comment;
                    dd.Client = client;
                    dd.IsCreatedByAcromag = true;
                    db.AddToDowntimeDataSet(dd);
                    result = db.SaveChanges() > 0;

                    if (result)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }
                }

            }

            return result;
        }

        [WebMethod]
        public bool CreateDowntimeData(DateTime? eventStart, DateTime? eventStop, decimal? minutes, string reasonCode, int? reasonCodeID, string comment, string line, string client)
        {
            bool result = false;

            if (eventStart.HasValue == false || eventStop.HasValue == false)
                return false;

            if (minutes.HasValue == false)
                minutes = Convert.ToInt32(eventStop.Value.Subtract(eventStart.Value).TotalMinutes);

            using (DB db = new DB())
            {
                DowntimeData latestDowntime = (from o in db.DowntimeDataSet
                                               where o.Client == client && o.Line == line
                                               orderby o.EventStop.Value descending
                                               select o).FirstOrDefault();


                DateTime? latestDate = null;

                if (latestDowntime != null)
                    latestDate = latestDowntime.EventStop;

                bool create = !latestDate.HasValue;

                DowntimeData dd = new DowntimeData();

                if (latestDate.HasValue)
                {
                    create = !(latestDate.Value >= eventStop);
                }

                if (!create) return false;

                //dd.EventStart = eventStart;
                //dd.EventStop = eventStop;
                dd.EventStart = eventStart;
                dd.EventStop = eventStop;

                dd.Minutes = minutes;
                dd.ReasonCodeID = reasonCodeID;
                dd.ReasonCode = reasonCode;
                dd.Line = line;
                dd.Comment = comment;
                dd.Client = client;
                dd.IsCreatedByAcromag = true;
                db.AddToDowntimeDataSet(dd);
                result = db.SaveChanges() > 0;

                if (result)
                {
                    DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                }
            }

            return result;
        }

        public bool alreadyExists(string client, string line, DateTime eventStart, DateTime eventStop)
        {
            using (DB db = new DB())
            {
                //Grab anything that fits in the event start & event stop time range. Whether it's within or greater. 
                DowntimeData dt = (from o in db.DowntimeDataSet
                                    where o.Client == client && o.Line == line
                                    && (((o.EventStart.Value >= eventStart && o.EventStop.Value <= eventStop) || (o.EventStart.Value <= eventStart && o.EventStop.Value >= eventStop)) || o.EventStop.Value == eventStop || o.EventStart.Value == eventStart)
                                    && o.IsCreatedByAcromag == true
                                    select o).FirstOrDefault();

                if (dt != null)
                    return true;
            }

            return false;
        }


        [WebMethod]
        public int CreateTComDowntimeEvent(string eStart, string eStop, decimal? minutes, string client, string line, string comment = null)
        {
            int result = 0;

            if (string.IsNullOrEmpty(eStart) || string.IsNullOrEmpty(eStop))
                return -1;

            DateTime eventStart;
            DateTime eventStop;

            if (!DateTime.TryParse(eStart, out eventStart))
                return -1;

            if (!DateTime.TryParse(eStop, out eventStop))
                return -1;

            if (eventStop < eventStart)
                return -1;

            if (minutes.HasValue == false)
                minutes = Convert.ToInt32(eventStop.Subtract(eventStart).TotalMinutes);

            decimal totalMinutes = (decimal)eventStop.Subtract(eventStart).TotalMinutes;

            if (totalMinutes > 60 && minutes < 60)//If eventstop is greater than an hour or more, but the minute difference isn't, then assume timezone conversion or something is wrong for the times
            {
                //assume Event Start is correct
                eventStop = eventStart.AddMinutes((double)minutes);
            }

            using (DB db = new DB())
            {
                if (alreadyExists(client, line, eventStart, eventStop) == true)
                    return -2;

                DowntimeData dd = new DowntimeData();

                dd.EventStart = eventStart;
                dd.EventStop = eventStop;

                dd.Minutes = minutes;
                dd.ReasonCodeID = null;
                dd.ReasonCode = null;
                dd.Line = line;
                dd.Comment = comment;
                dd.Client = client;
                dd.IsCreatedByAcromag = true;
                db.AddToDowntimeDataSet(dd);

                if (db.SaveChanges() <= 0) return result;

                result = dd.ID;
                DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
            }

            return result;
        }

        [WebMethod]
        public int CreateDowntimeEventWithTimeZone(DateTime? eStart, DateTime? eStop, string strTimeZone, decimal? minutes, string reasonCode, int? reasonCodeID, string comment, string line, string client)
        {
            int result = 0;

            if (eStart.HasValue == false || eStop.HasValue == false)
                return 0;

            if (minutes.HasValue == false)
                minutes = Convert.ToInt32(eStop.Value.Subtract(eStart.Value).TotalMinutes);

            DateTime eventStart = eStart.Value;
            DateTime eventStop = eStop.Value;

            if (!string.IsNullOrEmpty(strTimeZone))
            {
                TimeZoneInfo timeZone = TimeZoneInfo.FromSerializedString(strTimeZone);

                eventStart = TimeZoneInfo.ConvertTime(eStart.Value, timeZone);
                eventStop = TimeZoneInfo.ConvertTime(eStop.Value, timeZone);
            }

            decimal totalMinutes = (decimal)eventStop.Subtract(eventStart).TotalMinutes;

            if (totalMinutes > 60 && minutes < 60)//If eventstop is greater than an hour or more, but the minute difference isn't, then assume timezone conversion or something is wrong for the times
            {
                //assume Event Start is correct
                eventStop = eventStart.AddMinutes((double)minutes);
            }

            using (DB db = new DB())
            {
                if (alreadyExists(client, line, eventStart, eventStop))
                    return -2;

                DowntimeData dd = new DowntimeData();

                dd.EventStart = eventStart;
                dd.EventStop = eventStop;

                dd.Minutes = minutes;
                dd.ReasonCodeID = reasonCodeID;
                dd.ReasonCode = reasonCode;
                dd.Line = line;
                dd.Comment = comment;
                dd.Client = client;
                dd.IsCreatedByAcromag = true;
                db.AddToDowntimeDataSet(dd);

                if (db.SaveChanges() > 0)
                {
                    result = dd.ID;
                    DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                }

            }

            return result;
        }

        [WebMethod]
        public bool CreateDowntimeDataWithTimeZone(DateTime? eventStart, DateTime? eventStop, string strTimeZone, decimal? minutes, string reasonCode, int? reasonCodeID, string comment, string line, string client)
        {
            bool result = false;

            if (eventStart.HasValue == false || eventStop.HasValue == false)
                return false;

            if (minutes.HasValue == false)
                minutes = Convert.ToInt32(eventStop.Value.Subtract(eventStart.Value).TotalMinutes);

            if (!string.IsNullOrEmpty(strTimeZone))
            {
                TimeZoneInfo timeZone = TimeZoneInfo.FromSerializedString(strTimeZone);

                eventStart = TimeZoneInfo.ConvertTime(eventStart.Value, timeZone);
                eventStop = TimeZoneInfo.ConvertTime(eventStop.Value, timeZone);
            }

            using (DB db = new DB())
            {
                DowntimeData latestDowntime = (from o in db.DowntimeDataSet
                                               where o.Client == client && o.Line == line
                                               orderby o.EventStop.Value descending
                                               select o).FirstOrDefault();


                DateTime? latestDate = null;

                if (latestDowntime != null)
                    latestDate = latestDowntime.EventStop;

                bool create = !latestDate.HasValue;

                DowntimeData dd = new DowntimeData();

                if (latestDate.HasValue)
                {
                    create = !(latestDate.Value >= eventStop);
                }

                if (create)
                {
                    //dd.EventStart = eventStart;
                    //dd.EventStop = eventStop;
                    dd.EventStart = eventStart;
                    dd.EventStop = eventStop;

                    dd.Minutes = minutes;
                    dd.ReasonCodeID = reasonCodeID;
                    dd.ReasonCode = reasonCode;
                    dd.Line = line;
                    dd.Comment = comment;
                    dd.Client = client;
                    dd.IsCreatedByAcromag = true;
                    db.AddToDowntimeDataSet(dd);
                    result = db.SaveChanges() > 0;

                    if (result == true)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }
                }

            }

            return result;
        }

        [WebMethod]
        public bool LoginAndCreateDowntimeData(DateTime? eventStart, DateTime? eventStop, decimal? minutes, string reasonCode, int? reasonCodeID, string comment, string line, string client, string password)
        {
            bool result;

            bool loggedIn = Login(client, password);

            if (!loggedIn) return false;

            if (eventStart.HasValue == false || eventStop.HasValue == false)
                return false;

            if (minutes.HasValue == false)
                minutes = Convert.ToInt32(eventStop.Value.Subtract(eventStart.Value).TotalMinutes);

            using (DB db = new DB())
            {
                DowntimeData latestDowntime = (from o in db.DowntimeDataSet
                    where o.Client == client && o.Line == line
                    orderby o.EventStop.Value descending
                    select o).FirstOrDefault();


                DateTime? latestDate = null;

                if (latestDowntime != null)
                    latestDate = latestDowntime.EventStop;

                bool create = !latestDate.HasValue;

                DowntimeData dd = new DowntimeData();

                if (latestDate.HasValue)
                {
                    create = !(latestDate.Value >= eventStop);
                }

                if (create)
                {
                    //dd.EventStart = eventStart;
                    //dd.EventStop = eventStop;
                    dd.EventStart = eventStart;
                    dd.EventStop = eventStop;

                    dd.Minutes = minutes;
                    dd.ReasonCodeID = reasonCodeID;
                    dd.ReasonCode = reasonCode;
                    dd.Line = line;
                    dd.Comment = comment;
                    dd.Client = client;
                    dd.IsCreatedByAcromag = true;
                    db.AddToDowntimeDataSet(dd);
                    result = db.SaveChanges() > 0;

                    if (result)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }
                }
                else
                    result = false;

            }

            return result;
        }

        [WebMethod]
        public bool InsertDowntimeData(DateTime eventStart, DateTime eventStop, decimal? minutes, string reasonCode, int? reasonCodeID, string comment, string line, string client, string str_eventStart, string str_eventStop)
        {
            bool result;
            using (DB db = new DB())
            {
                DowntimeData latestDowntime = (from o in db.DowntimeDataSet
                                               where o.Client == client && o.Line == line
                                               orderby o.EventStop.Value descending
                                               select o).FirstOrDefault();


                DateTime? latestDate = (latestDowntime != null ? latestDowntime.EventStop : null);

                bool create = !latestDate.HasValue;

                DowntimeData dd = new DowntimeData();
                //为了避免时区问题，用string转过来转换
                DateTime es, et;
                if (!DateTime.TryParse(str_eventStart, out es) || !DateTime.TryParse(str_eventStop, out et))
                {
                    return false;
                }

                if (latestDate.HasValue)
                {
                    create = latestDate.Value < et;
                }

                if (create)
                {
                    //dd.EventStart = eventStart;
                    //dd.EventStop = eventStop;
                    dd.EventStart = es;
                    dd.EventStop = et;

                    dd.Minutes = minutes;
                    dd.ReasonCodeID = reasonCodeID;
                    dd.ReasonCode = reasonCode;
                    dd.Line = line;
                    dd.Comment = comment;
                    dd.Client = client;
                    dd.IsCreatedByAcromag = true;
                    db.AddToDowntimeDataSet(dd);
                    result = db.SaveChanges() > 0;

                    if (result)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }
                }
                else
                    result = false;

            }
            return result;
        }

        [WebMethod]
        public int CreateTComCaseCount(string eStart, string eStop, int caseCount, string line, string client)
        {
            try
            {
                int result;

                if (string.IsNullOrEmpty(eStart) || string.IsNullOrEmpty(eStop))
                    return -1;

                DateTime eventStart;
                DateTime eventStop;

                if (!DateTime.TryParse(eStart, out eventStart))
                    return -1;

                if (!DateTime.TryParse(eStop, out eventStop))
                    return -1;

                if (eventStop < eventStart)
                    return -1;

                using (DB db = new DB())
                {
                    int id;

                    CaseCount count = new CaseCount();
                    count.CaseCount1 = caseCount;
                    count.Client = client;
                    count.EventStart = eventStart;
                    count.EventStop = eventStop;
                    count.Line = line;

                    db.AddToCaseCountSet(count);

                    db.SaveChanges();

                    id = count.Id;

                    result = id;

                    if (result > 0)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                return -1;
            }
        }

        [WebMethod]
        public bool CreateCaseCount(DateTime? eventStart, DateTime? eventStop, int caseCount, string line, string client)
        {
            try
            {
                using (DB db = new DB())
                {
                    int id;

                    if (!eventStart.HasValue || !eventStop.HasValue)
                        return false;

                    CaseCount count = new CaseCount();
                    count.CaseCount1 = caseCount;
                    count.Client = client;
                    count.EventStart = eventStart;
                    count.EventStop = eventStop;
                    count.Line = line;

                    db.AddToCaseCountSet(count);

                    db.SaveChanges();

                    id = count.Id;

                    bool result = id > 0;

                    if (result)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw;
            }
        }

        [WebMethod]
        public bool LoginAndCreateCaseCount(DateTime? eventStart, DateTime? eventStop, int caseCount, string line, string client, string password)
        {
            bool loggedIn = Login(client, password);

            if (loggedIn)
            {
                try
                {
                    using (DB db = new DB(DBHelper.GetConnectionString(client)))
                    {
                        if (!eventStart.HasValue || !eventStop.HasValue)
                            return false;

                        CaseCount count = new CaseCount();
                        count.CaseCount1 = caseCount;
                        count.Client = client;
                        count.EventStart = eventStart;
                        count.EventStop = eventStop;
                        count.Line = line;

                        db.AddToCaseCountSet(count);

                        bool result = db.SaveChanges() > 0;

                        if (result)
                        {
                            DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                        }


                        return result;
                    }
                }
                catch (Exception ex)
                {
                    String fileName = this.Server.MapPath("~/App_Data/log.txt");
                    File.AppendAllText(fileName, ex.ToString());
                    throw;
                }
            }

            return false;
        }

        [WebMethod]
        public bool InsertCaseCount(string str_eventStart, string str_eventStop, int caseCount, string line, string client)
        {
            try
            {
                using (DB db = new DB())
                {
                    DateTime eventStart, eventStop;

                    if (!DateTime.TryParse(str_eventStart, out eventStart) || !DateTime.TryParse(str_eventStop, out eventStop))
                    {
                        return false;
                    }

                    CaseCount count = new CaseCount();
                    count.CaseCount1 = caseCount;
                    count.Client = client;
                    count.EventStart = eventStart;
                    count.EventStop = eventStop;
                    count.Line = line;
                    
                    db.AddToCaseCountSet(count);

                    bool result = db.SaveChanges() > 0;

                    if (result == true)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;
            }
        }
        [WebMethod]
        public int GetLatestCaseCount(string client, string line)
        {
            try
            {
                using (DB db = new DB())
                {
                    CaseCount caseCount = (from o in db.CaseCountSet
                                           where o.Client == client
                                           && o.Line == line
                                           orderby o.EventStop.Value descending
                                           select o).FirstOrDefault();

                    if (caseCount != null)
                        return caseCount.CaseCount1;

                    return 0;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw;
            }
        }

        [WebMethod]
        public CaseCount GetLatestCaseCountObject(string client, string line)
        {
            try
            {
                using (DB db = new DB())
                {
                    CaseCount caseCount = (from o in db.CaseCountSet
                                           where o.Client == client
                                           && o.Line == line
                                           orderby o.EventStop.Value descending
                                           select o).FirstOrDefault();

                    if (caseCount != null)
                        return caseCount;

                    return null;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;
            }
        }

        [WebMethod]
        public DowntimeData GetLatestDowntimeData(string client, string line)
        {
            try
            {
                using (DB db = new DB())
                {
                    DowntimeData dt = (from o in db.DowntimeDataSet
                                           where o.Client == client
                                           && o.Line == line
                                           orderby o.EventStop.Value descending
                                           select o).FirstOrDefault();

                    if (dt != null)
                        return dt;

                    return null;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;
            }
        }

        [WebMethod]
        public bool UpdateLineStatus(string client, string line, bool on)
        {
            try
            {
                using (DB db = new DB())
                {
                    LineStatus lineStat = (from o in db.LineStatus
                                           where o.Client == client
                                           && o.Line == line
                                           select o).FirstOrDefault();

                    bool result = false;

                    if (lineStat != null)
                    {
                        if (on != lineStat.Status || lineStat.Status == true)//Only log False once, but constantly update time for True
                        {
                            lineStat.Status = on;
                            lineStat.EventTime = DateTime.Now;

                            result = db.SaveChanges() > 0;
                        }
                    }
                    else
                    {
                        lineStat = new LineStatus();
                        lineStat.Client = client;
                        lineStat.Line = line;
                        lineStat.Status = on;
                        lineStat.ShiftStart = "06:00:00";
                        lineStat.Timezone = "Eastern Standard Time";
                        lineStat.EventTime = DateTime.Now;

                        db.AddToLineStatus(lineStat);

                        result = db.SaveChanges() > 0;
                    }

                    if (result == true)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;
            }
        }

        [WebMethod]
        public bool UpdateLineStatusWithTime(string client, string line, bool on, string strEventTime)
        {
            try
            {
                if (string.IsNullOrEmpty(strEventTime))
                    return false;

                DateTime eventTime;

                if (!DateTime.TryParse(strEventTime, out eventTime))
                    return false;


                using (DB db = new DB())
                {
                    LineStatus lineStat = (from o in db.LineStatus
                                           where o.Client == client
                                           && o.Line == line
                                           select o).FirstOrDefault();

                    bool result = false;

                    if (lineStat != null)
                    {
                        if (on != lineStat.Status || lineStat.Status == true)//Only log False once, but constantly update time for True
                        {
                            lineStat.Status = on;
                            lineStat.EventTime = eventTime;

                            result = db.SaveChanges() > 0;
                        }
                    }
                    else
                    {
                        lineStat = new LineStatus();
                        lineStat.Client = client;
                        lineStat.Line = line;
                        lineStat.Status = on;
                        lineStat.ShiftStart = "06:00:00";
                        lineStat.Timezone = "Eastern Standard Time";
                        lineStat.EventTime = eventTime;

                        db.AddToLineStatus(lineStat);

                        result = db.SaveChanges() > 0;
                    }

                    if (result == true)
                    {
                        DBHelper.UpdateAscommPing(client, DateTime.Now, line, true);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;
            }
        }

        public bool Login(string client, string password)
        {
            using (DB db = new DB())
            {
                MembershipUser mu = Membership.GetUser(client);

                if (mu == null)
                    return false;

                if (password != mu.GetPassword())
                    return false;

                return true;
            }

        }

        [WebMethod]
        public bool UpdateCollectionNode(string client, int uptime)
        {
            try
            {
                using (DB db = new DB())
                {
                    DataCollectionNode node = (from o in db.DataCollectionNodes
                                               where o.Client == client
                                               select o).FirstOrDefault();

                    if (node != null)
                    {
                        node.Uptime = uptime;

                        db.SaveChanges();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        [WebMethod]
        public DataCollectionNode GetCollectionNode(string client, string password)
        {
            try
            {
                using (DB db = new DB())
                {
                    MembershipUser mu = Membership.GetUser(client);

                    if (mu == null)
                        return null;

                    if (password != mu.GetPassword())
                        return null;

                    DataCollectionNode node = (from o in db.DataCollectionNodes
                                               where o.Client == client
                                               select o).FirstOrDefault();

                    if (node != null)
                    {
                        return node;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

        }

        [WebMethod]
        public string LoginServer(string client, string password)
        {
            try
            {
                using (DB db = new DB())
                {
                    MembershipUser mu = Membership.GetUser(client);

                    if (mu == null)
                        return false.ToString();

                    if (password != mu.GetPassword())
                        return false.ToString();

                    DataCollectionNode node = (from o in db.DataCollectionNodes
                                                where o.Client == client
                                                select o).FirstOrDefault();

                    if (node != null)
                    {
                        return true.ToString();
                    }
                    else
                    {
                        return false.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        [WebMethod]
        public bool InsertDevice(NodeLine device)
        {
            try
            {
                using (DB db = new DB())
                {
                    NodeLine node = (from o in db.DeviceSetups
                                      where o.Id == device.DeviceSetupId
                                      select new NodeLine
                                      {
                                          DeviceSetupId = o.Id,
                                          Client = o.LineSetup.DataCollectionNode.Client,
                                          AddressType = o.AddressType,
                                          DataType = o.DataType,
                                          DowntimeThreshold = o.LineSetup.DowntimeThreshold,
                                          UptimeThreshold = o.LineSetup.UptimeThreshold,
                                          IPAddress = o.IpAddress,
                                          Line = o.LineSetup.Line,
                                          TagType = o.TagType,
                                          TrackDowntime = o.TrackDowntime,
                                          TrackingType = o.TrackingType,
                                          TagName = o.TagName
                                      }).FirstOrDefault();

                    if (node != null)//Already exists
                    {
                        DeviceSetup setup = (from o in db.DeviceSetups
                                             where o.Id == node.DeviceSetupId
                                             select o).FirstOrDefault();

                        if (setup != null)
                        {
                            setup.IpAddress = device.IPAddress;
                            setup.TagName = device.TagName;
                            setup.TagType = device.TagType;
                            setup.TrackDowntime = device.TrackDowntime;
                            setup.TrackingType = device.TrackingType;

                            setup.AddressType = device.AddressType;
                            setup.DataType = device.DataType;
                            setup.LineSetup.Line = device.Line;
                            setup.LineSetup.DowntimeThreshold = device.DowntimeThreshold;
                            setup.LineSetup.UptimeThreshold = device.UptimeThreshold;
                            setup.LineSetup.DataCollectionNode.Client = device.Client;
                            setup.LineSetup.DataCollectionNode.ServerName = device.ServerName;


                            return db.SaveChanges() > 0;
                        }
                        
                    }
                    else
                    {
                        DeviceSetup setup = new DeviceSetup();

                        setup.IpAddress = device.IPAddress;
                        setup.TagName = device.TagName;
                        setup.TagType = device.TagType;
                        setup.TrackDowntime = device.TrackDowntime;
                        setup.TrackingType = device.TrackingType;

                        setup.AddressType = device.AddressType;
                        setup.DataType = device.DataType;

                        DataCollectionNode dataNode = (from o in db.DataCollectionNodes
                                                       where o.Client == device.Client
                                                       select o).FirstOrDefault();

                        if (dataNode == null)
                        {
                            dataNode = new DataCollectionNode();
                            dataNode.Password = "password";

                            setup.LineSetup.DataCollectionNode = dataNode;
                        }

                        setup.LineSetup.DataCollectionNode.Client = device.Client;

                        LineSetup lineSetup = (from o in db.LineSetups
                                               where o.DataCollectionNode.Id == dataNode.Id
                                               select o).FirstOrDefault();

                        if (lineSetup == null)
                        {
                            lineSetup = new LineSetup();
                            setup.LineSetup = lineSetup;
                        }

                        setup.LineSetup.Line = device.Line;
                        setup.LineSetup.DowntimeThreshold = device.DowntimeThreshold;
                        setup.LineSetup.UptimeThreshold = device.UptimeThreshold;

                        db.AddToLineSetups(lineSetup);

                        return db.SaveChanges() > 0;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;

            }
        }

        [WebMethod]
        [return: System.Xml.Serialization.XmlElement(typeof(NodeLine))]
        public NodeLine GetDevice(string client, string line)
        {
            try
            {
                using (DB db = new DB())
                {
                    NodeLine setup = (from o in db.DeviceSetups
                                      where o.LineSetup.DataCollectionNode.Client == client
                                      && o.LineSetup.Line == line
                                      select new NodeLine { DeviceSetupId = o.Id, Client = client, AddressType = o.AddressType, DataType = o.DataType, DowntimeThreshold = o.LineSetup.DowntimeThreshold, UptimeThreshold = o.LineSetup.UptimeThreshold,
                                          IPAddress = o.IpAddress, Line = line, TagType = o.TagType, TrackDowntime = o.TrackDowntime, TrackingType = o.TrackingType, TagName = o.TagName}).FirstOrDefault();

                    if (setup != null)
                    {
                        return setup;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;
            }
        }

        [WebMethod]
        [return: System.Xml.Serialization.XmlElement(typeof(List<NodeLine>))]
        public List<NodeLine> GetDevices(string client)
        {
            try
            {
                using (DB db = new DB())
                {
                    List<NodeLine> setups = (from o in db.DeviceSetups
                                         where o.LineSetup.DataCollectionNode.Client == client
                                        select new NodeLine { DeviceSetupId = o.Id, Client = client, AddressType = o.AddressType, DataType = o.DataType, DowntimeThreshold = o.LineSetup.DowntimeThreshold, UptimeThreshold = o.LineSetup.UptimeThreshold, IPAddress = o.IpAddress,
                                        Line = o.LineSetup.Line, TagType = o.TagType, TrackDowntime = o.TrackDowntime, TrackingType = o.TrackingType, TagName = o.TagName }).ToList();

                    if (setups != null)
                    {
                        return setups;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                String fileName = this.Server.MapPath("~/App_Data/log.txt");
                File.AppendAllText(fileName, ex.ToString());
                throw ex;
            }
        }


        [WebMethod]
        public int GetLatestSku(string client, string line)
        {
            try
            {
                using (DB db = new DB())
                {
                    return (from o in db.ThroughputHistory.Include("Throughput")
                            where o.Client == client && o.Line == line
                            orderby o.Date descending
                            select o.Throughput.PerHour).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }


        [WebMethod]
        public string Test()
        {
            try
            {
                using (DB db = new DB())
                {
                    return "true";
                }
            }
            catch (Exception ex)
            {

                return ex.ToString();
            }
        }
    }
}
