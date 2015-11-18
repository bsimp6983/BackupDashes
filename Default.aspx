<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DowntimeCollection_Demo._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <script src="scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
	<script type="text/javascript">
	    $(function () {
        <%if(Request.Cookies["first-default-dailog-opened"]==null){%>
	        $("#dialog:ui-dialog").dialog("destroy");
	        $("#dialog-confirm").dialog({
	            resizable: false,
	            height: "auto",
                width:300,
	            modal: true,
	            buttons: {
	                OK: function () {
	                    $(this).dialog("close");
	                }
	            }
	        });
            <%
            HttpCookie cookie=new HttpCookie("first-default-dailog-opened","true");
            cookie.Expires=DateTime.Now.AddYears(2);
            Response.Cookies.Add(cookie);
            } %>
	        $('#_buttons a').button();
	    });

	    
	</script>
</head>
<body><div id="dialog-confirm" title="Tips" style="display:none;font-size:14px;">
Follow the steps above to start collecting and analyzing downtime!
</div>
    <form id="form1" runat="server">
    <div style="text-align:center;width:100%;margin-top:50px;" id="_buttons">
    <table border="0" cellpadding="5" cellspacing="0" align="center">
      <tr style="font-weight:bold;font-size:1.1em;">
        <td>Step 1</td>
        <td>Step 2</td>
        <td>Step 3</td>
        <td>Step 4</td>
        <%if (User.Identity.Name.Equals("admin",StringComparison.CurrentCultureIgnoreCase))
      { %><td>Admin</td>
       <%} %>
      </tr>
      <tr>
        <td>
            <asp:LinkButton runat="server" ID="lnkReasonCodes" Text="Create Reason Codes"></asp:LinkButton>
        </td>
        <td>
            <asp:LinkButton runat="server" ID="lnkDcsDemo" Text="Enter Downtime Events"></asp:LinkButton>
        </td>
        <td>
            <asp:LinkButton runat="server" ID="analyzeData" Text="Analyze Data in Dashboard"></asp:LinkButton>
        </td>
        <td>
            <a href="DigestEmails.aspx">Daily Digest Emails</a>
        </td>
        <%if (User.Identity.Name.Equals("admin",StringComparison.CurrentCultureIgnoreCase))
      { %>
        <td><a href="Admin/UserList.aspx">Users</a></td>
        <td><a href="Admin/TvDemoControl.aspx">Tv Demo Controller</a></td>
        <td><a href="TvDemoDCS.aspx">Tv Demo Reasons</a></td>
        <td><a href="TvDashboardDemo.aspx">Tv Demo</a></td>
        <td><a href="AnnualSavings.aspx">Annual Savings</a></td>
        <td><a href="Admin/PersonSavings.aspx">People & Savings</a></td>
        <td><a href="AscommStatuses.aspx">Ascomm Statuses</a></td>
        <%} %>
      </tr>
    </table>
    </div>
    </form>
    <div id="dialog-confirm" title="Tips" style="display:none;">
	    <p>Select a date range at the top and click go.  Make sure to click on the graphs to drill down into the data.  For detailed instructions click <a href="http://www.infostreamusa.com/websites/dcs/tour/operator-interface/" target="_blank">here</a>.</p>
    </div>
</body>
</html>
