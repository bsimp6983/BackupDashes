<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TXI.aspx.cs" Inherits="TXI" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>TXI Dashboard</title>
<link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<script src="Scripts/global.js" type="text/javascript"></script>

<style type="text/css">
    body
    {
        /*background-image:url('images/app-main.png');*/
    }
    #header
    {
        height: 120px;
    }
    
    #main
    {
        text-align:center; 
        margin-left:auto;
        margin-right:auto;
        width:70%;
    }
    
    .columnDiv 
    {
        margin: 0px auto;
        width: 860px;
    }
    .columnDiv ul
    {
        list-style: none;
        margin: 1em 0;
        padding: 0;
    }
    
    .columnDiv ul li
    {
        font-weight: bold;
        margin: 10px;
        padding: 3px 10px 5px 10px;
        color: #666;
        text-align: center;
        border-radius: 10px;
    }
    
    .columnDiv ul li:hover
    {
        color: #000;
        background-color: #ddd;
    }
    
    .columnDiv a
    {
        text-decoration: none;
        color: inherit;
    }
    
    .columnDiv .line
    {
        float: left;
        width: 210px;
    }
    div .percent
    {
        display:inline;
    }
    div .time
    {
        display:inline;
    }
    .green_border
    {
        border: 6px solid #00ff00;
    }
    .red_border
    {
        border: 6px solid #ff0000;
    }

</style>
<script type="text/javascript">
    $(document).ready(function () {
        getStatus();
        setInterval(getStatus, 1000);
    });
    function getStatus() {
        $.ajax({
            url: 'GetLineStatus.ashx?client=txi',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $.each(data, function () {
                    $('li#' + this.line).find(".time").text(this.time);
                    if (this.status == true) {
                        $('li#' + this.line).attr('class', 'green_border');
                    }
                    else {
                        $('li#' + this.line).attr('class', 'red_border');
                    };
                });
            },
            error: function (xhr, textStatus, errorThrown) {
                $('li.green_border').attr('class', 'red_border');
                $('div.time').text(textStatus);
            }
        });
    }
</script>
</head>
<body>
	<div id="header">
        <div><a href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a></div>
    	<div id="logo_content">
        </div>
    </div>
    
    <div id="main">
        <div class="columnDiv">
            <ul id="casting" class="line">
                <li class="red_border" id="Raw_Mill_2"><a href="DCSDemo.aspx?line=Raw_Mill_2">Raw Mill 2</a><br />
                    <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Kiln_2"><a href="DCSDemo.aspx?line=Kiln_2">Kiln 2</a><br />
                    <div class="time">--:--:--</div>
                </li>

             </ul>
          </div>
          <div class="columnDiv">
            <ul id="finishing" class="line">
                <li class="red_border" id="Coal_Mill_C"><a href="DCSDemo.aspx?line=Coal_Mill_C">Coal Mill C</a><br />
                   <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="company-demo"><a href="DCSDemo.aspx?line=company-demo">Finish Mill 3</a><br />
                     <div class="time">--:--:--</div>
                </li>
            </ul>
           </div>
           <div class="columnDiv">
                <ul id="other" class="line">
                <li class="red_border" id="Finish_Mill_1"><a href="DCSDemo.aspx?line=Finish_Mill_1">Finish Mill 1</a><br />
                    <div class="time">--:--:--</div>
                </li>
                <li  class="red_border" id="Cooler_2"><a href="DCSDemo.aspx?line=Cooler_2">Cooler 2</a><br />
                    <div class="time">--:--:--</div>
                </li>
                </ul>
           </div>
        </div>
   
    
</body>
</html>