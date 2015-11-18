<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SkFood.aspx.cs" Inherits="SkFood" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>SK Foods Dashboard</title>
<link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<link href='http://fonts.googleapis.com/css?family=Fjalla+One' rel='stylesheet' type='text/css'>
<link href="SkFood.css" rel="stylesheet" type="text/css" />
<link rel="icon" href="images/SKFavicon.png" type="image/png">
<script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<script src="Scripts/global.js" type="text/javascript"></script>
<script type="text/javascript">
secondInterval = 5; // feel free to adjust
    seconds = secondInterval - 1; // do not modify
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
	seconds++; if (seconds % secondInterval == 0)
        $.ajax({
            url: 'http://api.thrivemes.com/api/LineStatus/SKFoods',
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
		else $(".time").each(function() { var times = $(this).text().split(":"); times[2]++; if (times[2] >= 60) { times[2] = 0; times[1]++; if (times[1] >= 60) { times[1] = 0; times[0]++; if (times[0] < 10) times[0] = "0" + times[0]; } if (times[1] < 10) times[1] = "0" + times[1]; } if (times[2] < 10) times[2] = "0" + times[2]; if (!isNaN(times[0]) && !isNaN(times[1]) && !isNaN(times[2])) $(this).text(times.join(":")); });
    }
</script>
</head>
<body>
	<div id="header">
        <!--<div><a href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a></div>-->
    	<div id="logo_content">
        </div>
    </div>
    <div id="main">
        <div class="columnDiv">
            <ul id="casting" class="line">
                <li class="red_border" id="Line_01"><a href="DCSDemo.aspx?line=Line_01">Line 1</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_01" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Line_01" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_01" target="_blank">RC</a>
					</div>
                </li>
                <li class="red_border" id="Line_02"><a href="DCSDemo.aspx?line=Line_02">Line 2</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_02" target="_blank">OI</a>
						<a href=http://dashboard.thrivemes.com/downtime/snapshot?line=Line_02" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_02" target="_blank">RC</a>
					</div>
                </li>
                <li class="red_border" id="Line_03"><a href="DCSDemo.aspx?line=Line_03">Line 3</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_03" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_03" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_03" target="_blank">RC</a>
					</div>
                </li>
                <li class="red_border" id="Line_04"><a href="DCSDemo.aspx?line=Line_04">Line 4</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_04" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_04" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_04" target="_blank">RC</a>
					</div>
                </li>

             </ul>
          </div>
          <div class="columnDiv">
            <ul id="finishing" class="line">
                <li class="red_border" id="Line_05"><a href="DCSDemo.aspx?line=Line_05">Line 5</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_05" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_05" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_05" target="_blank">RC</a>
					</div>
                </li>
                <li class="red_border" id="Line_06"><a href="DCSDemo.aspx?line=Line_06">Line 6</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_06" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_06" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_06" target="_blank">RC</a>
					</div>
                </li>
                <li class="red_border" id="Line_07"><a href="DCSDemo.aspx?line=Line_07">Line 7</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_07" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_07" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_07" target="_blank">RC</a>
					</div>
                </li>
                <li class="red_border" id="Line_08"><a href="DCSDemo.aspx?line=Line_08">Line 8</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_08" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_08" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_08" target="_blank">RC</a>
					</div>
                </li>
            </ul>
           </div>
           <div class="columnDiv">
                <ul id="other" class="line">
                    <li class="red_border" id="Line_09"><a href="DCSDemo.aspx?line=Line_09">Line 9</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
						<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_09" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_09" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_09" target="_blank">RC</a>
					</div>
                    </li>
                    <li class="red_border" id="Line_10"><a href="DCSDemo.aspx?line=Line_010">Line 10</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
						<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_10" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_10" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_10" target="_blank">RC</a>
					</div>
                    </li>
                    <li class="red_border" id="Line_11"><a href="DCSDemo.aspx?line=Line_11">Line 11</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
						<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_11" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_11" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_11" target="_blank">RC</a>
					</div>
                    </li>
                    <li class="red_border" id="Line_12"><a href="DCSDemo.aspx?line=Line_12">Line 12</a><br />
                        <div class="percent">--%</div>  <div class="time">--:--:--</div>
						<div class="buttons">
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Line_12" target="_blank">OI</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Line_12" target="_blank">DB</a>
						<a href="http://legacy.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Line_12" target="_blank">RC</a>
					</div>
                    </li>
                </ul>
           </div>
		   <div id="thrive">
			<a target="_blank" href="/"><img src="images/thrive.png" alt="Thrive DCS" /></a>
		   </div>
        </div>
</body>
</html>