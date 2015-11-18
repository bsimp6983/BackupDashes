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
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                lblError.Visible = false;
            }
            string wpaction = Request["wpaction"];
            if (!string.IsNullOrEmpty(wpaction))
            {
                if (string.Equals("login", wpaction, StringComparison.CurrentCultureIgnoreCase))
                {
                    string userName = Request["UserName"];
                    string password = Request["Password"];
                    bool remember = string.Equals(Request["RememberMe"], "on", StringComparison.CurrentCultureIgnoreCase);
                    if (Membership.ValidateUser(userName, password))
                    {
                        //sendMailToUser(userName, password);
                        lblError.Text = "";

                        WebProfile profile = WebProfile.GetProfile(userName);

                        if (profile != null)
                        {
                            FormsAuthentication.SetAuthCookie(userName, remember);

                            if (userName != "demoUser")
                                Response.Redirect(Request.ApplicationPath + "default.aspx");
                            else
                                Response.Redirect(Request.ApplicationPath + "DashboardDemo.aspx");
                        }
                        else
                        {
                            lblError.Text = "Account doesn't exist";
                            Response.Redirect(Request.ApplicationPath + "default.aspx");
                        }


                    }
                    else
                    {
                        lblError.Text = "Invalid user name or password";
                    }
                }
                else
                {
                    lblError.Text = "Error";
                }
            }
            else
            {
                lblError.Text = string.Empty;
            }
        }

        protected void LoggedIn(object sender, EventArgs e)
        {
            sendMailToUser(Login1.UserName, Login1.Password);
        }

        private void sendMailToUser(string username, string password)
        {
            string  name, email;

            MembershipUser mu = Membership.GetUser(username);
            WebProfile profile = WebProfile.GetProfile(mu.UserName);
            if (profile.SentMail) return;//如果发送过邮件了

            username = mu.UserName;
            email = mu.Email;

            name=profile.Name ;
            try
            {
                string body = string.Format(@"<p>Thank you for registering with the Thrive Downtime Collection System.  Your username and password are listed below. 
<br />
Click below to get started!<br />
<a href='http://www.downtimecollectionsolutions.com/login'>http://www.downtimecollectionsolutions.com/login</a><br />
User:{0}<br />
Pass:{1}<br />
<br /><br />
If you have any questions you can contact us at <a href='mailto:Info@downtimecollectionsolutions.com'>Info@downtimecollectionsolutions.com</a> or you can call our corporate office at 567-686-1040 and press 2 for Tech Support.
</p>",
    username,
    password);
                //send to user
                SendMail(email, "Your Account", body);


                profile.SentMail = true;
                profile.Save();
            }
            catch (Exception)
            {
                //
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
                sender = new MailAddress(WebConfigurationManager.AppSettings["EmailSenderAddress"], WebConfigurationManager.AppSettings["EmailSenderDisplayName"]);
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