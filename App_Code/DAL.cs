using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Globalization;

namespace DowntimeCollection_Demo
{
    public class DAL
    {
        static string connectionStr = ConfigurationManager.ConnectionStrings["default"].ToString();
        public static List<DowntimeReason> GetReasons()
        {
            List<DowntimeReason> result = new List<DowntimeReason>();

            return result;
        }

        public static List<ThroughputHistory> GetHistories(string client)
        {
            List<ThroughputHistory> result = new List<ThroughputHistory>();

            using (SqlConnection con = new SqlConnection(connectionStr))
            {
                SqlCommand com = new SqlCommand("GetHistories", con);
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.Parameters.Add("@client", System.Data.SqlDbType.NVarChar, 50).Value = client;
                con.Open();
                SqlDataReader sr = com.ExecuteReader();
                if (sr.HasRows)
                {
                    ThroughputHistory tmp;

                    while (sr.Read())
                    {
                        tmp = new ThroughputHistory();
                        tmp.Client = client;
                        tmp.Date = DateTime.Parse(sr["Date"].ToString());
                        tmp.Line = sr["Line"].ToString();
                        tmp.Throughput = new Throughput();
                        tmp.Throughput.Name = sr["name"].ToString();
                        result.Add(tmp);
                    }
                }
            }

            return result;
        }

        public static List<DownTimeDataRecord> GetDowntimeData(string client, string line, DateTime? startDate, DateTime endDate)
        {
            List<DownTimeDataRecord> result = new List<DownTimeDataRecord>();

            using (SqlConnection con = new SqlConnection(connectionStr))
            {
                SqlCommand com = new SqlCommand("Service_GetDowntimeData", con);
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.Parameters.Add("@client", System.Data.SqlDbType.NVarChar, 50).Value = client;
                com.Parameters.Add("@line", System.Data.SqlDbType.NVarChar, 50).Value = line;
                if (startDate.HasValue)
                {
                    com.Parameters.Add("@startDate", System.Data.SqlDbType.DateTime).Value = startDate.Value;
                }
                else
                {
                    com.Parameters.Add("@startDate", System.Data.SqlDbType.DateTime).Value = DBNull.Value;
                }
                com.Parameters.Add("@endDate", System.Data.SqlDbType.DateTime).Value = endDate;

                con.Open();
                SqlDataReader sr = com.ExecuteReader();
                if (sr.HasRows)
                {


                    while (sr.Read())
                    {
                        result.Add(
                            new DownTimeDataRecord
                            {
                                ID = (int)sr["ID"],
                                EventStart = DateTime.Parse(sr["EventStart"].ToString()).ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                                EventStop = sr["EventStop"] is DBNull ? "" : DateTime.Parse(sr["EventStop"].ToString()).ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                                ReasonCode = sr["ReasonCodeID"] is DBNull ? null : sr["ReasonCode"].ToString(),
                                ReasonCodeID = sr["ReasonCodeID"] is DBNull ? 0 : (int)sr["ReasonCodeID"],
                                Line = line,
                                Minutes = sr["Minutes"] is DBNull ? 0 : (decimal)sr["Minutes"],
                                Client = client,
                                Comment = sr["Comment"] is DBNull ? null : sr["Comment"].ToString(),
                                EventStartDisplay = DateTime.Parse(sr["EventStart"].ToString()).ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                                EventStopDisplay = sr["EventStop"] is DBNull ? "" : DateTime.Parse(sr["EventStop"].ToString()).ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                                IsChangeOver = sr["isChangeOver"] is DBNull ? false : (bool)sr["isChangeOver"]
                            });
                    }
                }
            }

            return result;
        }

        public static List<DownTimeDataRecord> GetDowntimeDataStopAfterEndDate(string client, string line, DateTime? startDate, DateTime endDate)
        {
            List<DownTimeDataRecord> result = new List<DownTimeDataRecord>();

            using (SqlConnection con = new SqlConnection(connectionStr))
            {
                SqlCommand com = new SqlCommand("Service_GetDowntimeDataStopAfterEndDate", con);
                com.CommandType = System.Data.CommandType.StoredProcedure;
                com.Parameters.Add("@client", System.Data.SqlDbType.NVarChar, 50).Value = client;
                com.Parameters.Add("@line", System.Data.SqlDbType.NVarChar, 50).Value = line;
                if (startDate.HasValue)
                {
                    com.Parameters.Add("@startDate", System.Data.SqlDbType.DateTime).Value = startDate.Value;
                }
                else
                {
                    com.Parameters.Add("@startDate", System.Data.SqlDbType.DateTime).Value = DBNull.Value;
                }
                com.Parameters.Add("@endDate", System.Data.SqlDbType.DateTime).Value = endDate;

                con.Open();
                SqlDataReader sr = com.ExecuteReader();
                if (sr.HasRows)
                {


                    while (sr.Read())
                    {
                        result.Add(
                            new DownTimeDataRecord
                            {
                                ID = (int)sr["ID"],
                                EventStart = DateTime.Parse(sr["EventStart"].ToString()).ToString(@"MM\/dd\/yyyy HH:mm:ss"),
                                EventStop = sr["EventStop"] is DBNull ? "" : DateTime.Parse(sr["EventStop"].ToString()).ToString(@"MM\/dd\/yyyy HH:mm:ss") ,
                                ReasonCode = sr["ReasonCodeID"] is DBNull ? null : sr["ReasonCode"].ToString(),
                                ReasonCodeID = sr["ReasonCodeID"] is DBNull ? 0 : (int)sr["ReasonCodeID"],
                                Line = line,
                                Minutes = sr["Minutes"] is DBNull ? 0 : (int)sr["Minutes"],
                                Client = client,
                                Comment = sr["Comment"] is DBNull ? null : sr["Comment"].ToString(),
                                EventStartDisplay = DateTime.Parse(sr["EventStart"].ToString()).ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                                EventStopDisplay = sr["EventStop"] is DBNull ? "" : DateTime.Parse(sr["EventStop"].ToString()).ToString(@"MM\/dd\/yyyy hh:mmtt", CultureInfo.GetCultureInfo("en-US")),
                                IsChangeOver = sr["isChangeOver"] is DBNull ? false : (bool)sr["isChangeOver"]
                            });
                    }
                }
            }

            return result;
        }
    }

    
    public class DownTimeDataRecord
    {
        public int ID {get; set;}
        public string EventStart { get; set; }
        public string EventStop { get; set; }
        public string ReasonCode { get; set; }
        public int ReasonCodeID { get; set; }
        public string Line { get; set; }
        public decimal Minutes { get; set; }
        public string Client { get; set; }
        public string Comment { get; set; }
        public string EventStartDisplay { get; set; }
        public string EventStopDisplay { get; set; }
        public bool IsChangeOver { get; set; }

    }
}