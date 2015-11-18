<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SkFood.aspx.cs" Inherits="SkFood" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>SK Foods Dashboard</title>
<link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<script src="Scripts/global.js" type="text/javascript"></script>

<style type="text/css">
    body
    {
        /*background-image:url('images/app-main.png');*/
    }
    #header
    {
        height: 120px;
    }
    
    #main
    {
        text-align:center; 
        margin-left:auto;
        margin-right:auto;
        width:70%;
    }
    
    .columnDiv 
    {
        margin: 0px auto;
        width: 860px;
    }
    .columnDiv ul
    {
        list-style: none;
        margin: 1em 0;
        padding: 0;
    }
    
    .columnDiv ul li
    {
        font-weight: bold;
        margin: 10px;
        padding: 3px 10px 5px 10px;
        color: #666;
        text-align: center;
        border-radius: 10px;
    }
    
    .columnDiv ul li:hover
    {
        color: #000;
        background-color: #ddd;
    }
    
    .columnDiv a
    {
        text-decoration: none;
        color: inherit;
    }
    
    .columnDiv .line
    {
        float: left;
        width: 210px;
    }
    div .percent
    {
        display:inline;
    }
    div .time
    {
        display:inline;
    }
    .green_border
    {
        border: 6px solid #00ff00;
    }
    .red_border
    {
        border: 6px solid #ff0000;
    }

</style>
<script type="text/javascript">
    $(document).ready(function () {
        getStatus();
        getPercent();
        setInterval(getStatus, 1000);
        setInterval(getPercent, 300000);
    });
    function getPercent() {
        $.ajax({
            url: 'GetLinePercent.ashx?client=SKFoods',
            type: 'GET',
            dataType: 'json',
            data: { time: new Date().format('yyyy/MM/dd HH:mm:ss') },
            success: function (data) {
                $.each(data, function () {
                    $('li#' + this.line).find(".percent").text(this.percent);
                });
            },
            error: function (xhr, textStatus, errorThrown) {
                $('div.percent').text('--%');
            }
        });
    }
    function getStatus() {
        $.ajax({
            url: 'GetLineStatus.ashx?client=SKFoods',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $.each(data, function () {
                    $('li#' + this.line).find(".time").text(this.time);
                    if (this.status == true) {
                        $('li#' + this.line).attr('class', 'green_border');
                    }
                    else {
                        $('li#' + this.line).attr('class', 'red_border');
                    };
                });
            },
            error: function (xhr, textStatus, errorThrown) {
                $('li.green_border').attr('class', 'red_border');
                $('div.time').text(textStatus);
            }
        });
    }
</script>
</head>
<body>
	<div id="header">
        <div><a href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a></div>
    	<div id="logo_content">
        </div>
    </div>
    
    <div id="main">
        <div class="columnDiv">
            <ul id="casting" class="line">
                <li class="red_border" id="Line_1"><a href="DCSDemo.aspx?line=Line_1">Line 1</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Line_2"><a href="DCSDemo.aspx?line=Line_2">Line 2</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Line_3"><a href="DCSDemo.aspx?line=Line_3">Line 3</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Line_4"><a href="DCSDemo.aspx?line=Line_4">Line 4</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>

             </ul>
          </div>
          <div class="columnDiv">
            <ul id="finishing" class="line">
                <li class="red_border" id="Line_5"><a href="DCSDemo.aspx?line=Line_5">Line_5</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Line_6"><a href="DCSDemo.aspx?line=Line_6">Line 6</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Line_7"><a href="DCSDemo.aspx?line=Line_7">Line 7</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Line_8"><a href="DCSDemo.aspx?line=Line_8">Line 8</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
            </ul>
           </div>
           <div class="columnDiv">
                <ul id="other" class="line">
                    <li class="red_border" id="Line_9"><a href="DCSDemo.aspx?line=Line_9">Line 9</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
                    </li>
                    <li class="red_border" id="Line_10"><a href="DCSDemo.aspx?line=Line_10">Line 10</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
                    </li>
                    <li class="red_border" id="Line_11"><a href="DCSDemo.aspx?line=Line_11">Line 11</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
                    </li>
                    <li class="red_border" id="Line_12"><a href="DCSDemo.aspx?line=Line_12">Line 12</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
                    </li>
                </ul>
           </div>
        </div>
   
    
</body>
</html>