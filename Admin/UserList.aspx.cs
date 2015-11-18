using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.Web.Profile;
using System.Configuration;

namespace DowntimeCollection_Demo.Admin
{
    public partial class UserList : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            GetDataSoure();
            DataBind();
        }

        private DataSet GetAllUsers()
        {
            
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt = ds.Tables.Add("Users");

            var muc = Membership.GetAllUsers();

            dt.Columns.Add("UserName", typeof (string));
            dt.Columns.Add("Name", typeof (string));
            dt.Columns.Add("Email", typeof (string));
            dt.Columns.Add("CreationDate", typeof (DateTime));
            dt.Columns.Add("Expire Date", typeof (DateTime));
            dt.Columns.Add("Phone", typeof (string));
            dt.Columns.Add("Password", typeof (string));
            dt.Columns.Add("AdminPassword", typeof (string));
            dt.Columns.Add("EstimatedOutput", typeof (int));
            dt.Columns.Add("IsApproved", typeof (bool));
            dt.Columns.Add("Eff Chart Enabled", typeof (bool));
            dt.Columns.Add("Last Login", typeof (DateTime));
            dt.Columns.Add("ShiftStart", typeof (DateTime));

            /* Here is the list of columns returned of the Membership.GetAllUsers() method
             * UserName, Email, PasswordQuestion, Comment, IsApproved
             * IsLockedOut, LastLockoutDate, CreationDate, LastLoginDate
             * LastActivityDate, LastPasswordChangedDate, IsOnline, ProviderName
             */

            List<UserInfo> info = GetUserInfo();

            DateTime d = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);

            foreach(MembershipUser mu in muc) {
                WebProfile profile = WebProfile.GetProfile(mu.UserName);
                var dr = dt.NewRow();
                dr["UserName"] = mu.UserName;
                dr["Name"] = profile.Name;
                dr["Email"] = mu.Email;
                dr["CreationDate"] = mu.CreationDate;
                dr["Expire Date"] = profile.ExpireDate;
                dr["Phone"] = profile.Phone;
                dr["Password"] = mu.GetPassword();
                dr["EstimatedOutput"] = 0;
                dr["IsApproved"] = mu.IsApproved;
                dr["Last Login"] = mu.LastLoginDate;
                dr["ShiftStart"] = d;
                dr["Eff Chart Enabled"] = "False";//False on default

                Guid guid = (Guid)mu.ProviderUserKey;

                UserInfo us = (from o in info
                               where o.UserId == guid
                               select o).FirstOrDefault();

                if (us != null)
                {
                    dr["Eff Chart Enabled"] = us.EffChartEnabled;
                    dr["ShiftStart"] = us.ShiftStart;
                    dr["EstimatedOutput"] = us.EstimatedOutput;
                    dr["AdminPassword"] = us.AdminPassword;
                }

                dt.Rows.Add(dr);
            }
            return ds;
        }

        private List<UserInfo> GetUserInfo()
        {
            using (DB db = new DB())
            {
                var q = from o in db.UserInfoSet
                        select o;

                return q.ToList();
            }
        }

        private void GetDataSoure()
        {

            int page;
            int.TryParse(Request.QueryString["page"], out page);

            _users = GetAllUsers();
        }

        private DataSet _users;

        protected DataSet Users
        {
            get
            {
                return _users;
            }
        }


        /// <summary>
        /// 获取 GridView 中选中的对象
        /// </summary>
        private List<string> GetSelectedUserNames()
        {
            List<string> ret = new List<string>();

            foreach (GridViewRow gRow in GridView1.Rows)
            {
                if (gRow.RowType == DataControlRowType.DataRow)
                {
                    // 列[0] 是复选框所在的列
                    CheckBox chkSelected = (CheckBox)gRow.Cells[0].FindControl("chkSelected");
                    if (chkSelected.Checked)
                    {
                        // 列[1]是用户名
                        string s = gRow.Cells[1].Text;
                        ret.Add(s);
                    }
                }
            }
            return ret;
        }
        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "delete user":
                    string userName = (string)e.CommandArgument;

                    var membershipUser = Membership.GetUser(userName);
                    if (membershipUser != null)
                    {
                        var providerUserKey = membershipUser.ProviderUserKey;
                        if (providerUserKey != null)
                        {
                            Guid userId = (Guid)providerUserKey;

                            Membership.DeleteUser(userName);

                
                            DCSDashboardDemoHelper.deleteUserInfo(userId);
                        }
                    }

                    Response.Redirect(Request.RawUrl);
                    break;
                case "edit user":
                {
                    GridViewRow row = (GridViewRow)((WebControl)e.CommandSource).Parent.Parent;
                    row.RowState = DataControlRowState.Edit;
                    GridView1.EditIndex = row.RowIndex;
                    GetDataSoure();
                    DataBind();
                }
                    break;
                case "save":
                {
                
                    GridViewRow row = (GridViewRow)((WebControl)e.CommandSource).Parent.Parent;
                    string email = ((TextBox)row.FindControl("EmailTextBox")).Text;
                    string phone = ((TextBox)row.FindControl("PhoneTextBox")).Text;
                    string name = ((TextBox)row.FindControl("NameTextBox")).Text;
                    string password = ((TextBox)row.FindControl("PasswordTextBox")).Text;
                    string adminPassword = ((TextBox)row.FindControl("AdminPasswordTextBox")).Text;
                    DateTime expireDate = DateTime.Parse(((TextBox)row.FindControl("ExpireDateTextBox")).Text);
                    bool isApproved = ((CheckBox)row.FindControl("CheckBoxIsApproved")).Checked;
                    bool effEnabled = ((CheckBox)row.FindControl("EffChartEnabled")).Checked;
                    DateTime shiftStart = DateTime.Parse(((TextBox)row.FindControl("ShiftStartTextBox")).Text);
                    int estOutput = Int16.Parse(((TextBox)row.FindControl("EstimatedOutputTextBox")).Text);
                
                
                    MembershipUser user = Membership.GetUser(e.CommandArgument.ToString());
                    if (user != null)
                    {
                        WebProfile profile = WebProfile.GetProfile(user.UserName);
                        user.Email = email;
                        user.IsApproved = isApproved;
                        profile.ExpireDate = expireDate;
                        profile.Phone = phone;
                        profile.Name = name;
                        profile.Save();

                        Membership.UpdateUser(user);

                        user.ChangePassword(user.GetPassword(), password);

                        UserInfo info = new UserInfo();

                        if (user.ProviderUserKey != null) info.UserId = (Guid)user.ProviderUserKey;
                        info.EffChartEnabled = effEnabled;
                        info.ShiftStart = shiftStart;
                        info.EstimatedOutput = estOutput;
                        info.AdminPassword = adminPassword;

                        DCSDashboardDemoHelper.updateUserInfo(info);
                    }

                    row.RowState = DataControlRowState.Normal;
                    GridView1.EditIndex = -1;
                    GetDataSoure();
                    DataBind();
                }
                    break;
                case "cn_cancel":
                {
                    GridViewRow row = (GridViewRow)((WebControl)e.CommandSource).Parent.Parent;
                    row.RowState = DataControlRowState.Normal;
                    GridView1.EditIndex = -1;
                    GetDataSoure();
                    DataBind();
                }
                    break;
            }
        }
    }
}