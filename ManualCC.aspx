<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManualCC.aspx.cs" Inherits="DowntimeCollection_Demo.Styles.ManualCC" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
    <script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
    <script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery-ui-timepicker-addon.js" type="text/javascript"></script>
<head runat="server">
    <title>Lebelgier Manual Case Counts</title>

    <script type="text/javascript">
        var casecounts = new Array();
        var currentFirstCount = 0;
        var maxCount = 0;

        function createCaseCount() {
            var casecount = {
                customer: '',
                mold: '',
                skew: '',
                date: new Date(),
                hour: new Date().getHours(),
                value: '',
            };

            return casecount;
        }

        Date.hourBetween = function( date1, date2 ) {
          //Get 1 day in milliseconds
          var one_day=1000*60*60;

          // Convert both dates to milliseconds
          var date1_ms = date1.getTime();
          var date2_ms = date2.getTime();

          // Calculate the difference in milliseconds
          var difference_ms = date2_ms - date1_ms;
    
          // Convert back to days and return
          return Math.round(difference_ms/one_day); 
        }

        function getCaseCounts() {
            
            casecounts = new Array();
            
            /*
            var dayDiff = Date.hourBetween(sd, ed);
            var h = 0;

            for(var x = 0; x < dayDiff; x++)
            {
                var cc = createCaseCount();
                var hour = h;

                if(hour > 23)
                    h = h - 12;
                
                //var d = new Date(year, month, day, hours, minutes, seconds, milliseconds);
                cc.date = new Date(sd.getFullYear(), sd.getMonth(), sd.getDate(), hour, 01, sd.getSeconds(), sd.getMilliseconds());//sd.setHours(sd.getHours() + x);
                cc.hour = cc.date.getHours();
                
                casecounts[x] = cc;

                h += 1;
                
            }
            */

            $.post('ManualCC.aspx', 'op=GetCaseCounts&date=' + formatDate(new Date()), function (data) { 

                if(data.length > 0)
                {
                    $('#tblCaseCounts tbody').empty();
                    var counter = 0;
                    $(data).each(function(){
                        var date = new Date(this.date);
                        var hour = date.getHours();
                        var value = this.value;
                        var day = date.getDate();
                        var cases = this.cases;
                        var customer = this.customer;
                        var mold = this.mold;
                        var skew = this.skew;
                        var ischangeover = false;

                        var cc = createCaseCount();

                        cc.date = date;
                        cc.mold = mold;
                        cc.skew = skew;
                        cc.customer = customer;
                        cc.value = cases;

                        if(this.ischangeover == true || this.ischangeover == 'true' || this.ischangeover == 'True')
                        {
                            ischangeover = true;

                        }

                        cc.hour = hour;

                        var show = false;

                        if(counter >  data.length - 9)
                        {
                            show = true;
                            
                            currentFirstCount = counter;
                            
                        }
                        
                        var row = '<tr id="count' + counter + '"' + (!show ? 'style="display:none"' : '') + '>';
                        row += '<td class="hour">' + (isNaN(hour) ? '' : calculateHour(date, ischangeover)) + '</td>';
                        row += '<td class="cases">' + cases + '</td>';
                        row += '<td class="customer">' + customer + '</td>';
                        row += '<td class="mold">' + mold + '</td>';
                        row += '<td class="skew">' + skew + '</td>';
                        row += '<td onclick="editCase(' + counter + ')"> <button>Edit</button></td>';
                        row += '</tr>';

                        /*
                        for(var x = 0; x < dayDiff; x++)
                        {
                            var d = casecounts[x].date.getDate();
                            var h = casecounts[x].date.getHours();

                            if(d == day && h == hour)
                            {
                                casecounts[x].date = date;
                                casecounts[x].value = value;
                                casecounts[x].hour = hour;

                                break;
                            }
                        }
                        */

                        casecounts.push(cc);
                        $('#tblCaseCounts').append(row);
                        counter++;

                    });

                    maxCount = counter;

                    
                }
                
            }, 'json');
        }

        function formatDate(d)
        {
            var date = new Date(d);
            

            return (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear() + ' ' + date.toLocaleTimeString();

        }
        
        function updateCase(val)
        {
            var id = '#count' + val;

            var cc = casecounts[val];

            var counts = $('#txtCount').val();

            $.post('ManualCC.aspx', { op: 'UpdateCC', date: formatDate(cc.date), counts: counts }, function(data){
                getCaseCounts();
                cancel();
            });

            


        }

        function createCase()
        {
            //var d = new Date(year, month, day, hours, minutes, seconds, milliseconds);
            var date = new Date($('#txtDate').val());
            var hour = $('#txtHour').val();
            
            var datetime = new Date(date.getFullYear(), date.getMonth(), date.getDate(), hour, 00, 00, 00);

            var counts = $('#txtCount').val();

            $.post('ManualCC.aspx', { op: 'UpdateCC', date: formatDate(datetime), counts: counts }, function(data){
                
                    getCaseCounts();
                    cancel();
            });
            


        }

        function editCase(val)
        {
            var id = '#count' + val;

            $('#txtDate').val($.datepicker.formatDate('mm/dd/yy', new Date(casecounts[val].date))).attr('disabled', 'disabled');
            $('#txtHour').val(casecounts[val].hour).attr('disabled', 'disabled');
            $('#txtCount').val($(id + ' .cases').text());
            $('#btnAdd').attr('edit', 'true').text('Edit Case Count').unbind('click').bind('click', function(){
                    var ans = confirm('Edit Case Count?');

                    if(ans)
                    {
                        updateCase(val);
                    }

                    $('#btnAdd').unbind('click', this);
                
            });

        }
        
        function cancel()
        {
        
            $('#txtDate').val($.datepicker.formatDate('mm/dd/yy', new Date())).attr('disabled', '');
            $('#txtHour').val(0).attr('disabled', '');
            $('#txtCount').val('');
            $('#btnAdd').attr('edit', 'false').text('Add New Case Count').unbind('click').bind('click', function(){
                    var ans = confirm('Create Case Count?');

                    if(ans)
                    {
                        createCase();
                    }

                    $('#btnAdd').unbind('click', this);
                
            });
        }

        function calculateHour(date, ischangeover)
        {
            date = new Date(date);
            var hour = date.getHours();
            var min = date.getMinutes();
            var newHour = '';
            var showMin = false;

            if(min > 0 || ischangeover == true)
                showMin = true;

            if(!showMin)
                min = '';
            else
            {
                var tmp = '';

                if(min == 0)
                    tmp = '00';
                else if(min > 0 && min < 10)
                    tmp = '0' + min;
                else
                    tmp = min;
                        
                min = ':' + tmp;
            }
            if(hour == 0)
            {
                newHour = 12 + min + 'AM';
            }
            else if(hour == 12)
            {
                newHour = hour.toString() + min + 'PM';
            }
            else if(hour > 12)
            {
                hour = hour - 12;

                if(hour == 0)
                    hour = 12;

                newHour = hour.toString() + min + 'PM';
            }
            else if(hour < 12)
            {
                if(hour == 0)
                    hour = 12;

                newHour = hour + min + 'AM';
            }

            return $.datepicker.formatDate('mm/dd/yy', date) + ' ' + newHour;
        }
        
        $(document).ready(function () { 

            var date = new Date();
            var d = new Date();
            
            $('.datepicker').each(function(){
                $(this).datepicker();
            });

            $('.datetimepicker').each(function(){
                $(this).timepicker({});
            });
            

            //$('#startDate').val($.datepicker.formatDate('mm/dd/yy', date));
            
            //date.setDate(date.getDate() + 1);

            //$('#endDate').val($.datepicker.formatDate('mm/dd/yy', date));

            cancel();

            $('#btnCancel').click(function(){
                cancel();
            });
            /*
            $('#btnSetDate').click(function(){
                var sd = new Date($('#startDate').val());
                var ed = new Date($('#endDate').val());

                getCaseCounts(sd, ed);
            });

            $('#btnSetDate').click();
            */
            $('#last8').click(function(){
                var count = currentFirstCount;
                
                if(count - 8 > 0 && count <= maxCount)
                {
                    //$('#count' + (count - 7)).css('display', 'none');
                    for(var x = count; x >= count - 8; x--)
                    {
                        if(x < 0)
                            break;
                    
                        var id = '#count' + x;

                        $(id).css('display', 'none');
                        currentFirstCount = x;

                    }
                
                    for(var x = currentFirstCount - 7; x <= currentFirstCount; x++)
                    {
                        if(x == maxCount)
                            break;
                    
                        var id = '#count' + x;

                        $(id).css('display', '');

                    }
                }

            });

            $('#next8').click(function(){
                var count = currentFirstCount;
                
                if(count >= 0 && count + 7 <= maxCount)
                {
                    for(var x = count; x >= count - 9; --x)
                    {
                        if(x < 0)
                            break;
                    
                        var id = '#count' + x;

                        $(id).css('display', 'none');

                    }

                    for(var x = count + 1; x <= count + 8; ++x)
                    {
                        if(x > maxCount)
                            break;
                    
                        var id = '#count' + x;

                        $(id).css('display', '');

                        currentFirstCount = x;
                    }
                }


            });

            getCaseCounts();
        
        });
       
    
    </script>

</head>
<body>
    <div>
        <!--
        Start Date: <input type='text' id="startDate" class="datepicker" />
        End Date: <input type='text' id="endDate" class="datepicker" />
        <input type="button" id="btnSetDate" value="Set Time" />
        -->
        <table id="tblCaseCounts" style="width: 50%">
            <thead>
                <tr>
                    <th>Hour</th>
                    <th>Cases</th>
                    <th>Customer</th>
                    <th>Mold</th>
                    <th>Skus</th>
                    <th><button id="last8">Last 8</button> <button id="next8">Next 8</button></th>
                </tr>
            </thead>
            <tbody>
            </tbody>
        </table>
        Date:
        <input type="text" id="txtDate" class="datepicker" />
        Hour:
        <select id="txtHour">
            <option value="0">12AM</option>
            <option value="1">1AM</option>
            <option value="2">2AM</option>
            <option value="3">3AM</option>
            <option value="4">4AM</option>
            <option value="5">5AM</option>
            <option value="6">6AM</option>
            <option value="7">7AM</option>
            <option value="8">8AM</option>
            <option value="9">9AM</option>
            <option value="10">10AM</option>
            <option value="11">11AM</option>
            <option value="12">12PM</option>
            <option value="13">1PM</option>
            <option value="14">2PM</option>
            <option value="15">3PM</option>
            <option value="16">4PM</option>
            <option value="17">5PM</option>
            <option value="18">6PM</option>
            <option value="19">7PM</option>
            <option value="20">8PM</option>
            <option value="21">9PM</option>
            <option value="22">10PM</option>
            <option value="23">11PM</option>
        
        </select>
        Case Count:
        <input type="text" id="txtCount" />
        <button id="btnAdd" edit="false" >Add New Case Count</button>
        <button id="btnCancel" >Cancel</button>

    </div>
</body>
</html>
