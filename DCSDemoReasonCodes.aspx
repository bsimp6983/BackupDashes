<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DCSDemoReasonCodes.aspx.cs" Inherits="DowntimeCollection_Demo.DCSDemoReasonCodes" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Reason Codes</title>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <link href="Styles/demo_table.css" rel="Stylesheet" type="text/css" />

    <script src="scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    
    <script src="Scripts/user.js" type="text/javascript"></script>

    <script src="scripts/jquery.treeTable.js" type="text/javascript"></script>
    <script src="scripts/DCSDemoReasonCodes.js" type="text/javascript"></script>
    <script src="Scripts/jquery.dataTables.js" type="text/javascript"></script>
    <link href="styles/jquery.treeTable.css" rel="stylesheet" type="text/css" />

    <script src="scripts/floating.js" type="text/javascript"></script>
    <script src="scripts/shared.js" type="text/javascript"></script>
    <script src="scripts/checkpassword.js" type="text/javascript"></script>
    <style type="text/css">
        #dialog-confirm dl{margin:0px;padding:0px; list-style:none;}
        #dialog-confirm dl dt{font-weight:bold;margin:0px 0px 5px 0px;padding:0px;line-height:1.5;color:Green;}
        #dialog-confirm dl dd{padding:3px 0px;margin:3px 0px;font-size:14px;}
        #dialog-confirm dl dd b{float:left;margin-right:3px;text-align:left;font-weight:bold;}

        iframe {
            width: 100%;
            height: 700px;
            border: 0;
        }
    </style>
    
    <script type="text/javascript" >
        function getParameterByName(name) {
            name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                results = regex.exec(location.search);
            return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
        }

        var client = "<%: User.Identity.Name %>";
        var hashcode = "<%: HashCode %>";
        var line = getParameterByName('line');
        var clientsAllowedToAccessShifts = ["WallyTest", "CHG"];
        var allowedToAccessShifts = false;
        var newdashboard = "http://api.thrivemes.com/dashboard/";
    </script>
</head>
<body>
    <div id="tab-container">
        <ul>
            <li><a href="#reasoncode-container">Reason Codes</a></li>
            <li><a href="#changeover-container" id="lkChangeOver" >Run Rates</a></li>
            <li class="shiftrelated"><a href="#shiftperiod-container">Shift Periods</a></li>
        </ul>
        <div id="reasoncode-container">
        
            <div style="width:90%;margin:5px auto;">
                <input type="button" id="btnSave" value="SAVE" /> <input type="button" id="btnReload" value="RELOAD" />
            </div>
            <div id="container">
    
            </div>
            <input type="button" id="btnShowGlobal" value="View Global" />
        </div>
        <div id="changeover-container">
            <iframe id="RunRates"></iframe>
        </div>
        <div class="shiftrelated" id="shiftperiod-container">
            <iframe id="ShiftPeriods"></iframe>
        </div>
        <div id="dialog-confirm" title="Tips" style="display:none;font-size:14px;">
            <dl>
                <dt>Instructions for adding layered Reason Codes.  System will only recognize three layers.</dt>
                <dd><b>Add:</b>Click on the parent and click "Add children".</dd>
                <dd><b>Rename:</b>Double click on title.</dd>
                <dd><b>Delete:</b>Click the item and press "Delete" on your keyboard</dd>
                <dd><b>Save:</b>Click Save at the Top Left.</dd>
                <dd><b>Hide Reason in Reports:</b>Tracks event time but it does not affect totaled downtime for reporting.</dd>
            </dl>
        </div>
    </div>
    <script type="text/javascript">
        setTimeout(function() { if (getParameterByName('rc') == 1) $('.ui-tabs-nav, #toprightnav').remove(); }, 1000);
    
        if (line == "") {
            document.getElementById('RunRates').src = newdashboard + "runrates/" + "?secret=" + hashcode;
            document.getElementById('ShiftPeriods').src = newdashboard + "shifts/" + "?secret=" + hashcode;
        } else {
            document.getElementById('RunRates').src = newdashboard + "runrates/" + line + "?secret=" + hashcode;
            document.getElementById('ShiftPeriods').src = newdashboard + "shifts/" + line + "?secret=" + hashcode;
        }
    </script>
</body>
</html>
