<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DCSDemo_bkUp.aspx.cs" Inherits="DowntimeCollection_Demo.DCSDemo" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>DCS Demo -<%=User.Identity.Name %></title>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<link rel="stylesheet" href="styles/DCSDemo_style.css" type="text/css" media="screen" />
<link rel="stylesheet" type="text/css" media="all" href="styles/jScrollPane.css" />
<link rel="stylesheet" type="text/css" media="all" href="styles/demoStyles.css" />
<script src="scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<script src="scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="scripts/jquery.mousewheel.js" type="text/javascript"></script>
<script src="scripts/jScrollPane.js" type="text/javascript"></script>
<script src="scripts/DCSDemo.js" type="text/javascript"></script>
<script src="scripts/datetimepicker_css.js" type="text/javascript"></script>

    <script src="scripts/floating.js" type="text/javascript"></script>
    <script src="scripts/shared.js" type="text/javascript"></script>
    <script type="text/javascript">
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
            HelpPopup();

            $('#toprightnav').append('<a href="#" onclick="HelpPopup()" id="openHelpPopup" style="width:100%;margin-top:20px;">Help</a>');
            $('#openHelpPopup').button();
            $('#openHelpPopup span').css('font-size', '13px')
                                .css('font-weight', 'bold')
                                .css('color', 'red');
        });	    
	</script>

    <style type="text/css">
        #start_date_time
        {
            margin:3px 0px 3px 4px\9!important;
            *+margin:3px 0px 3px 4px;
            _margin:3px 0px 3px 4px;
        }
        
        
        /* HACK Firefox*/
        @-moz-document url-prefix()
        {
            #start_date_time{margin:3px;}
        }
        
    </style>

</head>
<body>    
<div id="dialog-confirm" title="Tips" style="display:none;font-size:14px;">
	    <p style="font-size:14px;">To manually create a downtime event click "Create New Event" at the bottom. Then select for how many minutes the machine was down.  </p>
        <p style="font-size:14px;">This process can be automated by clicking <a href="http://www.downtimecollectionsolutions.com/index.php/tour/machine-integration/" target="_blank" style="color:Blue; text-decoration:underline;font-size:14px;font-weight:bold;">here</a>.</p>
        <p style="font-size:14px;">For more information on this process click <a href="http://www.downtimecollectionsolutions.com/index.php/tour/operator-interface/" target="_blank" style="color:Blue; text-decoration:underline;font-size:14px;font-weight:bold;">here</a>.</p>
    </div>
	<div id="header">
    	<div id="logo_content">
        	<img src="images/header-logo.png" alt="" />
        </div>
    </div>
    
    <div id="main">
        <div style="width:854px;">
            <div style="overflow:auto;" id="DataGridPanel">  
    	        <table width="100%" border="0" cellspacing="0" cellpadding="0" id="DataGrid">
                </table>
            </div>
        </div>
    </div>
    
    <div id="footer">
		<p id="copyright">
        	&copy; 2010 InfoStream Solutions
        </p>
      <input type="button" id="create_new_event" value="" />
        <a href="http://www.infostreamusa.com/" target="_blank" id="create_by" title="Website Design Toledo by InfoStream Solutions">Website Design Toledo by InfoStream Solutions</a>
    </div>
    <!--JQuery Windows-->
    <div id="AssignReasonCodePopWindow" style="display:none;">
        <input type="hidden" id="id" />
        <input type="hidden" id="orgValue" />
        <div id="step0">
            <label for="start_date_time" id="for_start_date_time">Start Date & Time:</label><input id="start_date_time" type="input" readonly="readonly" value="" onclick="NewCssCal('start_date_time','MMDDYYYY','arrow',true,12,true)" /><br />
            <label for="minutes" id="for_minutes">Minutes:</label><input type="input" id="minutes" value="" /><br />
            <label for="Occurences" id="for_Occurences" style="display:none;">Occurences:</label><input style="display:none;" type="input" id="Occurences" value="1" />
            <div style="display:none;">
                <br />
                <label for="line" id="for_line">Line:</label>
                <select id="line">
                    <option value="company-demo">company-demo</option>
                    <option value="line1">Line1</option>
                </select>
            </div>
            <input type="button" value="[Next] Select Reason Code" id="btn_next_select_reason_code" />
        </div>
        <div id="step1"></div>
        <div id="step2"></div>
        <div id="step3"></div>
        <div id="step4"><textarea id="comment">Operator puts in comment here.</textarea><br /><input id="btnSave" type="button" value="Save" /></div>
    </div>
</body>
</html>