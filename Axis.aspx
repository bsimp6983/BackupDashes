<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Axis.aspx.cs" Inherits="Default2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Axis Dashboard</title>
<link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<link href='http://fonts.googleapis.com/css?family=Fjalla+One' rel='stylesheet' type='text/css'>
<link href="axis.css" rel="stylesheet" type="text/css" />
<link rel="icon" href="images/CHGFavicon.png" type="image/png">
<script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<script src="Scripts/global.js" type="text/javascript"></script>
<script type="text/javascript">
    $(document).ready(function () {
        getStatus();
        getPercent();
        setInterval(getStatus, 1000);
        setInterval(getPercent, 300000);
    });
    function getPercent() {
        $.ajax({
            url: 'GetLinePercent.ashx?client=griffinpipe',
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
            url: 'GetLineStatus.ashx?client=griffinpipe',
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
				<li class="columnHeader"><strong>Casting</strong></li>
				<li class="green_border" id="A_Machine"><a href="DCSDemo.aspx?line=A_Machine">A Machine</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=A_Machine" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=A_Machine" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=A_Machine" target="_blank">RC</a>
					</div>
				</li>
				<li class="red_border" id="B_Machine"><a href="DCSDemo.aspx?line=B_Machine">B Machine</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=B_Machine" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=B_Machine" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=B_Machine" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="C_Machine"><a href="DCSDemo.aspx?line=C_Machine">C Machine</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=C_Machine" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=C_Machine" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=C_Machine" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="D_Machine"><a href="DCSDemo.aspx?line=D_Machine">D Machine</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=D_Machine" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=D_Machine" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=D_Machine" target="_blank">RC</a>
					</div>
				</li>
			 </ul>
			<ul id="finishing" class="line">
				<li class="columnHeader"><strong>Finishing</strong></li>
				<li class="green_border" id="SD_Test_Press"><a href="DCSDemo.aspx?line=SD_Test_Press">SD Test Press</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=SD_Test_Press" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=SD_Test_Press" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=SD_Test_Press" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="SD_Cement_Lining"><a href="DCSDemo.aspx?line=SD_Cement_Lining">SD Cement Lining</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=SD_Cement_Lining" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=SD_Cement_Lining" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=SD_Cement_Lining" target="_blank">RC</a>
					</div>
				</li>
				<li class="red_border" id="LD_Test_Press"><a href="DCSDemo.aspx?line=LD_Test_Press">LD Test Press</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=LD_Test_Press" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=LD_Test_Press" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=LD_Test_Press" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="LD_Cement_Lining"><a href="DCSDemo.aspx?line=LD_Cement_Lining">LD Cement Lining</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=LD_Cement_Lining" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=LD_Cement_Lining" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=LD_Cement_Lining" target="_blank">RC</a>
					</div>
				</li>
			</ul>
			<ul id="other" class="line">
				<li class="columnHeader"><strong>Other</strong></li>
				<li class="green_border" id="Cupola"><a href="DCSDemo.aspx?line=Cupola">Cupola</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=Cupola" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=Cupola" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=Cupola" target="_blank">RC</a>
					</div>
				</li>
				<li  class="green_border" id="7CoreMachine"><a href="DCSDemo.aspx?line=7CoreMachine">#7 Core Machine</a><br />
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemo.aspx?line=7CoreMachine" target="_blank">OI</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDashboard.aspx?line=7CoreMachine" target="_blank">DB</a>
						<a href="http://www.downtimecollectionsolutions.com/app/demo/DCSDemoReasonCodes.aspx?line=7CoreMachine" target="_blank">RC</a>
					</div>
				</li>
			</ul>
	   </div>
	</div>
</body>
</html>