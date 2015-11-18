<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TvDashboardDemo.aspx.cs" Inherits="DowntimeCollection_Demo.TvDashboardDemo" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Tv Dashboard Demo</title>
    <link href="Styles/TvDemo.css" rel="Stylesheet" type="text/css" />
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <script src="scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="scripts/Visifire.js" type="text/javascript"></script>
    <script src="scripts/TvDashboardDemo.js" type="text/javascript"></script>
    <script src="scripts/jquery.jmodal.js" type="text/javascript"></script>
    <script src="scripts/jquery.easing.1.3.js" type="text/javascript"></script>
    <script src="scripts/jquery.dataTables.js" type="text/javascript"></script>
    <script src="scripts/jquery.corner.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    <script src="scripts/jquery.tableSort.js" type="text/javascript"></script>
    <script src="scripts/floating.js" type="text/javascript"></script>
    <script type="text/javascript">
        var line = 'all';

        var now = new Date();

        var datapoints = new Array();

        datapoints['0'] = '                 <vc:DataPoint AxisXLabel="12AM" YValue="10"/>';
        datapoints['1'] = '                 <vc:DataPoint AxisXLabel="1AM" YValue="80"/>';
        datapoints['2'] = '                 <vc:DataPoint AxisXLabel="2AM" YValue="95"/>';
        datapoints['3'] = '                 <vc:DataPoint AxisXLabel="3AM" YValue="92"/>';
        datapoints['4'] = '                 <vc:DataPoint AxisXLabel="4AM" YValue="85"/>';
        datapoints['5'] = '                 <vc:DataPoint AxisXLabel="5AM" YValue="70"/>';
        datapoints['6'] = '                 <vc:DataPoint AxisXLabel="6AM" YValue="30"/>';
        datapoints['7'] = '                 <vc:DataPoint AxisXLabel="7AM" YValue="0"/>';
        datapoints['8'] = '                 <vc:DataPoint AxisXLabel="8AM" YValue="0"/>';
        datapoints['9'] = '                 <vc:DataPoint AxisXLabel="9AM" YValue="0"/>';
        datapoints['10'] = '                 <vc:DataPoint AxisXLabel="10AM" YValue="10"/>';
        datapoints['11'] = '                 <vc:DataPoint AxisXLabel="11AM" YValue="80"/>';
        datapoints['12'] = '                 <vc:DataPoint AxisXLabel="12PM" YValue="95"/>';
        datapoints['13'] = '                 <vc:DataPoint AxisXLabel="1PM" YValue="92"/>';
        datapoints['14'] = '                 <vc:DataPoint AxisXLabel="2PM" YValue="85"/>';
        datapoints['15'] = '                 <vc:DataPoint AxisXLabel="3PM" YValue="70"/>';
        datapoints['16'] = '                 <vc:DataPoint AxisXLabel="4PM" YValue="30"/>';
        datapoints['17'] = '                 <vc:DataPoint AxisXLabel="5PM" YValue="0"/>';
        datapoints['18'] = '                 <vc:DataPoint AxisXLabel="6PM" YValue="10"/>';
        datapoints['19'] = '                 <vc:DataPoint AxisXLabel="7PM" YValue="60"/>';
        datapoints['20'] = '                 <vc:DataPoint AxisXLabel="8PM" YValue="95"/>';
        datapoints['21'] = '                 <vc:DataPoint AxisXLabel="9PM" YValue="94"/>';
        datapoints['22'] = '                 <vc:DataPoint AxisXLabel="10PM" YValue="75"/>';
        datapoints['23'] = '                 <vc:DataPoint AxisXLabel="11PM" YValue="76"/>';

        var chartXamlTop = '<vc:Chart xmlns:vc="clr-namespace:Visifire.Charts;assembly=SLVisifire.Charts"  AnimatedUpdate=\"false\" AnimationEnabled=\"false\" Width="453" Height="171" Theme="Theme1">'
            + '     <vc:Chart.Titles>'
            + '         <vc:Title Text="Line Efficiency Chart" />'
            + '     </vc:Chart.Titles>'
            + '     <vc:Chart.Series>'
            + '         <vc:DataSeries RenderAs="Line">'
            + '             <vc:DataSeries.DataPoints>';

        var chartXamlBottom = '             </vc:DataSeries.DataPoints>'
            + '         </vc:DataSeries>'
            + '     </vc:Chart.Series>'
            + ' </vc:Chart>';

        function refreshData()
        {
            var sd=_dataRange.startDate();
            var ed=_dataRange.endDate();

            LossBuckets(_dataRange.startDate(), _dataRange.endDate(), line);
            Top5DowntimeEvents(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, line);
            DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1,_firstReportConfig.isEvents,_firstReportConfig.goal(),_firstReportConfig.type, line);
            GetEventRows(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, line);
            Top5DowntimeEvents_Comments(_dataRange.startDate(),_dataRange.endDate(), _secondReportConfig.level1, _secondReportConfig.levelid, _secondReportConfig.level3, line);
            getTotalDowntime(_dataRange.startDate(),_dataRange.endDate(), line);
            Hidden_top5occuring(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, line);
            setEfficiencyChart();
        }
        

        function formatCurrency(num) {
            num = num.toString().replace(/\$|\,/g, '');
            if (isNaN(num))
                num = "0";
            sign = (num == (num = Math.abs(num)));
            num = Math.floor(num * 100 + 0.50000000001);
            cents = num % 100;
            num = Math.floor(num / 100).toString();
            if (cents < 10)
                cents = "0" + cents;
            for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
                num = num.substring(0, num.length - (4 * i + 3)) + ',' +
                    num.substring(num.length - (4 * i + 3));
            return (((sign) ? '' : '-') + '$' + num);
        }

        function setEfficiencyChart()
        {
            var hour = now.getHours();

            for(var x = (now.getHours() - 8); x <= (hour - 1); x++)
            {
                if(datapoints[x] != undefined)
                    chartXamlTop += datapoints[x];
            }

            chartXamlTop += chartXamlBottom;


            var width = 448;
            var height = 171;
            var vChart = new Visifire('SL.Visifire.Charts.xap', 'effChart', width, height, "Transparent", true);

            vChart.setDataXml(chartXamlTop);
            
            vChart.render($('#EfficiencyChart .report').get(0));

            $('#efficiencyLine .stat').text(line);

        }

        function setAnnualSavings()
        {
            $.post('TvDashboardDemo.aspx?op=getSavings', function(data){
                $('#savings').text(formatCurrency(data));
            });

        }

        var counters = new Array();

        counters[1] = 0;
        counters[2] = 0;
        counters[3] = 0;
        counters[4] = 0;
        counters[5] = 0;


        function refreshLineStatus()
        {
            setAnnualSavings();
            $.post('TvDashboardDemo.aspx', "op=getLineStatus" , function (data) {

                $.each(data, function(){
                    var line = "#line" + this.Line;
                    var time = "#line" + this.Line + "time";
                    var status = this.Status;
                    var counter = this.Counter;


                    if(status == true)
                    {
                        counters[this.Line] = counter;

                        $(line).css('color', 'red');

                        if(counter > 0)
                            $(time).text(counters[this.Line]);

                    }
                    else
                    {
                        $(line).css('color', 'white');

                        $(time).text('');

                        counters[this.Line] = 0;
                    }

                });

            }, "json");
        }
	    $(function () {
            <% 
            //if(Request.Cookies["first-dashboard-dailog-opened"]==null){
            %>
            /*
	        $("#dialog:ui-dialog").dialog("destroy");
	        $("#dialog-confirm").dialog({
	            resizable: false,
	            height: "auto",
                width:400,
	            modal: true,
	            buttons: {
	                OK: function () {
	                    $(this).dialog("close");
	                }
	            }
	        });
            */
            <%
            //HttpCookie cookie=new HttpCookie("first-dashboard-dailog-opened","true");
            //cookie.Expires=DateTime.Now.AddYears(2);
            //Response.Cookies.Add(cookie);
            //} 
            %>

            line = "all";

            refreshData();
            refreshLineStatus();


            self.setInterval('refreshData()', 1000 * 30);
            self.setInterval('refreshLineStatus()', 1000);
    
            $('#btnGo').click(function(){
                refreshData();
            });


            $('#all').click(function() {
                line = "all";
                refreshData();
            
            });

            $('#line1').click(function() {
                line = 1;
                refreshData();

            
            });

            $('#line2').click(function() {
                line = 2;
                refreshData();
            
            });

            $('#line3').click(function() {
                line = 3;
                refreshData();
            
            });

            $('#line4').click(function() {
                line = 4;
                refreshData();
            
            });

            $('#line5').click(function() {
                line = 5;
                refreshData();
            
            });

            $('#DowntimeActualVsGoal input[type="button"].cmd').click(function() {
                _firstReportConfig.type = $(this).attr('reportType');
                DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, _firstReportConfig.isEvents, _firstReportConfig.goal(), _firstReportConfig.type, line);
            });

            $('#DowntimeActualVsGoal input[type="radio"][name="DowntimeActualVsGoal_Switchto"]').click(function() {
                _firstReportConfig.isEvents = (parseInt($(this).val()) == 1)
                DowntimeActualVsGoal(_dataRange.startDate(),_dataRange.endDate(), _firstReportConfig.level1, _firstReportConfig.isEvents, _firstReportConfig.goal(), _firstReportConfig.type, line);
            });

            $('#DowntimeActualVsGoal input[type="button"].setting').click(function() {
                settingVoid();
            });
	    });

	</script>
