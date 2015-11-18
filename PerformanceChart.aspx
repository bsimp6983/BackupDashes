<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PerformanceChart.aspx.cs" Inherits="DowntimeCollection_Demo.PerformanceChart" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Performance Graph</title>
    <link href="Styles/demo_table.css" rel="stylesheet" type="text/css" />
    <link href="Styles/demo_page.css" rel="stylesheet" type="text/css" />
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <link href="Scripts/timeline.css" rel="Stylesheet" type="text/css" />

    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="Scripts/underscore-min.js" type="text/javascript"></script>
    <script src="Scripts/backbone-min.js" type="text/javascript"></script>
    <script src="Scripts/handlebars.js" type="text/javascript"></script>
    <script src="Scripts/Visifire.js" type="text/javascript"></script>
    <script src="Scripts/moment.min.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    <script src="Scripts/timeline.js" type="text/javascript"></script>
    <script src="Scripts/jquery.qtip-1.0.0-rc3.min.js" type="text/javascript"></script>

    <script src="Scripts/pfchart.js" type="text/javascript"></script>
    
    <style type="text/css">
        body {font: 10pt arial;}

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

        $(document).ready(function () {
            PFChart.init();
            
            $( "#tabs" ).tabs();
            /*
            $('#pfTab').click(function(e){
                PFChart.CurrentTab = 0;
                PFChart.updatePerformanceGraph(true, function(){
                    //createTimeLine();
                });
            });

            $('#tlTab').click(function(e){
                PFChart.CurrentTab = 1;
                    PFChart.createTimeLine();
            });
            */
        });
    </script>
</head>
<body>
         <label id="lblMessage" style="font-size: 15px; font-weight: bold;"></label>
         
        Line: <select id="slLines"></select>
        <div id="tabs" style="width: 960px">
          <ul>
            <li><a href="#graph" id="pfTab">Performance Graph</a></li>
            <li><a href="#tabTimeLine" id="tlTab">Timeline</a></li>
          </ul>

            <div id="graph" style="width: 960px; height: 800px;">
                <div id="filter">
                    Start Date: <input type="text" id="txtStartDate" class="datetimepicker" />
                    End Date: <input type="text" id="txtEndDate" class="datetimepicker" />

                    <input type="button" value="Enter" id="btnEnter" />
        
                </div>
                <div id="group">
                    Hour: <input type="radio" name="group" value="hours" class="group" checked="checked"/>
                    Day :<input type="radio" name="group" value="days" class="group" />
                    Week :<input type="radio" name="group" value="weeks" class="group" />
                    Month :<input type="radio" name="group" value="months" class="group" />
                    Year :<input type="radio" name="group" value="years" class="group" />
                </div>
                <div id="perfChart" style="width: 850px; height: 800px;">
            
                </div>      
            </div>
            <div id="tabTimeLine">
                <div id="tlFilter">
                    Start Date: <input type="text" id="txtPerfStartDate" class="datetimepicker" />
                    End Date: <input type="text" id="txtPerfEndDate" class="datetimepicker" />

                    <input type="button" value="Enter" id="btnPerfEnter" />
        
                </div>
                <div id="timeline"></div>
            
            </div>
        </div>

</body>
</html>
