using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using DCSDemoData;

namespace DowntimeCollection_Demo
{
    public partial class ExportXLS : System.Web.UI.Page
    {
        string Line = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            string op=Request["op"];
            Line = Request["line"];

            if (string.IsNullOrEmpty(op)) return;
            switch (op.ToLower())
            {
                case "md_top5downtimeevents_comments":
                    save(Top5DowntimeEvents_Comments("md"), "Comments");
                    break;
                case "dcd_top5downtimeevents_comments":
                    save(Top5DowntimeEvents_Comments("dcd"), "Comments");
                    break;
                case "md_events":
                    save(_Events("md"), "Downtimes");
                    break;
                case "dcd_events":
                    save(_Events("dcd"), "Downtimes");
                    break;
                case "dcd_efficiency":
                    save(_Efficiency(), "LineEfficiencyData");
                    break;

            }
        }

        private DataTable Top5DowntimeEvents_Comments(string type)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request["enddate"], out d))
            {
                endDate = d;
            }
            string level1 = Request["level1"];
            string level3 = Request["level3"];
            int level3_id = 0;
            int.TryParse(Request["level3_id"], out level3_id);
            List<CommentRow> result = null;
            switch (type)
            {
                case "md":
                    result = DCSDashboardDemoHelper.CommentsOptionalReasons(startDate, endDate, Line).OrderByDescending(o => o.EventStart.Value).ToList();
                    break;
                case "dcd":
                    result = DCSDashboardDemoHelper.CommentsOptionalReasons(startDate, endDate, Line).OrderByDescending(o => o.EventStart.Value).ToList();
                    break;
            }

            List<NatureOfStoppage> noses = DCSDashboardDemoHelper.GetNatureOfStoppages(startDate, endDate);
            List<Options> options = DCSDashboardDemoHelper.GetOptions();
            
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] { 
                new DataColumn("ReasonCodeId"),
                new DataColumn("ID"),
                new DataColumn("EventStart"), 
                new DataColumn("EventStop"), 
                new DataColumn("Minutes"), 
                new DataColumn("Line"), 
                new DataColumn("Client"),
                new DataColumn("Reason"),
                new DataColumn("NOS"),
                new DataColumn("Manual"),
                new DataColumn("Comment")
            });

            if (result != null)
            {
                foreach (var item in result)
                {
                    NatureOfStoppage nos = (from o in noses
                                            where o.DowntimeId == item.ID
                                            select o).FirstOrDefault();
                    
                    Options option = null;

                    if(nos != null)
                    {
                        option = (from o in options
                                  where o.Id == nos.OptionId
                                  select o).FirstOrDefault();
                    }


                    DataRow dr = dt.NewRow();
                        dr["ReasonCodeId"]=item.ReasonCodeId;
                        dr["ID"] = item.ID;
                        dr["EventStart"] =item.EventStart;
                        dr["EventStop"] =item.EventStop;
                        dr["Minutes"] =item.Minutes;
                        dr["Line"]=item.Line;
                        dr["Client"]=item.Client;
                        dr["Reason"] = item.LevelPath();
                        dr["NOS"] = (option != null ? option.Name : "");
                        dr["Manual"] = item.IsManual;
                        dr["Comment"] = item.Comment;
                     dt.Rows.Add(dr);
                }
            }
            return dt;
        }
        private System.Data.DataTable _Efficiency()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request["enddate"], out d))
            {
                endDate = d;
            }
            List<LineEfficiency> result = null;
            result = DCSDashboardDemoHelper.GetLineEfficiencyData(startDate, endDate, Line).OrderByDescending(o => o.Time.Year).ThenByDescending(o => o.Time.Month).ThenByDescending(o => o.Time.Day).ThenByDescending(o => o.Time.Hour).ThenByDescending(o => o.Time.Minute).ThenByDescending(o => o.Time.Second).ThenByDescending(o => o.Time.Millisecond).ToList();

            DataTable dt = new DataTable();

            dt.Columns.AddRange(new DataColumn[] {   
                new DataColumn("Client"),
                new DataColumn("Line"),
                new DataColumn("Time"), 
                new DataColumn("Value"),
                new DataColumn("Cases"),
                new DataColumn("Estimate"),
                new DataColumn("SKU"), 
                });
            if (result != null)
            {
                foreach (var item in result)
                {
                    DataRow dr = dt.NewRow();
                    dr["Client"] = item.Client;
                    dr["Line"] = item.Line;
                    dr["Time"] = item.Time;
                    dr["Value"] = item.Value;
                    dr["Cases"] = item.Cases;
                    dr["Estimate"] = item.Estimate;
                    dr["SKU"] = item.SKU;
                    dt.Rows.Add(dr);
                }
            }
            dt.Columns["Value"].ColumnName = "Efficiency";
            return dt;
        }
        private System.Data.DataTable _Events(string type)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime d;
            if (DateTime.TryParse(Request["startdate"], out d))
            {
                startDate = d;
            }
            if (DateTime.TryParse(Request["enddate"], out d))
            {
                endDate = d;
            }

            
            List<EventRowWithAllColumns> result = null;
            switch (type)
            {
                case "md":
                    result = DCSDashboardDemoHelper.GetEventRowsOptionalReasons(startDate, endDate, null, Line).OrderByDescending(o => o.EventStart.Value).ToList();
                    break;
                case "dcd":
                    result = DCSDashboardDemoHelper.GetEventRowsOptionalReasons(startDate, endDate, null, Line).OrderByDescending(o => o.EventStart.Value).ToList();
                    break;
            }

            List<NatureOfStoppage> noses = DCSDashboardDemoHelper.GetNatureOfStoppages(startDate, endDate);
            List<Options> options = DCSDashboardDemoHelper.GetOptions();

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] {   
                new DataColumn("ReasonCodeId"),
                new DataColumn("ID"),
                new DataColumn("EventStart"), 
                new DataColumn("EventStop"),
                new DataColumn("NOS"),
                new DataColumn("Manual"),
                new DataColumn("Level1"), 
                new DataColumn("Level2"),
                new DataColumn("Level3"), 
                new DataColumn("Minutes"), 
                new DataColumn("Line"), 
                new DataColumn("Client"),
                new DataColumn("Comment")
            });

            if (result != null)
            {
                foreach (var item in result)
                {
                    NatureOfStoppage nos = (from o in noses
                                            where o.DowntimeId == item.Id
                                            select o).FirstOrDefault();

                    Options option = null;

                    if (nos != null)
                    {
                        option = (from o in options
                                  where o.Id == nos.OptionId
                                  select o).FirstOrDefault();
                    }

                    DataRow dr = dt.NewRow();
                    dr["ReasonCodeId"] = item.ReasonCodeId;
                    dr["ID"] = item.Id;
                    dr["EventStart"] = item.EventStart;
                    dr["EventStop"] = item.EventStop;
                    dr["NOS"] = (option != null ? option.Name : "");
                    dr["Manual"] = item.IsManual;
                    dr["Level1"] = item.Level1;
                    dr["Level2"] = item.Level2;
                    dr["Level3"] = item.Level3;
                    dr["Minutes"] = item.Minutes;
                    dr["Line"] = item.Line;
                    dr["Client"] = item.Client;
                    dr["Comment"] = item.Comment;
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        private void save(DataTable dt,string filename)
        {
            DataGrid excel = new DataGrid();
            System.Web.UI.WebControls.TableItemStyle AlternatingStyle = new TableItemStyle();
            System.Web.UI.WebControls.TableItemStyle headerStyle = new TableItemStyle();
            System.Web.UI.WebControls.TableItemStyle itemStyle = new TableItemStyle();
            AlternatingStyle.BackColor = System.Drawing.Color.LightGray;
            headerStyle.BackColor = System.Drawing.Color.LightGray;
            headerStyle.Font.Bold = true;
            headerStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;
            itemStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center; ;

            excel.AlternatingItemStyle.MergeWith(AlternatingStyle);
            excel.HeaderStyle.MergeWith(headerStyle);
            excel.ItemStyle.MergeWith(itemStyle);
            excel.GridLines = GridLines.Both;
            excel.HeaderStyle.Font.Bold = true;
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            excel.DataSource = ds;   //输出DataTable的内容
            excel.DataBind();

            System.IO.StringWriter oStringWriter = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter oHtmlTextWriter = new System.Web.UI.HtmlTextWriter(oStringWriter);
            excel.RenderControl(oHtmlTextWriter);

            Response.AddHeader("Content-Disposition", "attachment; filename=" + System.Web.HttpUtility.UrlEncode(filename, System.Text.Encoding.UTF8) + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            Response.Write(oHtmlTextWriter.InnerWriter.ToString());
            Response.End();
        }
    }
}
