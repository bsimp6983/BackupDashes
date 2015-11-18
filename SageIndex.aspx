<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SageIndex.aspx.cs" Inherits="DowntimeCollection_Demo.WebForm1" %>

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
    position:absolute;top:111px;right:54%;z-index:999;margin-left:52px;color:#0697C3;font-size:11px;
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
            url: 'GetLinePercent.ashx?client=Sage',
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
            url: 'GetLineStatus.ashx?client=Sage',
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
        <a id="performanceChart" href="PerformanceChart.aspx">Timeline / Performance Chart</a>
		<a id="editcodes" href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a>
		<div id="logo_content"> </div>
	</div>
	<div id="main">
		<div class="columnDiv">
			<ul id="wipes" class="line">
				<li class="columnHeader"><b>WIPES</b></li>
				<li class="green_border" id="Comfort_Bath_1A"><a href="DCSDemo.aspx?line=Comfort_Bath_1A" target="_blank">
					<b>COMFORT BATH 1A</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Comfort_Bath_1A" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Comfort_Bath_1A" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_1A" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Comfort_Bath_1B"><a href="DCSDemo.aspx?line=Comfort_Bath_1B" target="_blank">
					<b>COMFORT BATH 1B</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Comfort_Bath_1B" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Comfort_Bath_1B" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_1B" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Comfort_Bath_3"><a href="DCSDemo.aspx?line=Comfort_Bath_3" target="_blank">
					<b>COMFORT BATH 3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Comfort_Bath_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Comfort_Bath_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Comfort_Bath_4"><a href="DCSDemo.aspx?line=Comfort_Bath_4" target="_blank">
					<b>COMFORT BATH 4</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Comfort_Bath_4" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Comfort_Bath_4" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_4" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Comfort_Bath_5"><a href="DCSDemo.aspx?line=Comfort_Bath_5" target="_blank">
					<b>COMFORT BATH 5</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Comfort_Bath_5" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Comfort_Bath_5" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_5" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Comfort_Bath_6"><a href="DCSDemo.aspx?line=Comfort_Bath_6" target="_blank">
					<b>COMFORT BATH 6</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Comfort_Bath_6" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Comfort_Bath_6" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_6" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="SWABS" class="line">
				<li class="columnHeader"><b>SWABS</b></li>
				<li class="green_border" id="SOS_1"><a href="DCSDemo.aspx?line=SOS_1" target="_blank">
					<b>SOS 1</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=SOS_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=SOS_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=SOS_1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="SOS_2"><a href="DCSDemo.aspx?line=SOS_2" target="_blank">
					<b>SOS 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=SOS_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=SOS_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=SOS_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="SOS_3"><a href="DCSDemo.aspx?line=SOS_3" target="_blank">
					<b>SOS 3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=SOS_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=SOS_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=SOS_3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="SOS_4"><a href="DCSDemo.aspx?line=SOS_4" target="_blank">
					<b>SOS 4</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=SOS_4" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=SOS_4" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=SOS_4" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="MC_3"><a href="DCSDemo.aspx?line=MC_3" target="_blank">
					<b>MC 3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=MC_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=MC_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=MC_3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Handle"><a href="DCSDemo.aspx?line=Handle" target="_blank">
					<b>HANDLE</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Handle" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Handle" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Handle" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="FORM_FILL_SEAL" class="line">
				<li class="columnHeader"><b>FORM FILL SEAL</b></li>
				<li class="green_border" id="FFS_1"><a href="DCSDemo.aspx?line=FFS_1" target="_blank">
					<b>FFS 1</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=FFS_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=FFS_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=FFS_1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="FFS_2"><a href="DCSDemo.aspx?line=FFS_2" target="_blank">
					<b>FFS 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=FFS_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=FFS_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=FFS_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="FFS_3"><a href="DCSDemo.aspx?line=FFS_3" target="_blank">
					<b>FFS 3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=FFS_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=FFS_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=FFS_3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="FFS_4"><a href="DCSDemo.aspx?line=FFS_4" target="_blank">
					<b>FFS 4</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=FFS_4" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=FFS_4" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=FFS_4" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="FFS_5"><a href="DCSDemo.aspx?line=FFS_5" target="_blank">
					<b>FFS 5</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=FFS_5" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=FFS_5" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=FFS_5" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="BURST_POUCH" class="line">
				<li class="columnHeader"><b>BURST POUCH</b></li>
				<li class="green_border" id="BP_1"><a href="DCSDemo.aspx?line=BP_1" target="_blank">
					<b>BP 1</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BP_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BP_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BP_1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="BP_2"><a href="DCSDemo.aspx?line=BP_2" target="_blank">
					<b>BP 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BP_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BP_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BP_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="BP_3"><a href="DCSDemo.aspx?line=BP_3" target="_blank">
					<b>BP 3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BP_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BP_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BP_3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="BP_4"><a href="DCSDemo.aspx?line=BP_4" target="_blank">
					<b>BP 4</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BP_4" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BP_4" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BP_4" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="BP_5"><a href="DCSDemo.aspx?line=BP_5" target="_blank">
					<b>BP 5</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BP_5" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BP_5" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BP_5" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="BP_6"><a href="DCSDemo.aspx?line=BP_6" target="_blank">
					<b>BP 6</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BP_6" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BP_6" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BP_6" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
	<div id="clearit"> </div>
	<a id="bythrive" href="/">Dashboard Design by Thrive MES <img src="images/thriveLogo.png" alt="Dashboard Design by Thrive" /></a>
	</div>
</body>
</html>