</head>
<body>    
<div id="dialog-confirm" title="Tips" style="display:none;">
	<p>Select a date range at the top and click go.  Make sure to click on the graphs to drill down into the data.  For detailed instructions click <a href="http://legacy.downtimecollectionsolutions.com/index.php/tour/" target="_blank">here</a>.</p>
</div>
    <div id="header">
        <div class="container">
            <div id="buttons">
                        <a href="#" id="all" >View All</a>
                        <a href="#" id="line1" >Line 1</a>
                        <a href="#" id="line2" >Line 2</a>
                        <a href="#" id="line3" >Line 3</a>
                        <a href="#" id="line4" >Line 4</a>
                        <a href="#" id="line5" >Line 5</a>
                        <!--
                        <a href="#" id="line1time" ></a>
                        <a href="#" id="line2time" ></a>
                        <a href="#" id="line3time" ></a>
                        <a href="#" id="line4time" ></a>
                        <a href="#" id="line5time" ></a> -->
            </div>
            <div id="annual">
                <label id="savings"></label>
            </div>
            <div class="inputs">
                <div id="dates">
					<input type="text" class="datepicker" readonly="readonly" id="startdate" /><br />
					<input type="text" id="enddate" readonly="readonly" class="datepicker"/>  
                </div>
                <!-- <div id="Total-Downtime"><label>0.00</label> Minutes</div> -->  
                <input type="button" value=" " class="go" id="btnGo" />
            </div>

        </div>
    </div>
