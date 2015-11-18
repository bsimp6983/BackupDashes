using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using DowntimeCollection_Demo.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DowntimeCollection_Demo
{
    public partial class SKFoods_OEE : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string op = Request["op"];
                string res = string.Empty;

                if (!string.IsNullOrEmpty(op))
                {
                    switch (op.ToLower())
                    {
                        case "calculate":
                            {
                                res = calculate();
                            }
                            break;
                    }
                    Response.AddHeader("Content-type", "text/json");
                    Response.AddHeader("Content-type", "application/json");

                    Response.Write(res);
                    Response.End();
                }
            }
        }

        protected string calculate()
        {
            JavaScriptSerializer json_serializer = new JavaScriptSerializer();

            Dictionary<string, object> values = (Dictionary<string, object>)json_serializer.DeserializeObject(Request["data"]);

            OEE oee = new OEE();

            try
            {

                oee.setFrom(Convert.ToDateTime(values["from"]));
                oee.setTo(Convert.ToDateTime(values["to"]));

                oee.partsPerHour = Convert.ToDecimal(values["partsPerHour"]);
                oee.downtimeMinutes = Convert.ToDecimal(values["downtimeMinutes"]);
                oee.partsPerHour = Convert.ToDecimal(values["partsPerHour"]);
                oee.totalPieces = Convert.ToDecimal(values["totalPieces"]);
                oee.goodUnits = Convert.ToDecimal(values["goodUnits"]);

                oee.calculateTotal();

            }
            catch { }

            return ConvertToJsonString(oee);
        }

        public string ConvertToJsonString(object obj)
        {
            JsonSerializer js = new JsonSerializer();
            js.Converters.Add(new JavaScriptDateTimeConverter());
            System.IO.TextWriter tw = new System.IO.StringWriter();
            js.Serialize(tw, obj);
            return tw.ToString();

        }
    }
}