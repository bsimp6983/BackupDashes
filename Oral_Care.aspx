

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Oral Care</title>
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
.columnDiv{margin:0px auto;width:1000px;}
</style>

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
seconds++; if (seconds % secondInterval == 0)
        $.ajax({
            url: 'http://api.thrivemes.com/api/LineStatus/Sage',
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
	<div id="header">
        <a id="performanceChart" href="PerformanceChart.aspx">Timeline / Performance Chart</a>
		<a id="editcodes" href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a>
		<div id="logo_content"> </div>
	</div>
	<div id="main">
		<div class="columnDiv">
			<ul id="POWDER_ROOM" class="line">
				<li class="columnHeader"><b>Powder Room</b></li>
				<li class="green_border" id="TA_1"><a href="DCSDemo.aspx?line=TA_1" target="_blank">
					<b>TA 1</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=TA_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=TA_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=TA_1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="TA_2"><a href="DCSDemo.aspx?line=TA_2" target="_blank">
					<b>TA 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=TA_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=TA_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=TA_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="TA_3"><a href="DCSDemo.aspx?line=TA_3" target="_blank">
					<b>TA_3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=TA_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=TA_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=TA_3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="DB_1"><a href="DCSDemo.aspx?line=DB_1" target="_blank">
					<b>DB 1</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=DB_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=DB_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=DB_1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="DB_2"><a href="DCSDemo.aspx?line=DB_2" target="_blank">
					<b>DB 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=DB_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=DB_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=DB_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="DB_3"><a href="DCSDemo.aspx?line=DB_3" target="_blank">
					<b>DB 3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=DB_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=DB_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=DB_3" target="_blank">RC</a>
					</div>
				</li>
                <li class="green_border" id="SPRINT"><a href="DCSDemo.aspx?line=SPRINT" target="_blank">
					<b>SPRINT</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=SPRINT" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=SPRINT" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=SPRINT" target="_blank">RC</a>
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
			<ul id="SUCTION" class="line">
				<li class="columnHeader"><b>Suction</b></li>
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
                <li class="green_border" id="SOB_2"><a href="DCSDemo.aspx?line=Handle" target="_blank">
					<b>SOB 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=SOB_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=SOB_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=SOB_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="MC_4"><a href="DCSDemo.aspx?line=MC_4" target="_blank">
					<b>MC 4</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=MC_4" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=MC_4" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=MC_4" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="MOUTH_CARE" class="line">
				<li class="columnHeader"><b>Mouth Care</b></li>
				<li class="green_border" id="MC_3"><a href="DCSDemo.aspx?line=MC_3" target="_blank">
					<b>MC 3</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=MC_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=MC_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=MC_3" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="DB_4"><a href="DCSDemo.aspx?line=DB_4" target="_blank">
					<b>DB 4</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=DB_4" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=DB_4" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=DB_4" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="DB_5"><a href="DCSDemo.aspx?line=DB_5" target="_blank">
					<b>DB 5</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=DB_5" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=DB_5" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=DB_5" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
        <div class="columnDiv">
			<ul id="DEVICE" class="line">
				<li class="columnHeader"><b>Device</b></li>
				<li class="green_border" id="BRISTLER_1"><a href="DCSDemo.aspx?line=BRISTLER_1" target="_blank">
					<b>BRISTLER 1</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BRISTLER_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BRISTLER_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BRISTLER_1" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="BRISTLER_2"><a href="DCSDemo.aspx?line=BRISTLER_2" target="_blank">
					<b>BRISTLER 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=BRISTLER_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=BRISTLER_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=BRISTLER_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="green_border" id="Yank_2"><a href="DCSDemo.aspx?line=Yank_2" target="_blank">
					<b>YANK 2</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Yank_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Yank_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Yank_2" target="_blank">RC</a>
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
                <li class="green_border" id="CATH_1"><a href="DCSDemo.aspx?line=CATH_1" target="_blank">
					<b>CATH 1</b><br /></a>
					<div class="percent">--%</div>  <div class="time">--:--:--</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=CATH_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=CATH_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=CATH_1" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
	<div id="clearit"> </div>
	<a id="bythrive" href="/">Dashboard Design by Thrive MES <img src="images/thriveLogo.png" alt="Dashboard Design by Thrive" /></a>
	</div>
</body>
</html>
