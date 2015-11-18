<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AnnualSavings.aspx.cs" Inherits="DowntimeCollection_Demo.AnnualSaving" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<link rel="stylesheet" type="text/css" media="all" href="../styles/jScrollPane.css" />
<script src="scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<style type="text/css">
    #container { margin: 0 auto 0 auto; width: 50%;}
    #tabs div { margin: 0 auto; }
    #tabs-1 { }
    #tabs-2 { }
    .display { float:right; font-size: 24px; }
    .annual { background-color: #00FF33; font-size: 27px; font-weight: bold;}
    .annualLoss { background-color: Yellow; font-size: 27px; font-weight: bold;}

</style>

    <title>Annual Savings</title>
    <script type="text/javascript">
        $(document).ready(function () {
            $('#tabs').tabs();

            var name = null;
            var number = null;
            var email = null;

            function setPerson(savings, type) {
                name = $('#name').val();
                number = $('#number').val();
                email = $('#email').val();

                if (name != "" && number != "" && email != "") {

                    $.post('AnnualSavings.aspx?op=updatePerson', { name: name, number: number, email: email, savings: savings, type: type }, function (data) {
                        if (data != '1')
                            alert(data);
                    });
                }

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

            $('#computeLine').click(function () {
                var lines = $('#lines').val();
                var hourPerLine = $('#hourPerLine').val();
                var costPerHour = $('#costPerHour').val();

                var savings = lines * hourPerLine * costPerHour;

                setPerson(savings, 'Line');

                $('#annualLineLoss').text(formatCurrency(savings));

                $('#line10Red').text(formatCurrency(savings * .10));
                $('#line15Red').text(formatCurrency(savings * .15));
                $('#line25Red').text(formatCurrency(savings * .25));
                $('#line50Red').text(formatCurrency(savings * .50));

            });

            $('#computeMachine').click(function () {
                var numOfMachines = $('#numOfMachines').val();
                var opPerMachine = $('#operPerMachine').val();
                var avgOperatorWage = $('#avgOperatorWage').val();
                var minDownPerShift = $('#minDownPerShift').val();
                var shiftsPerDay = $('#shiftsPerDay').val();
                var workDaysPerYear = $('#workDaysPerYear').val();

                var savings = (((minDownPerShift * shiftsPerDay) / 60) * workDaysPerYear) * avgOperatorWage * numOfMachines;

                setPerson(savings, 'Machine');

                $('#annualMachineLoss').text(formatCurrency(savings));

                $('#mac10Red').text(formatCurrency(savings * .10));
                $('#mac15Red').text(formatCurrency(savings * .15));
                $('#mac25Red').text(formatCurrency(savings * .25));
                $('#mac50Red').text(formatCurrency(savings * .50));

            });

            $('.display').each(function () {
                $(this).click(function () {

                    var line = $(this).attr('id');

                    if (line.indexOf('Line') != -1)
                        line = '#line' + line.replace('disLine', '') + 'Red';
                    else
                        line = '#mac' + line.replace('disMachine', '') + 'Red';


                    var savings = $(line).text().replace('$', '');

                    $.post('AnnualSavings.aspx?op=setSavings', { savings: savings }, function (data) {
                        if (data != '1')
                            alert(data);
                    });

                });
            });

        });
    
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="container">
    <div id="customer">
        <table>
            <tr>
                <td>Name:</td>
                <td><input type="text" id="name" /></td>
                <td>Phone Number:</td>
                <td><input type="text" id="number" /></td>
                <td>Email:</td>
                <td><input type="text" id="email" /></td>
            
            </tr>
        </table>
    
    </div>
    <div id="tabs">
        <ul>
            <li><a href="#tabs-1">Lines</a></li>
            <li><a href="#tabs-2">Machines</a></li>
        </ul>
        <div id="tabs-1">
            <table>
                <tr>
                    <td>Lines</td>
                    <td><input type="text" id="lines" /></td>
                </tr>
                <tr>
                    <td>Yearly DT Hrs Per Line</td>
                    <td><input type="text" id="hourPerLine" /></td>
                </tr>
                <tr>
                    <td>Cost Per Hour</td>
                    <td><input type="text" id="costPerHour" /></td>
                </tr>
                <tr>
                    <td>&nbsp; </td>
                    <td>&nbsp; </td>
                </tr>
                <tr>
                    <td>Annual Loss</td>
                    <td class="annualLoss"><label id="annualLineLoss">$0</label></td>
                </tr>
                <tr>
                    <td>&nbsp; </td>
                    <td>&nbsp; </td>
                </tr>
                <tr>
                    <td></td>
                    <td><input type="button" id="computeLine" class="compute" value="Compute" style="font-size: 30px"/></td>
                </tr>
                <tr>
                    <td>&nbsp; </td>
                    <td>&nbsp; </td>
                </tr>
                <tr>
                    <td></td>
                    <td class="annual"><label>Annual Savings</label></td>
                </tr>
                <tr>
                    <td>10% Reduction</td>
                    <td class="annual"><label id="line10Red">$0</label><input type="button" value="Display" class="display" id="disLine10" /></td>
                </tr>
                <tr>
                    <td>15% Reduction</td>
                    <td class="annual"><label id="line15Red">$0</label><input type="button" value="Display" class="display" id="disLine15" /></td>
                </tr>
                <tr>
                    <td>25% Reduction</td>
                    <td class="annual"><label id="line25Red">$0</label><input type="button" value="Display" class="display" id="disLine25" /></td>
                </tr>
                <tr>
                    <td>50% Reduction</td>
                    <td class="annual"><label id="line50Red">$0</label><input type="button" value="Display" class="display" id="disLine50" /></td>
                </tr>
            </table>
        </div>
        <div id="tabs-2">
            <table>
                <tr>
                    <td>Number Of Machines</td>
                    <td><input type="text" id="numOfMachines" /></td>
                </tr>
                <tr>
                    <td>Operaters Per Machine</td>
                    <td><input type="text" id="operPerMachine" /></td>
                </tr>
                <tr>
                    <td>Average Operator Wage <i>(overhead)</i></td>
                    <td><input type="text" id="avgOperatorWage" /></td>
                </tr>
                <tr>
                    <td>Minutes of Downtime Per Shift</td>
                    <td><input type="text" id="minDownPerShift" /></td>
                </tr>
                <tr>
                    <td>Shifts Per Day</td>
                    <td><input type="text" id="shiftsPerDay" /></td>
                </tr>
                <tr>
                    <td>Work Days Per Year</td>
                    <td><input type="text" id="workDaysPerYear" /></td>
                </tr>
                <tr>
                    <td>&nbsp; </td>
                    <td>&nbsp; </td>
                </tr>
                <tr>
                    <td>Annual Loss</td>
                    <td class="annualLoss"><label id="annualMachineLoss">$0</label></td>
                </tr>
                <tr>
                    <td>&nbsp; </td>
                    <td>&nbsp; </td>
                </tr>
                <tr>
                    <td></td>
                    <td><input type="button" id="computeMachine" value="Compute" class="compute" style="font-size: 30px" /></td>
                </tr>
                <tr>
                    <td>&nbsp; </td>
                    <td>&nbsp; </td>
                </tr>
                <tr>
                    <td></td>
                    <td><label class="annual">Annual Savings</label></td>
                </tr>
                <tr>
                    <td>10% Reduction</td>
                    <td class="annual"><label id="mac10Red">$0</label><input type="button" value="Display" class="display" id="disMachine10" /></td>
                </tr>
                <tr>
                    <td>15% Reduction</td>
                    <td class="annual"><label id="mac15Red">$0</label><input type="button" value="Display" class="display"  id="disMachine15" /></td>
                </tr>
                <tr>
                    <td>25% Reduction</td>
                    <td class="annual"><label id="mac25Red">$0</label><input type="button" value="Display" class="display"  id="disMachine25" /></td>
                </tr>
                <tr>
                    <td>50% Reduction</td>
                    <td class="annual"><label id="mac50Red">$0</label><input type="button" value="Display" class="display"  id="disMachine50" /></td>
                </tr>
            </table>
        </div>
        </div>
    </div>
    </form>
</body>
</html>
