<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DCSDashboard.aspx.cs" Inherits="DCSDemoData.DCSDashBoard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>DCS Dashboard</title>
    <link href="Styles/demo_table.css" rel="stylesheet" type="text/css" />
    <link href="Styles/demo_page.css" rel="stylesheet" type="text/css" />


    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <link href="http://cdnjs.cloudflare.com/ajax/libs/qtip2/2.1.1/basic/jquery.qtip.min.css" rel="Stylesheet" type="text/css" />
    <!--
    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    -->


    <!--[if lt IE 9]>
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.9.2/jquery-ui.min.js"></script>
    <![endif]-->
    <!--[if gte IE 9]><!-->
    <script src="//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.2/jquery-ui.min.js"></script>
    <!--<![endif]-->

    <script src="Scripts/Visifire.js" type="text/javascript"></script>
    <script src="DiamondCrystaldashboard.js" type="text/javascript"></script>

    <script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
    <script src="Scripts/jquery.easing.1.3.js" type="text/javascript"></script>
    <script src="Scripts/jquery.dataTables.js" type="text/javascript"></script>
    <script src="Scripts/jquery.corner.js" type="text/javascript"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-noty/2.3.5/packaged/jquery.noty.packaged.min.js" type="text/javascript"></script>

    <!--<script src="Scripts/jquery.tableSort.js" type="text/javascript"></script>-->
    <script src="Scripts/jquery.tablesorter.min.js" type="text/javascript"></script>
    <script src="scripts/floating.js" type="text/javascript"></script>
    <script src="scripts/Shared.js" type="text/javascript"></script>

    <script src="Scripts/jquery-ui-timepicker-addon.js" type="text/javascript"></script>
    <link href="Scripts/timeline.css" rel="Stylesheet" type="text/css" />
    <script src="Scripts/underscore-min.js" type="text/javascript"></script>
    <script src="Scripts/backbone-min.js" type="text/javascript"></script>
    <script src="Scripts/handlebars.js" type="text/javascript"></script>
    <script src="Scripts/Visifire.js" type="text/javascript"></script>
    <script src="Scripts/moment.min.js" type="text/javascript"></script>
    <script src="Scripts/timeline.js" type="text/javascript"></script>
    <script src="Scripts/user.js" type="text/javascript"></script>
    <script src="http://cdnjs.cloudflare.com/ajax/libs/qtip2/2.1.1/basic/jquery.qtip.min.js" type="text/javascript"></script>

    <style type="text/css">.noty_close { display: block !important; opacity: 1 !important; }</style>
    <script type="text/javascript">
        $(function() {
            if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
                noty({text: '<b>The latest version of Google Chrome no longer supports Silverlight. <a href="chrome-npapi.html">Click here for two options</a> to view your data.</b>', type: 'warning', closeWith: ['click', 'button'] });
            }
        });
    </script>

    <script src="Scripts/pfchart.js" type="text/javascript"></script>

    <style type="text/css">
        body {
            font: 10pt arial;
        }

        div.timeline-event {
            border: none;
            background: none;
            border-radius: 0;
        }

        div.timeline-event-selected {
            border: none;
            background: none;
        }

        div.timeline-event-content {
            margin: 0;
        }

        div.timeline-event-range {
            border: none;
            border-radius: 0;
            height: 100px;
            width: 100%;
            position: relative;
            overflow: visible;
        }

        div.bar {
            position: absolute;
            bottom: 0;
            left: 0;
            width: 100%;
            text-align: center;
            color: white;
            /* height and color is set for each individual bar */
        }

        div.requirement {
            position: absolute;
            bottom: 0;
            left: 0;
            width: 100%;
            border-top: 2px solid gray;
            background: #e5e5e5;
            opacity: 0.5;
        }
    </style>

    <script type="text/javascript">
        $(function () {
            <% 
    //if(Request.Cookies["first-dashboard-dailog-opened"]==null){
            %>

            getUser(function (user) {
                if (user) {
                    $("#dialog:ui-dialog").dialog("destroy");

                    if (!user.hideHelper) {
                        $("#dialog-confirm").dialog({
                            resizable: false,
                            height: "auto",
                            width: 400,
                            modal: true,
                            buttons: {
                                OK: function () {
                                    $(this).dialog("close");
                                }
                            }
                        });
                    }

                    if (user.hidePanel) {
                        $('#toprightnav').hide();
                    }

                }
            });

            <%
    //HttpCookie cookie=new HttpCookie("first-dashboard-dailog-opened","true");
    //cookie.Expires=DateTime.Now.AddYears(2);
    //Response.Cookies.Add(cookie);
    //} 
            %>
        });


    </script>
    <style type="text/css">
        body {
            background: url(images/mrp_bg.jpg) fixed center top no-repeat;
            margin: 0px;
            padding: 0px;
            overflow-x: hidden;
            overflow-y: hidden;
        }

        #header {
            width: 100%;
            background: url(images/mrp_header.png) center bottom no-repeat;
            height: 150px;
        }

            #header .container {
                width: 1025px;
                height: 100%;
                margin: 0px auto;
                padding: 0px;
            }

                #header .container img {
                    margin: 0px 0px 0px 0px;
                    border: none;
                    float: left;
                }

                #header .container .inputs {
                    width: 200px;
                    float: left;
                    margin-left: 214px;
                    margin-top: -20px;
                }



        #startdate, #enddate {
            border: none;
            width: 100px;
            height: 20px;
            text-align: right;
            padding-right: 3px;
            font-size: 15px;
            font-family: Arial;
            margin-top: 28px;
        }

        #enddate {
            margin-top: 4px;
        }

        .go {
            background: url(images/mrp_buttons.png) -1030px -33px no-repeat;
            width: 79px;
            height: 55px;
            border: none;
        }

        .day {
            width: 58px;
            height: 37px;
            background: url(images/rp_buttons.png) -2px 0px no-repeat;
            border: none;
        }

        .week {
            width: 75px;
            height: 37px;
            background: url(images/rp_buttons.png) -67px 0px no-repeat;
            border: none;
        }

        .month {
            width: 77px;
            height: 37px;
            background: url(images/rp_buttons.png) -155px 0px no-repeat;
            border: none;
        }

        .year {
            width: 67px;
            height: 37px;
            background: url(images/rp_buttons.png) -249px 0px no-repeat;
            border: none;
        }

        .setgoals {
            width: 109px;
            height: 37px;
            background: url(images/rp_buttons.png) -327px 0px no-repeat;
            border: none;
        }

        .exporttoxls {
            width: 166px;
            height: 33px;
            background: url(images/mrp_buttons.png) -444px 0px no-repeat;
            border: none;
            margin-right: 20px;
        }

        .efficiencytoxls {
            border-style: none;
            border-color: inherit;
            border-width: medium;
            width: 188px;
            height: 36px;
            background: url(images/efficiency-export.png) no-repeat;
            margin-right: 20px;
        }
         
        .downtimeexporttoxls {
            border-style: none;
            border-color: inherit;
            border-width: medium;
            width:188px;
            height:36px;
            background:url(images/downtime-export.png) no-repeat;
            margin-right:20px;
        }

        .addnew {
            width: 106px;
            height: 33px;
            background: url(images/mrp_buttons.png) -915px 0px no-repeat;
            border: none;
        }

        .edit {
            width: 61px;
            height: 37px;
            background: url(images/rp_buttons.png) -1031px 0px no-repeat;
            border: none;
        }

        .deletet {
            width: 84px;
            height: 37px;
            background: url(images/rp_buttons.png) -1098px 0px no-repeat;
            border: none;
        }

        #btnGo {
            margin-left: -72px;
            margin-top: 3px;
            float: left;
        }

        #Total-Downtime {
            font-size: 22px;
            font-weight: bold;
            margin: 41px 0px 0px 60px;
            color: Black;
            white-space: nowrap;
        }

        #fake {
            overflow: hidden;
            width: 100%;
            margin-top: 27px;
        }

        #fake-body {
            overflow: hidden;
            z-index: 1;
            width: 945px;
            margin: 0px auto 0px auto;
            background: #ffffff;
            padding: 20px;
        }


        #Top5DowntimeEvents, #HistoricalDetail_Top5DowntimeEvents, #HistoricalDetail_Top5OccuringEvents, #HistoricalDetail_DowntimeHistory, #HistoricalDetail_OccurrenceHistory {
            width: 50%;
            float: left;
        }

        #Top5OccuringEvents {
            width: 50%;
            float: left;
        }

        .report {
            z-index: 1;
        }

        #jmodal-container-content label {
            width: 60px;
            text-align: right;
            display: block;
            float: left;
            margin: 3px;
        }

        #jmodal-container-content input {
            width: 70px;
            margin: 3px;
        }

        #Goal_SET {
            float: right;
        }


        #LossBuckets, #Top5DowntimeEvents, #Top5OccuringEvents, #DowntimeActualVsGoal {
            margin-bottom: 20px;
        }

        .report {
            margin-top: 10px;
            width: 100%;
            text-align: center;
        }

        .home {
            width: 75px;
            height: 32px;
            background: url(images/mrp_buttons.png) 0px -46px no-repeat;
            display: block;
            margin-left: 70px !important;
        }

        .Top5DowntimeEvents {
            width: 235px;
            height: 32px;
            background: url(images/mrp_buttons.png) -75px -46px no-repeat;
            display: block;
        }

        .Top5OccuringEvents {
            width: 229px;
            height: 32px;
            background: url(images/mrp_buttons.png) -310px -46px no-repeat;
            display: block;
        }

        .HistoricalDetail {
            width: 171px;
            height: 32px;
            background: url(images/mrp_buttons.png) -539px -46px no-repeat;
            display: block;
        }

        .DetailsbyShift {
            width: 165px;
            height: 32px;
            background: url(images/mrp_buttons.png) -710px -46px no-repeat;
            display: block;
        }

        .Administrator {
            width: 148px;
            height: 32px;
            background: url(images/mrp_buttons.png) -875px -46px no-repeat;
            display: none;
        }

        #copyright {
            font-size: 11px;
            font-weight: bold;
            float: left;
            margin-top: 7px;
            text-indent: 70px;
            color: #413600;
            width: 100%;
        }

            #copyright span {
                float: left;
            }

        #createby, #createby:visited {
            font-size: 11px !important;
            float: right !important;
            color: #413600 !important;
            font-weight: bold;
            background: url(images/rp_createby_logo.png) right 50% no-repeat;
            padding-right: 33px;
            height: 24px;
            display: block;
            margin-top: -2px !important;
            margin-right: 130px !important;
        }

        #dock {
            line-height: normal;
            z-index: 100;
            width: 100%;
            text-align: center;
            margin: 20px auto 0px auto;
            height: 82px;
            background: url(images/mrp_footer.png) center top no-repeat;
        }

            #dock .container {
                width: 1030px;
                margin: 0px auto;
            }

            #dock a, #dock a:visited {
                float: left;
                color: #000000;
                font-size: 16px;
                font-weight: bold;
                text-decoration: none;
                margin: 10px 0px 0px;
            }

                #dock a:hover {
                    color: red;
                    text-decoration: underline;
                }

        table {
            width: 100%;
        }

            table th {
                text-align: left;
            }

        #ui-datepicker-div {
            display: none;
        }

        #DowntimeActualVsGoal, #DowntimeActualVsGoal_rows {
            width: 50%;
            float: left;
        }

        #DowntimeActualVsGoal_rows {
            height: 350px;
            overflow-y: auto;
            overflow-x: hidden;
        }

            #DowntimeActualVsGoal_rows td, #DowntimeActualVsGoal_rows th {
                background: #ffffff;
                text-align: center;
                font-size: 12px;
            }

        #TI_HiddenReasons_eventsRows, #TI_HiddenReasons_actualAs, #TI_HiddenReasons_top5occuring, #TI_HiddenReasons_top5downtime {
            width: 50%;
            float: left;
        }

        #TI_HiddenReasons td, #TI_HiddenReasons th {
            background: #ffffff;
            text-align: center;
            font-size: 12px;
        }

        #TI_HiddenReasons_eventsRows {
            height: 350px;
            overflow-y: auto;
            overflow-x: hidden;
        }

        .HiddenReasons
     {
         width:200px;
         height:36px;
         background:url(images/planned-downtime.png) no-repeat center center;
     }

        #home-events-grid th {
            color: Blue;
            cursor: pointer;
        }
    </style>
