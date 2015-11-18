<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EfficiencyGraph.aspx.cs" Inherits="DowntimeCollection_Demo.EfficiencyGraph" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Efficiency Chart</title>
        
    <link href="Styles/demo_table.css" rel="stylesheet" type="text/css" />
    <link href="Styles/demo_page.css" rel="stylesheet" type="text/css" />
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />

    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="Scripts/underscore-min.js" type="text/javascript"></script>
    <script src="Scripts/backbone-min.js" type="text/javascript"></script>
    <script src="Scripts/handlebars.js" type="text/javascript"></script>
    <script src="Scripts/Visifire.js" type="text/javascript"></script>
    <script src="Scripts/moment.min.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery.qtip-1.0.0-rc3.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery-ui-timepicker-addon.js" type="text/javascript"></script>

    <script src="Scripts/efficiencyGraph.js" type="text/javascript"></script>

    <style>
    
        #tblData td
        {
            text-align: center;
        }
        
        #efficiencyGraph
        {
            margin: auto 0;
        }
    </style>

    <script type="text/javascript">
        $(document).ready(function(){
            EGraph.init();

        });
    </script>

</head>
<body>
    <div id="container">
        <table id="tblData">
            <thead>
                <tr>
                    <th>Minutes</th>
                    <th>Cycles Per Minute</th>
                    <th>Theoretical Cases</th>
                    <th>Actual Cases</th>
                    <th>Start Cycle</th>
                    <th>Stop Cycle</th>
                    <th>Machine Speed</th>
                    <th>Waste</th>
                    <th>Machine Line</th>
                    <th>Efficiency</th>
                    <th>Waste %</th>
                    <th>Downtime</th>
                    <th>Theo Downtime Minutes</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td id="minutes">0</td>
                    <td id="cyclesPerMinute">72</td>
                    <td id="theoCases">0</td>
                    <td id="actCases">0</td>
                    <td id="startCycle">0</td>
                    <td id="endCycle">0</td>
                    <td id="machineSpeed">3100</td>
                    <td id="waste">0</td>
                    <td id="machineLine">Splenda1</td>
                    <td id="efficiency">0</td>
                    <td id="wastePercent">0</td>
                    <td id="downtime">0</td>
                    <td id="theorDowntimeMinutes">0</td>
                </tr>
            </tbody>
        </table>
        <div>
            <input type="text" id="startDate" />
            <input type="text" id="endDate" />
            <button id="btnEnter">Submit</button>
        </div>
        <div id="effGraph" style="margin: auto 0;"></div>
    </div>
</body>
</html>
