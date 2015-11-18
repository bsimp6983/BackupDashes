﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CHGIndex.aspx.cs" Inherits="DCSDemoData.CHGIndex" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8; Cache-Control: no-cache" />
<title>CHG Dashboard</title>
<link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<link href="chg.css" rel="stylesheet" type="text/css" />
<link rel="icon" href="images/CHGFavicon.png" type="image/png">
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
            url: 'GetLinePercent.ashx?client=chg',
            type: 'GET',
            dataType: 'json',
            data: {time: new Date().format('yyyy/MM/dd HH:mm:ss') },
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
            url: 'http://api.thrivemes.com/api/LineStatus/chg',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $.each(data, function () {
                    $('li#' + this.line).find(".time").text(this.time);
                    if (this.status == false) {
                        $('li#' + this.line).attr('class', 'red_border');
                    }
                    else {
                        $('li#' + this.line).attr('class', 'green_border');
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
    <div id="main">
		<div class="columnDiv">
			<ul id="casting" class="line">
				<li class="green_border" id="Robag1"><a href="DCSDemo.aspx?line=Robag1">Robag1</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=Robag1" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Robag1" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=Robag1" target="_blank">RC</a>
					</div>
				</li>
				<li class="red_border" id="Haysen2"><a href="DCSDemo.aspx?line=Haysen2">Haysen2</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=Haysen2" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Haysen2" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=Haysen2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Robag3"><a href="DCSDemo.aspx?line=Robag3">Robag3</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=Robag3" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Robag3" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=Robag3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Robag4"><a href="DCSDemo.aspx?line=Robag4">Robag4</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=Robag4" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Robag4" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=Robag4" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		
		<div id="line2" class="columnDiv">
			
			<ul id="casting" class="line">
				<li class="green_border" id="4P1"><a href="DCSDemo.aspx?line=4P1">4P1</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=4P1" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=4P1" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=4P1" target="_blank">RC</a>
					</div>
				</li>
				<li class="red_border" id="4P3"><a href="DCSDemo.aspx?line=4P3">4P3</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=4P3" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=4P3" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=4P3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="4I1"><a href="DCSDemo.aspx?line=4I1">4I1</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=4I1" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=4I1" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=4I1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="4I2"><a href="DCSDemo.aspx?line=4I2">4I2</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=4I2" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=4I2" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=4I2" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		
	<div id="line3" class="columnDiv">
			<a id="reason" href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a>
			<ul id="casting" class="line">
				<li class="green_border" id="4A1"><a href="DCSDemo.aspx?line=4A1">4A1</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=4A1" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=4A1" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=4A1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Bib1"><a href="DCSDemo.aspx?line=Bib1">Bib1</a><br />
					<div class="bottom"><div class="percent">--%</div>  <div class="time">--:--:--</div></div>
					<div class="buttons">
						<a href="./DCSDemo.aspx?line=Bib1" target="_blank">OI</a>
						<a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Bib1" target="_blank">DB</a>
						<a href="./DCSDemoReasonCodes.aspx?line=Bib1" target="_blank">RC</a>
					</div>
				
				</li>
			</ul>
		</div>	
	</div>
</body>
</html>