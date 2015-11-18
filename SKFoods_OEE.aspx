<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SKFoods_OEE.aspx.cs" Inherits="DowntimeCollection_Demo.SKFoods_OEE" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SKFoods OEE</title>
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />

    <!--[if lt IE 9]>
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.9.2/jquery-ui.min.js"></script>
    <![endif]-->
    <!--[if gte IE 9]><!-->
        <script src="//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js"></script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.2/jquery-ui.min.js"></script>
    <!--<![endif]-->
    
    <script src="Scripts/jquery-ui-sliderAccess.js" type="text/javascript"></script>
    <script src="Scripts/jquery-ui-timepicker-addon.js" type="text/javascript"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#to').datetimepicker();
            $('#from').datetimepicker();

            $('#to').datetimepicker('setDate', new Date()).datetimepicker('show').datetimepicker('hide');
            $('#from').datetimepicker('setDate', new Date());
            $('#downtimeMinutes').val(0),
            $('#partsPerHour').val(0),
            $('#totalPieces').val(0),
            $('#goodUnits').val(0)

            $('#form').submit(function (e) {
                e.preventDefault();

                var form = {
                    to: $('#to').val(),
                    from: $('#from').val(),
                    downtimeMinutes: $('#downtimeMinutes').val(),
                    partsPerHour: $('#partsPerHour').val(),
                    totalPieces: $('#totalPieces').val(),
                    goodUnits: $('#goodUnits').val()
                };

                $.post('SKFoods_OEE.aspx?op=calculate', {
                    data: JSON.stringify(form)
                }, function (res) {
                    if (res) {
                        $('#machineAvailability').text(res.machineAvailability.toFixed(2) * 100 + "%");
                        $('#quality').text(res.quality.toFixed(2) * 100 + "%");
                        $('#performance').text(res.performance.toFixed(2) * 100 + "%");
                        $('#uptime').text(res.uptime);
                        $('#scheduledTimeMinutes').text(res.scheduledTimeMinutes);
                        $('#total').text(res.total.toFixed(2) * 100 + "%");
                    }
                });
            });
        });
    </script>
</head>
<body>
    <form action="SKFoods_OEE.aspx" id="form">
        <ul>
            <li>
                From <input type="text" id="from" />
                To <input type="text" id="to" />
            </li>
            <li>
                OEE Total: <span id="total" style="font-weight: bold" ></span>
            </li>
            <li>
                Machine Availability <span id="machineAvailability" style="font-weight: bold"  ></span>
            </li>
            <li>
                Downtime Minutes <input type="text" id="downtimeMinutes" />
            </li>
            <li>
                Uptime <span id="uptime" style="font-weight: bold"  ></span>
            </li>
            <li>
                Schedule Time Minutes <span id="scheduledTimeMinutes" style="font-weight: bold"  ></span>
            </li>
            <li>
                Performance <span id="performance" style="font-weight: bold" ></span>
            </li>
            <li>
                Parts Per Hour <input type="text" id="partsPerHour" />
            </li>
            <li>
                Total Pieces <input type="text" id="totalPieces" />
            </li>
            <li>
                Quality <span id="quality" style="font-weight: bold"  ></span>
            </li>
            <li>
                Good Units <input type="text" id="goodUnits" />
            </li>
        </ul>        
        <button type="submit">Calculate</button>
    </form>
</body>
</html>
