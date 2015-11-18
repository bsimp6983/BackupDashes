<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SageIndexNew.aspx.cs" Inherits="DowntimeCollection_Demo.SageIndexNew2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Sage Index Dashboard</title>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    <link rel="icon" href="images/SageFavicon.png" type="image/png">
    <link href='http://fonts.googleapis.com/css?family=BenchNine' rel='stylesheet' type='text/css'>
    <link href="SageIndex.css" rel="stylesheet" type="text/css" />
    <script src="Scripts/global.js" type="text/javascript"></script>
    <style type="text/css">
        #performanceChart
        {
            position: absolute;
            top: 111px;
            right: 54%;
            z-index: 999;
            margin-left: 52px;
            color: #0697C3;
            font-size: 11px;
        }
    </style>
    <script type="text/javascript">
   
    </script>
</head>
<body>
    <div id="header">
        <a id="performanceChart" href="PerformanceChart.aspx">Timeline / Performance Chart</a>
        <a id="editcodes" href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a>
        <div id="logo_content">
        </div>
    </div>
    <div id="main">
        <div class="columnDiv">
            <ul id="Wipes" class="line">
                <li class="columnHeader"><b></b></li>
                <li class="" id="Wipes"><a href="Wipes.aspx" target="_blank">
                    <b>Wipes</b><br />
                </a></li>
            </ul>
        </div>
        <div class="columnDiv">
            <ul id="Burst_Pouch" class="line">
                <li class="columnHeader"><b></b></li>
                <li class="" id="Burst_Pouch"><a href="BurstPouch.aspx" target="_blank"><b>Burst Pouch</b><br />
                </a></li>
            </ul>
        </div>
        <div class="columnDiv">
            <ul id="Oral_Care" class="line">
                <li class="columnHeader"><b></b></li>
                <li class="" id="Oral_Care"><a href="Oral_Care.aspx" target="_blank"><b>Oral Care</b><br />
                </a></li>
        </div>
        <div class="columnDiv">
            <ul id="Needle_Punch" class="line">
                <li class="columnHeader"><b></b></li>
                <li class="" id="BP_1"><a href="NeedlePunch.aspx" target="_blank"><b>Needle Punch</b><br />
                </a></li>
            </ul>
        </div>
        <div id="clearit">
        </div>
        <a id="bythrive" href="/">Dashboard Design
            by Thrive MES
            <img src="images/thriveLogo.png"
                alt="Dashboard Design by Thrive" /></a>
    </div>
</body>
</html>
