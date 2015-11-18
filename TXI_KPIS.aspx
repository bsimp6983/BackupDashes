<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TXI_KPIS.aspx.cs" Inherits="DowntimeCollection_Demo.TXI_KPIS" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!--[if lt IE 9]>
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.9.2/jquery-ui.min.js"></script>
    <![endif]-->
    <!--[if gte IE 9]><!-->
        <script src="//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.2/jquery-ui.min.js"></script>
    <!--<![endif]-->
    
    <script src="Scripts/jquery-ui-timepicker-addon.js" type="text/javascript"></script>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    
    <link href="//netdna.bootstrapcdn.com/twitter-bootstrap/2.3.1/css/bootstrap-combined.min.css" rel="stylesheet" />
    <script src="Scripts/moment.min.js" type="text/javascript"></script>

    <title>TXI KPIS</title>

    <script type="text/javascript">
        $(document).ready(function () {

            $('#txtStartDate').datetimepicker().val(moment().subtract('days', 7).format('M/D/YYYY'));
            $('#txtStopDate').datetimepicker().val(moment().format('M/D/YYYY'));

            $('#btnEnter').click(function () {
                $.get('TXI_KPIS.aspx?op=calculation&sd=' + $('#txtStartDate').val() + '&ed=' + $('#txtStopDate').val() + '&line=' + $("#slLines").val(), function (res) {
                    if (res != false) {
                        var mtbf = res.MTBF;
                        var rf = res.RF;
                        var uf = res.UF;

                        var totalMinutes = res.TotalMinutes;
                        var uptime = res.Uptime;
                        var downtime = res.Downtime;
                        var nonDowntime = res.NonDowntime;
                        var downtimeOccurrences = res.DowntimeOccurrences;
                        var nonDowntimeOccurrences = res.NonDowntimeOccurrences;
                        var totalOccurrences = res.TotalOccurrences;

                        $('#mtbf').text(mtbf.toFixed(2));
                        $('#rf').text(rf);
                        $('#uf').text(uf);

                        $("#lblCalculation").html(''.concat(
                            'Total Minutes: ' + totalMinutes + '<br />',
                            'Uptime: ' + uptime + '<br />',
                            'Downtime: ' + downtime + '<br />',
                            'Non-Downtime: ' + nonDowntime + '<br />',
                            'Downtime Occurrences: ' + downtimeOccurrences + '<br />',
                            'Non-Downtime Occurrences: ' + nonDowntimeOccurrences + '<br />',
                            'Total Occurrences: ' + totalOccurrences
                        ));

                    }
                }, 'json');
            });


        });

    </script>

</head>
<body>
    <div>
        <input type="text" id="txtStartDate" /> <input type="text" id="txtStopDate" /> 
        <select id="slLines">
            <option value="Raw_Mill_2">Raw Mill 2</option>
            <option value="Kiln_2">Kiln 2</option>
            <option value="Coal_Mill_C">Coal Mill C</option>
            <option value="Finish_Mill_1">Finish Mill 1</option>
            <option value="Finish_Mill_3">Finish Mill 3</option>
            <option value="Cooler_2">Cooler 2</option>
        </select>

        <button id="btnEnter" class="btn" >Enter</button> <br />

        <label id="lblCalculation">
        
        </label>

        <table class="table">
            <thead>
                <tr>
                    <th>
                        MTBF
                    </th>
                    <th>
                        RF
                    </th>
                    <th>
                        UF
                    </th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>
                        <label id="mtbf"></label>
                    </td>
                    <td>
                        <label id="rf"></label>
                    </td>
                    <td>
                        <label id="uf"></label>
                    </td>
                </tr>
            </tbody>
        </table>

    </div>
</body>
</html>
