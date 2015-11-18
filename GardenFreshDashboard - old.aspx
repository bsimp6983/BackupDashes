<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GardenFreshDashboard.aspx.cs" Inherits="DCSDemoData.GardenFreshDashboard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Garden Fresh Dashboard</title>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>

<style>
    
    #header
    {
        height: 120px;
    }
    
    #main
    {
    }
    
    #lineDiv 
    {
        margin: 0px auto;
        width: 860px;
    }
    
    #lineDiv ul
    {
        list-style: none;
        margin: 1em 0;
        padding: 0;
    }
    
    #lineDiv ul li
    {
        font-weight: bold;
        margin: 0;
        padding: 3px 10px 5px 10px;
        color: #666;
        text-align: center;
    }
    
    #lineDiv ul li:hover
    {
        color: #000;
        background-color: #ddd;
    }
    
    #lineDiv a
    {
        text-decoration: none;
        color: inherit;
    }
    
    #lineDiv .line
    {
        float: left;
        width: 170px;
    }
    

</style>

</head>
<body>
	<div id="header">
        <div><a href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a></div>
    	<div id="logo_content">
        </div>
    </div>
    
    <div id="main">
        <div id="lineDiv">
            <ul id="salsaLine" class="line">
                <li style="color: Black;"><b>Salsa</b></li>
                <li><a href="DCSDemo.aspx?line=SalsaLine2">Line 2</a></li>
                <li><a href="DCSDemo.aspx?line=SalsaLine3">Line 3</a></li>
                <li><a href="DCSDemo.aspx?line=SalsaCostcoD">Costco D</a></li>
                <li><a href="DCSDemo.aspx?line=SalsaCostcoE">Costco E</a></li>
            </ul>
            <ul id="hummusLine" class="line">
                <li style="color: Black;"><b>Hummus</b></li>
                <li><a href="DCSDemo.aspx?line=HummusPackline">Packline</a></li>
                <li><a href="DCSDemo.aspx?line=HummusArda">Arda</a></li>
                <li><a href="DCSDemo.aspx?line=Hummus24oz">2oz/4oz</a></li>
                <li><a href="DCSDemo.aspx?line=HighSpeed">High Speed</a></li>
            </ul>
            <ul id="hppLine" class="line">
                <li style="color: Black;"><b>HPP</b></li>
                <li><a href="DCSDemo.aspx?line=HppOne">One</a></li>
                <li><a href="DCSDemo.aspx?line=HppTwo">Two</a></li>
            </ul>
            <ul id="dipsLine" class="line">
                <li style="color: Black;"><b>Dips</b></li>
                <li><a href="DCSDemo.aspx?line=DipsArda">Arda</a></li>
                <li><a href="DCSDemo.aspx?line=DipsFlange">Flange</a></li>
            </ul>
            <ul id="muckyDuckLine" class="line">
                <li style="color: Black;"><b>Mucky Duck</b></li>
                <li><a href="DCSDemo.aspx?line=MuckyDuckLine1">Line 1</a></li>
            </ul>
        </div>
    </div>
    
</body>
</html>