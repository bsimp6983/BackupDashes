<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SageQAIndex.aspx.cs" Inherits="DowntimeCollection_Demo.SageQAIndex" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Sage QA Index Dashboard</title>
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
            url: 'GetLinePercent.ashx?client=SageQA',
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
            url: 'GetLineStatus.ashx?client=SageQA',
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
				<li class="columnHeader"><b> </b></li>
				<li class="green_border" id="TOC_A_ML0305"><a href="DCSDemo.aspx?line=TOC_A_ML0305" target="_blank">
					<b>TOC A ML0305</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=TOC_A_ML0305" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=TOC_A_ML0305" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=TOC_A_ML0305" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="TOC_B_ML0383"><a href="DCSDemo.aspx?line=TOC_B_ML0383" target="_blank">
					<b>TOC B ML0383</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=TOC_B_ML0383" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=TOC_B_ML0383" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=TOC_B_ML0383" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Vitek_2_ML0361"><a href="DCSDemo.aspx?line=Vitek_2_ML0361" target="_blank">
					<b>Vitek 2 ML0361</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Vitek_2_ML0361" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Vitek_2_ML0361" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Vitek_2_ML0361" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Thermal_Cycler_ML0370"><a href="DCSDemo.aspx?line=Thermal_Cycler_ML0370" target="_blank">
					<b>Thermal Cycler ML0370</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Thermal_Cycler_ML0370" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Thermal_Cycler_ML0370" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Thermal_Cycler_ML0370" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Agilent_ML0369"><a href="DCSDemo.aspx?line=Agilent_ML0369" target="_blank">
					<b>Agilent ML0369</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Agilent_ML0369" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Agilent_ML0369" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Agilent_ML0369" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="SWABS" class="line">
				<li class="columnHeader"><b> </b></li>
				<li class="green_border" id="Celsis_A_ML0224"><a href="DCSDemo.aspx?line=Celsis_A_ML0224" target="_blank">
					<b>Celsis A ML0224</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Celsis_A_ML0224" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Celsis_A_ML0224" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Celsis_A_ML0224" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Celsis_B_ML0223"><a href="DCSDemo.aspx?line=Celsis_B_ML0223" target="_blank">
					<b>Celsis B ML0223</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Celsis_B_ML0223" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Celsis_B_ML0223" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Celsis_B_ML0223" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Autoclave_ML0141"><a href="DCSDemo.aspx?line=Autoclave_ML0141" target="_blank">
					<b>Autoclave ML0141</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Autoclave_ML0141" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Autoclave_ML0141" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Autoclave_ML0141" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="HPLC_1_ML0187"><a href="DCSDemo.aspx?line=HPLC_1_ML0187" target="_blank">
					<b>HPLC 1 ML0187</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=HPLC_1_ML0187" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=HPLC_1_ML0187" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=HPLC_1_ML0187" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="HPLC_2_ML0188"><a href="DCSDemo.aspx?line=HPLC_2_ML0188" target="_blank">
					<b>HPLC 2 ML0188</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=HPLC_2_ML0188" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=HPLC_2_ML0188" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=HPLC_2_ML0188" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="HPLC_3_ML0189"><a href="DCSDemo.aspx?line=HPLC_3_ML0189" target="_blank">
					<b>HPLC 3 ML0189</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=HPLC_3_ML0189" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=HPLC_3_ML0189" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=HPLC_3_ML0189" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="FORM_FILL_SEAL" class="line">
				<li class="columnHeader"><b> </b></li>
				<li class="green_border" id="HPLC_4_ML0190"><a href="DCSDemo.aspx?line=HPLC_4_ML0190" target="_blank">
					<b>HPLC 4 ML0190</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=HPLC_4_ML0190" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=HPLC_4_ML0190" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=HPLC_4_ML0190" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="HPLC_5_ML0191"><a href="DCSDemo.aspx?line=HPLC_5_ML0191" target="_blank">
					<b>HPLC 5 ML0191</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=HPLC_5_ML0191" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=HPLC_5_ML0191" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=HPLC_5_ML0191" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="HPLC_6_ML0271"><a href="DCSDemo.aspx?line=HPLC_6_ML0271" target="_blank">
					<b>HPLC 6 ML0271</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=HPLC_6_ML0271" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=HPLC_6_ML0271" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=HPLC_6_ML0271" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="FTIR_ML0375"><a href="DCSDemo.aspx?line=FTIR_ML0375" target="_blank">
					<b>FTIR ML0375</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=FTIR_ML0375" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=FTIR_ML0375" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=FTIR_ML0375" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
	<div id="clearit"> </div>
	<a id="bythrive" href="/">Dashboard Design by Thrive MES <img src="images/thriveLogo.png" alt="Dashboard Design by Thrive" /></a>
	</div>
</body>
</html>
