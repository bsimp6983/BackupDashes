<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TvDemoControl.aspx.cs" Inherits="DowntimeCollection_Demo.Admin.TvDemoControl" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Tv Demo Controller</title>
<link href="../plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<link rel="stylesheet" type="text/css" media="all" href="../styles/jScrollPane.css" />
<script src="../scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="../plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<style type="text/css">
    body { font-size: 38px; }
    .timer { font-size: 30px; width: 100px;}
</style>
    <script type="text/javascript" >
        $(document).ready(function () {

            $.extend({
                getUrlVars: function () {
                    var vars = [], hash;
                    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
                    for (var i = 0; i < hashes.length; i++) {
                        hash = hashes[i].split('=');
                        vars.push(hash[0]);
                        vars[hash[0]] = hash[1];
                    }
                    return vars;
                },
                getUrlVar: function (name) {
                    return $.getUrlVars()[name];
                }
            });

            $('.btnClearLine').each(function () {
                $(this).click(function () {
                    var line = $(this).attr('id').replace('clear', '');

                    var ans = confirm('Are you sure you want to clear Line: ' + line + '?');



                    if (line != undefined && ans) {
                        $.post('TvDemoControl.aspx', { op: 'clearLine', line: line }, function (data) {
                            if (data == 'True')
                                alert('Line cleared');
                            else if(data != 'False')
                                alert(data);

                        });
                    }


                });

            });

            $('.controlTable a').each(function () {
                if ($(this).attr('class') == 'up') {
                    $(this).click(function () {
                        var line = $(this).attr('id');

                        line = line.replace('u', '');

                        var time = $('#line' + line + 'Timer').val();

                        stopTimer('#line' + line + 'Timer');

                        $.post('TvDemoControl.aspx', { op: "stopEvent", time: time, line: line }, function (data) {
                            if (data !== 'True')
                                alert(data);
                        });



                    });
                }

                if ($(this).attr('class') == 'down') {
                    $(this).click(function () {
                        var line = $(this).attr('id');

                        line = line.replace('d', '');

                        doTimer('#line' + line + 'Timer', line);



                    });
                }
            });

        });


        var times = new Array();

        times['#line1Timer'] = 0;
        times['#line2Timer'] = 0;
        times['#line3Timer'] = 0;
        times['#line4Timer'] = 0;
        times['#line5Timer'] = 0;

        var timeouts = new Array();

        timeouts['#line1Timer'] = 0;
        timeouts['#line2Timer'] = 0;
        timeouts['#line3Timer'] = 0;
        timeouts['#line4Timer'] = 0;
        timeouts['#line5Timer'] = 0;

        function timedCount(timer, line) {
            $(timer).val(times[timer]);
            times[timer]++;

            timeouts[timer] = setTimeout("timedCount('" + timer + "','" + line + "')", 1000);

            $.post('TvDemoControl.aspx', { op: "updateEvent", time: times[timer], line: line }, function (data) {
                if(data !== 'True')
                    alert(data);
            });
        }

        function doTimer(id, line) {
            if (times[id] == 0)
                timedCount(id, line);
        }

        function stopTimer(id) {
            clearTimeout(timeouts[id]);
            times[id] = 0;
            $(id).val('0');
        }
    
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <table class="controlTable" >
        <tr>
            <td><b>Line 1: </b></td>
            <td><b><a href="#" id="d1" class="down" >DOWN</a></b></td>
            <td><b><a href="#" id="u1" class="up" >UP</a></b></td>
            <td><b><input type="text" id="line1Timer" class="timer"/> </b></td>
            <td><input type="button" id="clear1" class="btnClearLine" value="Clear Line" style="float: right; width: 110px; height: 33px; font-size: 20px; font-weight: bolder;"/></td>
        </tr>
        <tr>
            <td><b>Line 2: </b></td>
            <td><b><a href="#" id="d2" class="down" >DOWN</a></b></td>
            <td><b><a href="#" id="u2" class="up" >UP</a></b></td>
            <td><b><input type="text" id="line2Timer" class="timer" /> </b></td>
            <td><input type="button" id="clear2" class="btnClearLine" value="Clear Line" style="float: right; width: 110px; height: 33px; font-size: 20px; font-weight: bolder;"/></td>
        </tr>
        <tr>
            <td><b>Line 3: </b></td>
            <td><b><a href="#" id="d3" class="down" >DOWN</a></b></td>
            <td><b><a href="#" id="u3" class="up" >UP</a></b></td>
            <td><b><input type="text" id="line3Timer" class="timer" /> </b></td>
            <td><input type="button" id="clear3" class="btnClearLine" value="Clear Line" style="float: right; width: 110px; height: 33px; font-size: 20px; font-weight: bolder;"/></td>
        </tr>
        <tr>
            <td><b>Line 4: </b></td>
            <td><b><a href="#" id="d4" class="down" >DOWN</a></b></td>
            <td><b><a href="#" id="u4" class="up" >UP</a></b></td>
            <td><b><input type="text" id="line4Timer" class="timer" /> </b></td>
            <td><input type="button" id="clear4" class="btnClearLine" value="Clear Line" style="float: right; width: 110px; height: 33px; font-size: 20px; font-weight: bolder;"/></td>
        </tr>
        <tr>
            <td><b>Line 5: </b></td>
            <td><b><a href="#" id="d5" class="down" >DOWN</a></b></td>
            <td><b><a href="#" id="u5" class="up" >UP</a></b></td>
            <td><b><input type="text" id="line5Timer" class="timer" /> </b></td>
            <td><input type="button" id="clear5" class="btnClearLine" value="Clear Line" style="float: right; width: 110px; height: 33px; font-size: 20px; font-weight: bolder;"/></td>
        </tr>
    
    </table>
    </form>
</body>
</html>