</head>
<body>
    <div id="scroll-body">
        <form id="form1" runat="server">
            <div id="header">
                <div class="container">
                    <img src="images/rp_logo.png" alt="" />
                    <div class="inputs">
                        <input type="text" class="datepicker" readonly="readonly" id="startdate" />
                        <input type="text" id="enddate" readonly="readonly" class="datepicker" />
                        <div id="Total-Downtime">
                            <label>0.00</label>
                            Minutes</div>

                        <asp:DropDownList ID="clientLines" runat="server" Style="margin: -22px 0 0 260px;">
                        </asp:DropDownList>

                    </div>
                    <input type="button" value=" " class="go" id="btnGo" />


                    <asp:Panel ID="divKPIS" runat="server">
                        <div style="float: left; width: 250px; margin-top: -10px; margin-left: 50px; font-size: 18px;">
                            <ul style="list-style: none;">
                                <li>Total Downtime:
                                    <label id="totalDowntime">--</label>
                                </li>
                                <li>MTBF:
                                    <label id="mtbf">--</label>
                                </li>
                                <li>RF:
                                    <label id="rf">--</label>
                                </li>
                                <li>UF:
                                    <label id="uf">--</label>
                                </li>
                            </ul>
                        </div>
                    </asp:Panel>

                    <!--<asp:Label ID="lblLineTitle" runat="server" style="font-size: 25px; color: #803232; float: left; margin: 95px 0 0 93px; font-weight: bolder;"></asp:Label> -->
                </div>
            </div>
            <div id="fake">
                <div id="fake-body">
                    <input id="btnUI" type="button" value="Print/View" style="float: right; margin-bottom: 10px;" />
                    <div id="TI_Home" class="TabItem">
                        <input type="button" value="Downtime" id="lossListBtn" style="float: left; margin-bottom: 10px; margin-left: 10px;" />
                        <input type="button" value="Efficiency Chart" id="efficiencyChartBtn" style="float: left; margin-bottom: 10px; margin-left: 10px;" />
                        <!--<input type="button" value="Reload" id="efPast12hours" class="eftype" ctype="past12hours" style="float:left;margin-bottom:10px;margin-left:10px;" /> -->
                        <input type="radio" value="line" name="showAs" class="eftype" id="showAs_0" checked="checked" /><label for="showAs_0" class="eftype">Line Graph</label>
                        <input type="radio" value="column" name="showAs" class="eftype" id="showAs_1" /><label for="showAs_1" class="eftype">Red Green Chart</label>
                        <label id="lblLoadingEffChart" style="display: none; font-size: 15px; font-weight: bold; margin-left: 10px;">Loading Efficiency...</label>
                        <input type="button" value="numerator/denominator" id="efNumeratorDenominator" style="float: left; margin-bottom: 10px; margin-left: 10px; display: none;" />
                        <input type="button" value="Efficiency Chart Setting" id="efficiencyChartSettingBtn" style="float: left; margin-bottom: 10px; margin-left: 10px; display: none;" />
                        <div id="LossBuckets">
                            <div class="report">
                            </div>
                        </div>
                        <div style="clear: both;"></div>
                        <div id="EfficiencyChart" style="height: 313px;">
                            <div style="margin-top: 10px; width: 242px; height: 303px; float: left; font-size: 37px; text-align: center; font-weight: bold; color: #53AE2C; background: url(images/Efficiency-r.jpg) center top no-repeat;">
                                <div style="clear: both;"></div>
                                <div style="margin-top: 20px;">
                                    <asp:Label ID="lblLine" runat="server" Style="font-size: 25px; color: #803232;"></asp:Label>
                                </div>
                                <div style="margin: 43px 0 0 5px; font-size: 18px; color: #803232;">
                                    <asp:Label ID="shiftStart" runat="server"></asp:Label>
                                </div>
                                <div style="margin-top: 0px;" id="runningEfficiency">---</div>
                                <div style="margin-top: 50px;" id="yesterdayEffiency">---</div>
                            </div>
                            <div class="report" style="width: 703px; float: left;">
                            </div>
                        </div>
                        <div style="clear: both;"></div>
                        <div id="Top5DowntimeEvents" style="display: none;">
                            <div class="report">
                            </div>
                        </div>

                        <div id="Top5OccuringEvents" style="display: none;">
                            <div class="report">
                            </div>
                        </div>

                        <div id="DowntimeActualVsGoal" style="display: none;">
                            <input type="button" class="cmd" reporttype="hours" value="Hours" />
                            <input type="button" class="cmd day" reporttype="day" value="" />
                            <input type="button" class="cmd week" reporttype="week" value="" />
                            <input type="button" class="cmd month" reporttype="month" value="" />
                            <input type="button" class="cmd year" reporttype="year" value="" />
                            <input type="button" value="" style="display: none;" class="setting setgoals" />
                            <br />
                            Switch to:<input type="radio" name="DowntimeActualVsGoal_Switchto" id="DowntimeActualVsGoal_Switchto_0" value="1" checked="checked" /><label for="DowntimeActualVsGoal_Switchto_0">Downtimes</label>
                            <input type="radio" name="DowntimeActualVsGoal_Switchto" id="DowntimeActualVsGoal_Switchto_1" value="0" /><label for="DowntimeActualVsGoal_Switchto_1">Occurences</label>

                            <div class="report">
                            </div>
                        </div>
                        <div id="DowntimeActualVsGoal_rows" style="display: none;">
                            <input type="radio" name="radEvents" id="radShowBasic" checked="checked" />
                            Basic
                <input type="radio" name="radEvents" id="radShowAdvance" />
                            Advance
                Filter by Cumulative:
                            <input type="text" id="txtEventSearch" />
                            <table cellpadding="2" cellspacing="1" bgcolor="#000000" id="home-events-grid">
                                <caption style="color: Blue; font-weight: bold;">Table Detail</caption>
                                <thead>
                                    <tr>
                                        <th width="70">Minutes</th>
                                        <th width="80">Occurrences</th>
                                        <th>ReasonCode</th>
                                        <th style="display: none;" class="advanced">Duration</th>
                                        <th style="display: none;" class="advanced">Frequency</th>
                                        <th style="display: none;" class="advanced">Average</th>
                                        <th style="display: none;" class="advanced">Cumulative</th>
                                    </tr>
                                    <thead>
                                        <tbody>
                                        </tbody>
                            </table>
                        </div>
                    </div>


                    <div id="TI_Top5DowntimeEvents" class="TabItem">
                        <div id="TI_Top5DowntimeEvents_LossBuckets">
                            <div class="report">
                            </div>
                        </div>

                        <div id="TI_Top5DowntimeEvents_Top5DowntimeEvents">
                            <div class="report">
                            </div>
                        </div>

                        <div id="TI_Top5DowntimeEvents_DowntimeActualVsGoal">
                            <input type="button" class="cmd" reporttype="hours" value="Hours" />
                            <input type="button" class="cmd day" reporttype="day" value="" />
                            <input type="button" class="cmd week" reporttype="week" value="" />
                            <input type="button" class="cmd month" reporttype="month" value="" />
                            <input type="button" class="cmd year" reporttype="year" value="" />
                            <input type="button" value="" class="setting setgoals" style="display: none;" />
                            <div class="report">
                            </div>
                        </div>

                        <div id="TI_Top5DowntimeEvents_Comments">
                            <input type="button" value="" class="xls exporttoxls" style="float: right;" />
                            <table cellpadding="3" cellspacing="0">
                                <caption></caption>
                            </table>
                        </div>
                    </div>

                    <div id="TI_Top5OccuringEvents" class="TabItem">
                        <div id="TI_Top5OccuringEvents_LossBuckets">
                            <div class="report">
                            </div>
                        </div>

                        <div id="Top5OccuringEvents_Top5OccuringEvents">
                            <div class="report">
                            </div>
                        </div>

                        <div id="Top5OccuringEvents_OccuringActualVsGoal">
                            <input type="button" class="cmd" reporttype="hours" value="Hours" />
                            <input type="button" class="cmd day" reporttype="day" value="" />
                            <input type="button" class="cmd week" reporttype="week" value="" />
                            <input type="button" class="cmd month" reporttype="month" value="" />
                            <input type="button" class="cmd year" reporttype="year" value="" />
                            <input type="button" value="" class="setting setgoals" style="display: none;" />
                            <div class="report">
                            </div>
                        </div>

                        <div id="Top5OccuringEvents_Comments">
                            <input type="button" value="" class="xls exporttoxls" style="float: right;" />
                            <table cellpadding="3" cellspacing="0">
                                <caption></caption>
                            </table>
                        </div>
                    </div>


                    <div id="TI_HistoricalDetail" class="TabItem">
                        <div id="HistoricalDetail_LossBuckets">
                            <div class="report">
                            </div>
                        </div>

                        <div id="HistoricalDetail_DowntimeActualVsGoal">
                            <input type="button" class="cmd" reporttype="hours" value="Hours" />
                            <input type="button" class="cmd day" reporttype="day" value="" />
                            <input type="button" class="cmd week" reporttype="week" value="" />
                            <input type="button" class="cmd month" reporttype="month" value="" />
                            <input type="button" class="cmd year" reporttype="year" value="" />
                            Switch to:<input type="radio" name="HistoricalDetail_DowntimeActualVsGoal_Switchto" id="HistoricalDetail_DowntimeActualVsGoal_Switchto_0" value="1" checked="checked" /><label for="HistoricalDetail_DowntimeActualVsGoal_Switchto_0">Downtimes</label>
                            <input type="radio" name="HistoricalDetail_DowntimeActualVsGoal_Switchto" id="HistoricalDetail_DowntimeActualVsGoal_Switchto_1" value="0" /><label for="HistoricalDetail_DowntimeActualVsGoal_Switchto_1">Occurences</label>
                            <input type="button" value="" style="display: none;" class="setting setgoals" />
                            <div class="report">
                            </div>
                        </div>

                        <div id="HistoricalDetail_Top5DowntimeEvents">
                            <div class="report">
                            </div>
                        </div>

                        <div id="HistoricalDetail_Top5OccuringEvents">
                            <div class="report">
                            </div>
                        </div>

                        <div id="HistoricalDetail_DowntimeHistory">
                            <input type="button" class="cmd" reporttype="hours" value="Hours" />
                            <input type="button" class="cmd day" reporttype="day" value="" />
                            <input type="button" class="cmd week" reporttype="week" value="" />
                            <input type="button" class="cmd month" reporttype="month" value="" />
                            <input type="button" class="cmd year" reporttype="year" value="" />
                            <div class="report">
                            </div>
                        </div>

                        <div id="HistoricalDetail_OccurrenceHistory">
                            <input type="button" class="cmd" reporttype="hours" value="Hours" />
                            <input type="button" class="cmd day" reporttype="day" value="" />
                            <input type="button" class="cmd week" reporttype="week" value="" />
                            <input type="button" class="cmd month" reporttype="month" value="" />
                            <input type="button" class="cmd year" reporttype="year" value="" />
                            <div class="report">
                            </div>
                        </div>
                        <div style="width: 100%; float: left; margin-top: 15px;">
                            <label style="font-size: 15px; text-align: center; display: block;">Downtime Timeline</label>
                            <div id="timeline"></div>
                            <label>*Red boxes represent downtime events.  Scroll in with your mouse to see more detail. Click and drag to move throughout the timeline.</label>
                        </div>
                        <input type="button" value="" class="xls downtimeexporttoxls" style="float: right;" />
                         <input type="button"  value="" class="efficiencytoxls" style="float:right;" />
                    </div>

                    <div id="TI_ADMIN" class="TabItem">
                        <fieldset style="width: 95%;">
                            <legend>Goal Settings</legend>
                            <table id="redLineListTable" cellpadding="3" cellspacing="1" width="98%" align="center" style="text-align: center; margin-top: 10px;"></table>
                            <input type="button" id="setRedLines" class="addnew" value="" />
                        </fieldset>
                    </div>

                    <div id="TI_HiddenReasons" class="TabItem">
                        <div id="TI_HiddenReasons_top5downtime">
                            <div class="report">
                            </div>
                        </div>

                        <div id="TI_HiddenReasons_top5occuring">
                            <div class="report">
                            </div>
                        </div>

                        <div id="TI_HiddenReasons_actualAs">
                            <input type="button" class="cmd" reporttype="hours" value="Hours" />
                            <input type="button" class="cmd day" reporttype="day" value="" />
                            <input type="button" class="cmd week" reporttype="week" value="" />
                            <input type="button" class="cmd month" reporttype="month" value="" />
                            <input type="button" class="cmd year" reporttype="year" value="" />
                            <br />
                            Switch to:<input type="radio" name="TI_HiddenReasons_actualAs_Switchto" id="TI_HiddenReasons_actualAs_Switchto0" value="1" checked="checked" /><label for="TI_HiddenReasons_actualAs_Switchto0">Downtimes</label>
                            <input type="radio" name="TI_HiddenReasons_actualAs_Switchto" id="TI_HiddenReasons_actualAs_Switchto1" value="0" /><label for="TI_HiddenReasons_actualAs_Switchto1">Occurences</label>

                            <div class="report">
                            </div>
                        </div>
                        <div id="TI_HiddenReasons_eventsRows">
                            <table cellpadding="2" cellspacing="1" bgcolor="#000000" id="hidden-events-grid">
                                <caption style="color: Blue; font-weight: bold;">Table Detail</caption>
                                <thead>
                                    <tr>
                                        <th width="70"><a href="javascript:$('#home-events-grid').sortTable({onCol: 1, keepRelationships: true,sortDesc: true,sortType: 'numeric'})" id="A1">Minutes</a></th>
                                        <th width="80"><a href="javascript:$('#home-events-grid').sortTable({onCol: 2, keepRelationships: true,sortDesc: true,sortType: 'numeric'});" id="A2">Occurrences</a></th>
                                        <th>ReasonCode</th>
                                    </tr>
                                    <thead>
                                        <tbody>
                                        </tbody>
                            </table>
                        </div>
                    </div>


                </div>

            </div>
        </form>
    </div>
    <div id="dock">
        <div class="container">
            <a href="#" tabitem="TI_Home" class="home"></a>
            <a href="#" id="Top5DowntimeEventsHref" tabitem="TI_Top5DowntimeEvents" class="Top5DowntimeEvents"></a>
            <a href="#" id="Top5OccuringEventsHref" tabitem="TI_Top5OccuringEvents" class="Top5OccuringEvents"></a>
            <a href="#" id="HistoricalDetailHref" tabitem="TI_HistoricalDetail" class="HistoricalDetail"></a>
            <a href="#" style="display: none;" class="DetailsbyShift"></a>
            <a href="#" class="HiddenReasons" tabitem="TI_HiddenReasons"></a>
            <a href="#" id="settingHref" tabitem="TI_ADMIN" class="Administrator"></a>
            <div id="copyright">
                <span>&copy; 2013 InfoStream Solutions</span>
                <a title="Created by InfoStream Solutions" id="createby" target="_blank" href="http://www.infostreamusa.com/">Created by InfoStream Solutions</a>
            </div>
        </div>
    </div>
</body>
</html>
