using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;
using System.Web.Script.Serialization;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// Summary description for DigestEmails
    /// </summary>
    public class DigestEmails : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string result = string.Empty;

            context.Response.ContentType = "text/json";

            switch (context.Request.HttpMethod)
            {
                case "GET":
                    result = Get(context);
                    break;
                case "POST":
                    result = Create(context);
                    break;
                case "PUT":
                    result = Update(context);
                    break;
                case "DELETE":
                    result = Delete(context);
                    break;

            }

            context.Response.Clear();
            context.Response.Write(result);
            context.Response.End();
        }

        protected object ToObject(DigestEmailView view)
        {
            if (view != null)
            {
                return new
                {
                    Id = view.Id,
                    Email = view.Email,
                    Lines = view.Lines,
                    IsDaily = view.IsDaily,
                    IsWeekly = view.IsWeekly
                };
            }

            return null;
        }

        protected string Get(HttpContext context)
        {
            string client = context.User.Identity.Name;

            int emailId = 0;

            if (!int.TryParse(context.Request["id"], out emailId))
                emailId = 0;

            bool getLines = false;

            if (!bool.TryParse(context.Request["l"], out getLines))
                getLines = false;

            if (!getLines)
            {
                if (emailId > 0)
                {
                    DigestEmailView view = DigestEmailHelper.GetEmailView(emailId);

                    return ConvertToJsonString(ToObject(view));
                }
                else
                {

                    List<object> objs = new List<object>();

                    if (!string.IsNullOrEmpty(client))
                    {
                        List<DigestEmailView> views = DigestEmailHelper.GetClientEmailViews(client);

                        foreach (DigestEmailView view in views)
                        {
                            objs.Add(ToObject(view));
                        }
                    }

                    return ConvertToJsonString(objs);
                }
            }
            else
            {
                List<string> lines = DigestEmailHelper.GetClientLines(client);

                return ConvertToJsonString(lines);
            }
        }

        protected string Create(HttpContext context)
        {
            if (!string.IsNullOrEmpty(context.Request["delete"]))
            {
                return Delete(context);
            }
            else if (!string.IsNullOrEmpty(context.Request["update"]))
            {
                return Update(context);
            }
            else
            {
                DigestEmailView view = Deserialize(Encoding.ASCII.GetString(context.Request.BinaryRead(context.Request.ContentLength)));

                if (view != null)
                {
                    string client = context.User.Identity.Name;
                    
                    view.Client = client;
                    view = DigestEmailHelper.Insert(view);

                    return ConvertToJsonString(ToObject(view));
                }

                return ConvertToJsonString(false);
            }
        }

        protected string Update(HttpContext context)
        {
            DigestEmailView view = Deserialize(Encoding.ASCII.GetString(context.Request.BinaryRead(context.Request.ContentLength)));

            if (view != null)
            {
                if (view.Client != context.User.Identity.Name)
                    view.Client = context.User.Identity.Name;

                bool suc = DigestEmailHelper.Update(view);

                return ConvertToJsonString(ToObject(view));
            }

            return ConvertToJsonString(false);
        }

        protected string Delete(HttpContext context)
        {
            string client = context.User.Identity.Name;

            bool success = false;

            if (!string.IsNullOrEmpty(client))
            {
                int emailId = 0;

                if (!int.TryParse(context.Request["Id"], out emailId))
                    emailId = 0;
                else if (!int.TryParse(context.Request["id"], out emailId))
                    emailId = 0;

                if (emailId == 0)
                    return null;

                success = DigestEmailHelper.DeleteFromClient(emailId, client);
            }

            return ConvertToJsonString(success);
        }

        public string ConvertToJsonString(object obj)
        {
            JsonSerializer js = new JsonSerializer();
            js.Converters.Add(new JavaScriptDateTimeConverter());
            System.IO.TextWriter tw = new System.IO.StringWriter();
            js.Serialize(tw, obj);
            return tw.ToString();

        }

        private DigestEmailView Deserialize(string data)
        {
            try
            {
                var jss = new JavaScriptSerializer();

                DigestEmailView view = new DigestEmailView();

                view = jss.Deserialize<DigestEmailView>(data);

                return view;
            }
            catch (Exception)
            {
                return null;
            }
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