using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DCSDemoData;
using System.Data;

namespace DowntimeCollection_Demo.Admin
{
    public partial class Cases : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            getCases();
        }

        protected void getCases()
        {
            DateTime n = DateTime.Now;
            //DateTime d = new DateTime(n.Year, n.Month, 13, 0, 0, 0);

            List<CaseCount> rows = MillardDashboardHelper.getCasesPerHourPerLine(n.AddDays(-1), n, "SKFoods").OrderByDescending(o => o.EventStop).ToList();

            DataTable dt = new DataTable();
            dt.Columns.Add("Line");
            dt.Columns.Add("Date");
            dt.Columns.Add("Cases");

            foreach (CaseCount caseCount in rows)
            {
                DataRow row = dt.NewRow();
                row["Line"] = caseCount.Line;
                row["Date"] = caseCount.EventStop;
                row["Cases"] = caseCount.CaseCount1;
                dt.Rows.Add(row);
            }

            gridView.DataSource = dt;
            gridView.DataBind();
        }
    }
}