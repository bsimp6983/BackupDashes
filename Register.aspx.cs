using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Web.Configuration;
using System.Net.Mail;
using System.Net;

namespace DowntimeCollection_Demo
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string op = Request["op"];
            if (string.Equals(op, "reg-from-wp", StringComparison.CurrentCultureIgnoreCase))
            {
                string username = Request["username"];
                string password = Request["password"];
                string name = Request["name"];
                string email = Request["email"];
                string phone = Request["phone"];
                string r = createNewUser(username, password,name, email, phone);
                Response.Write(r);
                Response.End();
            }
        }

        private string createNewUser(string username, string password, string name,string email, string phone)
        {
            try
            {
                if (Membership.GetUser(username) != null) return "Please choose a different username.  This username is already registered.";
                MembershipUser mu = Membership.CreateUser(username, password, email);
                mu.IsApproved = true;
                Membership.UpdateUser(mu);
                WebProfile profile = WebProfile.GetProfile(mu.UserName);
                profile.Name = name;
                profile.Phone = phone;
                profile.ExpireDate = DateTime.Now.AddDays(30);
                profile.ActiveCode = Guid.NewGuid().ToString();
                profile.SentMail = false;
                profile.Save();

                try
                {
                    string body = string.Format(@"<p>Thank you for registering with the Thrive Downtime Collection System.  Your username and password are listed below.  Please click <a href='http://www.downtimecollectionsolutions.com/app/demo/active.aspx?user={0}&code={1}' target='_blank'>here</a> to activate your account.  <br />Click below to get started!<br /><a href='http://www.downtimecollectionsolutions.com/login'>http://www.downtimecollectionsolutions.com/login</a><br />User:{2}<br />Pass:{3}<br />=<br /><br />If you have any questions you can contact us at <a href='mailto:Info@downtimecollectionsolutions.com'>Info@downtimecollectionsolutions.com</a> or you can call our corporate office at 567-686-1040 and press 2 for Tech Support.</p>", 
                    Server.UrlEncode(mu.UserName), 
                    Server.UrlEncode(profile.ActiveCode.ToString()), username, password);
                    SendMailToTim("A new account has been created.", string.Format(@"<p>UserName:{0}<br/>Password:{1}<br/>Name:{2}<br/>Email:{3}<br/>Phone:{4}<br/></p>",username,password,name,email,phone));
                }
                catch (Exception)
                {
                    Membership.DeleteUser(mu.UserName);
                    return "Failed to create the account, an error occurred while sending an email to you.";
                }

                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void SendMailToTim(string title, string msg)
        {
            try
            {
                SendMail("tim.saddoris@infostreamusa.com", title, msg);
                //SendMail("joe.clauson@infostreamusa.com", title, msg);
            }
            catch (Exception e)
            {

                throw (e);
            }
        }

        private void SendMail(string email, string title, string msg)
        {
            int port = 25;
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["EmailServerPort"]))
            {
                if (!int.TryParse(WebConfigurationManager.AppSettings["EmailServerPort"], out port))
                {
                    port = 25;
                }
            }
            if (WebConfigurationManager.AppSettings["SendEmails"].ToLower() != "true")
            {
                return;
            }

            MailAddress sender = null;
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["EmailSenderAddress"]))
            {
                sender = new MailAddress(WebConfigurationManager.AppSettings["EmailSenderAddress"],WebConfigurationManager.AppSettings["EmailSenderDisplayName"]);
            }

            MailMessage mm = new MailMessage();
            mm.To.Add(new MailAddress(email));
            mm.Body = msg;
            mm.Subject = title;
            if (sender != null)
            {
                mm.Sender = sender;
            }
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["EmailFromAddress"]))
            {
                mm.From = new MailAddress(WebConfigurationManager.AppSettings["EmailFromAddress"]);
            }
            mm.IsBodyHtml = true;
            mm.SubjectEncoding = System.Text.Encoding.UTF8;
            mm.BodyEncoding = System.Text.Encoding.UTF8;

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = false;
            smtpClient.Host = WebConfigurationManager.AppSettings["EmailServer"];
            smtpClient.Port = port;
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["EmailUser"]))
            {
                smtpClient.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["EmailUser"], WebConfigurationManager.AppSettings["EmailPassword"]);
            }
            else
            {
                smtpClient.UseDefaultCredentials = true;
            }

            try
            {
                smtpClient.Send(mm);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}