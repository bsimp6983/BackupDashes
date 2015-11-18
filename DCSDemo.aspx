﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DCSDemo.aspx.cs" Inherits="DCSDemoData.DCSDemo" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>DCS OI</title>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="styles/DCSDemo_style.css" type="text/css" media="screen" />
    <link rel="stylesheet" type="text/css" media="all" href="Scripts/jquery.jscrollpane.css" />
    <link rel="stylesheet" type="text/css" media="all" href="styles/demoStyles.css" />

    <!--[if lt IE 9]>
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.9.2/jquery-ui.min.js"></script>
    <![endif]-->
    <!--[if gte IE 9]><!-->
        <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js"></script>
        <script type="text/javascript" src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.2/jquery-ui.min.js"></script>
    <!--<![endif]-->
    

    <script src="scripts/jquery.jmodal.js" type="text/javascript"></script>
    <script src="scripts/jquery.mousewheel.js" type="text/javascript"></script>
    <script src="Scripts/jquery.jscrollpane.min.js" type="text/javascript"></script>
    <script src="Scripts/moment.min.js" type="text/javascript"></script>
    <script src="DCSDemo.js" type="text/javascript"></script>
    <script src="Scripts/jquery-ui-timepicker-addon.js" type="text/javascript"></script>
    <script src="Scripts/user.js" type="text/javascript"></script>

    <script src="scripts/floating.js" type="text/javascript"></script>
    <script src="scripts/shared.js" type="text/javascript"></script>
    <script src="scripts/checkpassword.js" type="text/javascript"></script>

    <script type="text/javascript">

        var hashcode = "<%: HashCode %>";

        function HelpPopup() {
            $("#dialog:ui-dialog").dialog("destroy");
            $("#dialog-confirm").dialog({
                resizable: false,
                height: "auto",
                width: 400,
                modal: true,
                buttons: {
                    OK: function () {
                        $(this).dialog("close");
                    }
                }
            });
        }

        $(document).ready(function () {
            $('#errorMessage').text('').val('').hide();

            getUser(function (user) {
                if (user) {

                    if (!user.hideHelper) {

                        HelpPopup();

                        $('#toprightnav').append('<a href="#" onclick="HelpPopup()" id="openHelpPopup" style="width:100%;margin-top:20px;">Help</a>');
                        $('#openHelpPopup').button();
                        $('#openHelpPopup span').css('font-size', '13px')
                                    .css('font-weight', 'bold')
                                    .css('color', 'red');
                    }

                    if (user.hidePanel) {
                        $('#toprightnav').hide();                    
                    }
                }

            });
        });	    

        function getThroughPuts(callback) {
            var url = 'http://api.thrivemes.com/dashboard/throughputs/<%: Line %>?secret=<%: HashCode %>';

            $.get(url, function (data) {
                callback(data);
            });
        }

    </script>


    <style type="text/css">
        #start_date_time
        {
            margin:3px 0px 3px 4px\9!important;
            *+margin:3px 0px 3px 4px;
            _margin:3px 0px 3px 4px;
        }
        
        /* Make this work on Firefox*/
        @-moz-document url-prefix()
        {
            #start_date_time{margin:3px;}
        }
        

        .ui-tooltip {
            padding: 10px 20px;
            color: white;
            font: bold 14px "Helvetica Neue", Sans-Serif;
            text-transform: uppercase;
            box-shadow: 0 0 7px black;
            width: 150px;
        }
        
        .ui-timepicker-div .ui-widget-header { margin-bottom: 8px; }
        .ui-timepicker-div dl { text-align: left; }
        .ui-timepicker-div dl dt { height: 25px; margin-bottom: -25px; }
        .ui-timepicker-div dl dd { margin: 0 10px 10px 65px; }
        .ui-timepicker-div td { font-size: 90%; }
        .ui-tpicker-grid-label { background: none; border: none; margin: 0; padding: 0; }

        .ui-timepicker-rtl{ direction: rtl; }
        .ui-timepicker-rtl dl { text-align: right; }
        .ui-timepicker-rtl dl dd { margin: 0 65px 10px 10px; }
        
        .ui-datepicker span,div,table,tr,td,th,col,a,p,ul,li
        {
            font-size: 12px;
	        font-family:Arial, Helvetica, sans-serif;
	        color:#000000;
        }

        #f_search_btn
        {
            cursor: pointer;
        }

    </style>

</head>
<body>
    
