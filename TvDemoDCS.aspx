<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TvDemoDCS.aspx.cs" Inherits="DCSDemoData.TvDemoDCS" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Tv Demo Reason Codes</title>
<link rel="stylesheet" href="Styles/DCSDemo_style.css" type="text/css" media="screen" />
<link rel="stylesheet" type="text/css" media="all" href="Scripts/jScrollPane.css" />
<link rel="stylesheet" type="text/css" media="all" href="Styles/demoStyles.css" />
<script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="Scripts/jquery.mousewheel.js" type="text/javascript"></script>
<script src="Scripts/jScrollPane.js" type="text/javascript"></script>
<script src="DSCtvdemo.js" type="text/javascript"></script>
<script src="Scripts/datetimepicker_css.js" type="text/javascript"></script>


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
	<div id="header">
    	<div id="logo_content">
        	<img src="images/header-logo.png" alt="" />
        <div style="float: right;">
            <a href="TvDemoDCS.aspx">All</a>
            <a href="TvDemoDCS.aspx?line=1">Line 1</a>
            <a href="TvDemoDCS.aspx?line=2">Line 2</a>
            <a href="TvDemoDCS.aspx?line=3">Line 3</a>
            <a href="TvDemoDCS.aspx?line=4">Line 4</a>
            <a href="TvDemoDCS.aspx?line=5">Line 5</a>
        </div>
        </div>
    </div>
    
    <div id="main">
        <div style="width:350px;height:30px;position:absolute;margin:-35px 0px 0px 500px;font-size:16px;">
        Date:<input type="text" style="width:80px;height:22px;font-size:12px;font-weight:normal;" readonly="readonly" id="f_startdate" onclick="NewCssCal('f_startdate','MMDDYYYY','arrow',false,12,true)" />
         To <input type="text" style="width:80px;height:22px;font-size:12px;font-weight:normal;" readonly="readonly" id="f_enddate" onclick="NewCssCal('f_enddate','MMDDYYYY','arrow',false,12,true)" />
        <input type="button" style="width:40px;" value="GO" id="f_go_btn" />
        <input type="button" style="width:40px;" value="DEL" id="f_del_btn" />
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
        	&copy; 2012 InfoStream Solutions
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