<div id="contentBg" >
    <div id="contentCharts">
        <div id="chartContainer">
        <div id="topCharts">
            <div id="EfficiencyChart" class="chart">
                <div id="Efficiency">

                    <div class="stats">
                        <div style="clear:both;"></div>
                        <div id="efficiencyLine">
                            <center><label id="Efficiency1" class='stat'></label></center>
                        </div>
                        <div id="runningEfficiency">
                            <center style="padding-top:4px;">
                            <label style="font-size: 14px; font-weight: bold;color:#ffffff;text-shadow:#426c31 1px 1px 1px;">Running Efficiency</label>
                            <br />
                            <label style="font-size: 10px; font-weight: bold;color:#ffffff;text-shadow:#426c31 1px 1px 1px;">since 8:00am</label>
                            <br />
                            <center style="padding-top:12px;">
							<label class='stat' >70.84%</label> <br />
                            <label class='stat' style="font-size: 12px">4982 Cases</label>
                            </center>
                            </center>
                        </div>
                        <div id="yesterdayEffiency">
                            <center style="margin-top:10px;">
                            <label style="font-size: 13px; font-weight: bold;color:#ffffff;text-shadow:#426c31 1px 1px 1px;">Yesterday's Efficiency</label>
                            <br />
                            <center style="padding-top:12px;">
							<label class='stat'>80.24%</label><br />
                            <label class='stat' style="font-size: 12px">8250 Cases</label>
                            </center>
                            </center>
                        </div>
                    </div>
                </div>
                <div class="report" >     
                </div>
            </div>

            <div id="LossBuckets" class="chart">
                <div class="report">     
                </div>
            </div>
        </div>
        <div id="middleCharts">

            <div id="DowntimeActualVsGoal_rows" class="chart">
                <table cellpadding="2" cellspacing="1" bgcolor="#000000" id="home-events-grid">
                    <caption style="color:#032f4d;font-weight:bold;text-shadow:#cccccc 1px 1px 1px">Downtime Events Detail</caption>
                    <thead> 
                    <tr>
                        <th width="70"><a href="javascript:$('#home-events-grid').sortTable({onCol: 1, keepRelationships: true,sortDesc: true,sortType: 'numeric'})" id="sortByMinutes">Minutes</a></th>
                        <th width="80"><a href="javascript:$('#home-events-grid').sortTable({onCol: 2, keepRelationships: true,sortDesc: true,sortType: 'numeric'});" id="sortByOccurrences">Occurrences</a></th>
                        <th>ReasonCode</th>
                    </tr>  
                    </thead> 
                    <tbody>
                    </tbody>                  
                </table>
            </div>
            <div id="Top5DowntimeEvents" class="chart">
                <div class="report">
                </div>
            </div>
            
            <div id="TI_HiddenReasons_top5occuring" class="chart">
                <div class="report">
                </div>
            </div>
        </div>
        <div id="bottomCharts">
            
            <div id="DowntimeActualVsGoal" class="chart">
                <div id="moveButtonsDown">
					<input type="button" style="display:none;" class="cmd" reportType="hours" value="Hours" /> 
					<input type="button" class="cmd day" reportType="day" value="" /> 
					<input type="button" class="cmd week" reportType="week" value="" /> 
					<input type="button" class="cmd month" reportType="month" value="" /> 
					<input type="button" class="cmd year" reportType="year" value="" /> 
                </div>
                <!--Switch to:<input type="radio" name="DowntimeActualVsGoal_Switchto" id="DowntimeActualVsGoal_Switchto_0" value="1" checked="checked" /><label for="DowntimeActualVsGoal_Switchto_0">Downtimes</label>
                <input type="radio" name="DowntimeActualVsGoal_Switchto" id="DowntimeActualVsGoal_Switchto_1" value="0" /><label for="DowntimeActualVsGoal_Switchto_1">Occurences</label>
                -->
                
                <div class="report">
                </div>
           </div>
            
                <div id="TI_Top5DowntimeEvents_Comments" class="chart">
                    <table cellpadding="3" cellspacing="0">
                    <caption></caption>
                    </table>
                    <input type="button" value="" id="exporttoxls" class="xls exporttoxls" />
                </div>
        </div>
        </div>
    </div>
</div>
<div id="footer">
    <div class="container">
        <div id="copyright">
            <a title="Created by InfoStream Solutions" id="createby" target="_blank" href="http://www.infostreamusa.com/">&copy; 2012 Dashboard by InfoStream Solutions <img src="images/infostream.png" /></a>
        </div>
    </div>
</div>
</body>
</html>
