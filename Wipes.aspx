<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Wipes.aspx.cs" Inherits="DowntimeCollection_Demo.Wipes" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Wipes</title>
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

        #WIPES {
            width: inherit !important;   
        }

        #WIPES > li:not(:first-child) {
            background: none;
            background-color: #fff; 
            width: 200px !important;
            float: left !important;
            background-color: transparent;
        }

        #WIPES > .columnHeader {
            width: 100% !important;
        }
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
        <div id="logo_content">
        </div>
    </div>
    <div id="main">
        <div class="columnDiv">
            <ul id="WIPES" style="list-style-type: none; width: 183px;">
                <li class="columnHeader"><b>Wipes</b></li>
                <li>
                    <ul>
                        <li class="green_border" id="Comfort_Bath_1A"><a href="DCSDemo.aspx?line=Comfort_Bath_1A"
                            target="_blank"><b>Comfort Bath 1A</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_1A" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_1A"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_1A" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                        <li class="green_border" id="Comfort_Bath_1B"><a href="DCSDemo.aspx?line=Comfort_Bath_1B"
                            target="_blank"><b>Comfort Bath 1B</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_1B" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_1B"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_1B" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                        <li class="green_border" id="Comfort_Bath_3"><a href="DCSDemo.aspx?line=Comfort_Bath_3"
                            target="_blank"><b>Comfort Bath 3</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_3" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_3"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_3" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                    </ul>
                </li>
                <li>
                    <ul>
                        <li class="green_border" id="Comfort_Bath_4"><a href="DCSDemo.aspx?line=Comfort_Bath_4"
                            target="_blank"><b>Comfort Bath 4</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_4" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_4"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_4" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                        <li class="green_border" id="Comfort_Bath_5"><a href="DCSDemo.aspx?line=Comfort_Bath_5"
                            target="_blank"><b>Comfort Bath 5</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_5" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_5"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_5" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                        <li class="green_border" id="Comfort_Bath_6"><a href="DCSDemo.aspx?line=Comfort_Bath_6"
                            target="_blank"><b>Comfort Bath 6</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_6" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_6"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_6" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                    </ul>
                </li>
                <li>
                    <ul>
                        <li class="green_border" id="Comfort_Bath_7"><a href="DCSDemo.aspx?line=Comfort_Bath_7"
                            target="_blank"><b>Comfort Bath 7</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_7" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_7"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_7" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                        <li class="green_border" id="Comfort_Bath_8"><a href="DCSDemo.aspx?line=Comfort_Bath_8"
                            target="_blank"><b>Comfort Bath 8</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_8" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_8"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_8" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                        <li class="green_border" id="Comfort_Bath_9"><a href="DCSDemo.aspx?line=Comfort_Bath_9"
                            target="_blank"><b>Comfort Bath 9</b><br />
                        </a>
                            <div class="percent">
                                --%</div>
                            <div class="time">
                                --:--:--</div>
                            <div class="buttons">
                                <a href="DCSDemo.aspx?line=Comfort_Bath_9" target="_blank">OI</a> <a href="http://dashboard.thrivemes.com/downtime/snapshot?line=Comfort_Bath_9"
                                    target="_blank">DB</a> <a href="DCSDemoReasonCodes.aspx?line=Comfort_Bath_9" target="_blank">
                                        RC</a>
                            </div>
                        </li>
                    </ul>
                </li>
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