<div id="dialog-confirm" title="Tips" style="display:none;font-size:14px;">
	    <p style="font-size:14px;">To manually create a downtime event click "Create New Event" at the bottom. Then select for how many minutes the machine was down.  </p>
        <p style="font-size:14px;">This process can be automated by clicking <a href="http://legacy.downtimecollectionsolutions.com/index.php/tour/machine-integration/" target="_blank" style="color:Blue; text-decoration:underline;font-size:14px;font-weight:bold;">here</a>.</p>
        <p style="font-size:14px;">For more information on this process click <a href="http://legacy.downtimecollectionsolutions.com/index.php/tour/operator-interface/" target="_blank" style="color:Blue; text-decoration:underline;font-size:14px;font-weight:bold;">here</a>.</p>
    </div>
	<div id="header">
    	<div id="logo_content">
        	<img src="images/header-logo.png" alt="" />
        </div>

        <form runat="server" ID="lightForm" style="cursor: pointer; margin: -40px 30% 0 0; float: right; display: none;">
            <span style="font-size: 20px;">Lights: </span>
            <asp:DropDownList ID="drpLights" runat="server" style="width: 70px; height: 30px; font-size: 20px">
                <asp:ListItem Text="On" Value="On"></asp:ListItem>
                <asp:ListItem Text="Off" Value="Off"></asp:ListItem>
            </asp:DropDownList>
            <br />
            <span style="font-size: 10px; font-weight: bold;">Since: </span>
            <asp:Label ID="lblSince" runat="server" style="font-size: 10px; font-weight: bold;">---</asp:Label>
        </form>

        <asp:Label ID="lblLine" runat="server" style="margin: -35px 30% 0 0; float: right;" />

        <h2 id="errorMessage" style="text-align: center;">
            ERROR: No jQuery 
        </h2>
    </div>
    
    <div id="main">
        <div style="width:370px;height:30px;position:absolute;margin:-35px 0px 0px 475px;font-size:16px;">Date:<input type="text" 
                style="width:70px; height:22px;font-size:12px;font-weight:normal;" 
                id="f_startdate"  />:<input type="text" style="width:70px; height:22px;font-size:12px;font-weight:normal;" id="f_enddate" />
        <input type="button" style="width:40px;padding:0;" value="GO" id="f_go_btn" />
        <input type="button" style="width:40px;padding:0;" value="DEL" id="f_del_btn" />
        <input type="button" style="width:40px;padding:0;" value="MRG" id="f_mrg_btn" />
        <input type="button" style="width:40px;padding:0;" value="SPLT" id="f_splt_btn" />

        </div>
        <div style="width:854px;">
            <div style="overflow:auto;" id="DataGridPanel">  
    	        <table width="100%" border="0" cellspacing="0" cellpadding="0" id="DataGrid">
                </table>
            </div>
        </div>
    </div>
    
    <div id="footer">
		<p id="copyright">
        	&copy; 2013 InfoStream Solutions
        </p>
      <input type="button" id="create_new_event" value="" />
        <a href="http://www.infostreamusa.com/" target="_blank" id="create_by" title="Website Design Toledo by InfoStream Solutions">Website Design by InfoStream Solutions</a>
    </div>
    <!--JQuery Windows-->
    <div id="AssignReasonCodePopWindow" style="display:none;">
        <input type="hidden" id="id" />
        <input type="hidden" id="orgValue" />
        <div id="step0">
            <label for="start_date_time" id="for_start_date_time">Start Date & Time:</label><input type="text" id="start_date_time" />
            <br />
            <label for="minutes" id="for_minutes">Minutes:</label><input type="input" id="minutes" value="" /> <button id="startTimer">Start Timer</button> <label id="lblTimer"></label><br />
            <label for="Occurences" id="for_Occurences" style="display:none;">Occurences:</label><input style="display:none;" type="input" id="Occurences" value="1" />
            <div style="display:none;">
                <br />
                <label for="line" id="for_line">Line:</label>
                <select id="line">
                    <option value="line1">Line1</option>
                </select>
            </div>
            <input type="button" value="[Next] Select Reason Code" id="btn_next_select_reason_code" />
        </div>
        <div id="step1"></div>
        <div id="step2"></div>
        <div id="step3"></div>
        <div id="step4">
            <textarea id="comment" style="width: 668px;">Operator puts in comment here.</textarea>
            <br />
            <div>
            <label id="lblThroughput" style="margin: 0; width: 10%; float: left; font-size: 20px">SKU:</label>
            <select id="slThroughput" style="width: 20%; height: 20px; font-size: 15px;float: left;"></select>
            <label id="lblOption1" style="margin: 0 0 0 5px; width: 15%; float: left; font-size: 20px; display: none;">Option 1:</label> 
            <select id="slOption1" style="width: 20%; height: 20px; font-size: 15px;float: left; display: none;"></select>
            <label id="lblOption2" style="margin: 0 0 0 5px; width: 13%; float: left; font-size: 20px; display: none;">Option 2:</label>
            <select id="slOption2" style="width: 20%; height: 20px; font-size: 15px;float: left; display: none;"></select>
            </div>
            <br />
            <input id="btnSave" type="button" value="Save" />
        </div>
    </div>
    <div id="SplitDowntimesPopWindow">
        <div id="splitdt-choose-time">
            <span style="width: 325px; display: inline-block; margin-left: 28px;">First Event (HH:MM:SS)
                <button style="margin-bottom: 10px; margin-left: 10px; width: 48px; font-size: 20px;">=</button>
            </span>
            <span>Remaining Downtime</span>
            <input id="splitdt-time-original" type="text" value="03:00:00" />
            <input id="splitdt-time-new" type="text" value="00:00:00" disabled="disabled" />
            <input id="splitdt-time-seconds" type="hidden" value="0" />
            <input id="splitdt-save-button" type="submit" value="Save" style="font-size: 30px; margin-top: 15px; margin-left: 523px;" />
        </div>
    </div>
</body>
</html>