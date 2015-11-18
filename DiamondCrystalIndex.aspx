<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DiamondCrystalIndex.aspx.cs" Inherits="DCSDemoData.DiamondCrystalIndex" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Diamond Crystal Dashboard</title>
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
        getPercent();
        setInterval(getStatus, 1000);
        setInterval(getPercent, 300000);
    });
    function getPercent() {
        $.ajax({
            url: 'GetLinePercent.ashx?client=diamondcrystal',
            type: 'GET',
            dataType: 'json',
            data: { time: new Date().format('yyyy/MM/dd HH:mm:ss') },
            success: function (data) {
                $.each(data, function () {
                    $('li#' + this.line).find(".percent").text(this.percent);
                });
            },
            error: function (xhr, textStatus, errorThrown) {
                $('div.percent').text('--%');
            }
        });
    }
    function getStatus() {
        $.ajax({
            url: 'GetLineStatus.ashx?client=diamondcrystal',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $.each(data, function () {
                    $('li#' + this.line).find(".time").text(this.time);
                    if (this.status == false) {
                        $('li#' + this.line).attr('class', 'red_border');
                    }
                    else {
                        $('li#' + this.line).attr('class', 'green_border');
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
            <ul id="sugar" class="line">
                <li style="color: Black;"><b>Sugar</b></li>
                <li class="red_border" id="Ropak10"><a href="DCSDemo.aspx?line=Ropak10">Ropak 10</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Ropak11"><a href="DCSDemo.aspx?line=Ropak11">Ropak 11</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Ropak12"><a href="DCSDemo.aspx?line=Ropak12">Ropak 12</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Ropak13"><a href="DCSDemo.aspx?line=Ropak13">Ropak 13</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Ropak14"><a href="DCSDemo.aspx?line=Ropak14">Ropak 14</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Ropak15"><a href="DCSDemo.aspx?line=Ropak15">Ropak 15</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Ropak16"><a href="DCSDemo.aspx?line=Ropak16">Ropak 16</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Ropak17"><a href="DCSDemo.aspx?line=Ropak17">Ropak 17</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>

             </ul>
          </div>
        <div class="columnDiv">
            <ul id="speciality" class="line">
                <li style="color: Black;"><b>Speciality</b></li>
                <li class="red_border" id="Splenda1"><a href="DCSDemo.aspx?line=Splenda1">Splenda1</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Splenda2"><a href="DCSDemo.aspx?line=Splenda2">Splenda2</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Splenda3"><a href="DCSDemo.aspx?line=Splenda3">Splenda3</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Splenda4"><a href="DCSDemo.aspx?line=Splenda4">Splenda4</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Splenda5"><a href="DCSDemo.aspx?line=Splenda5">Splenda5</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Cloud"><a href="DCSDemo.aspx?line=Cloud">Cloud</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Jones"><a href="DCSDemo.aspx?line=Jones">Jones</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="NDC_Ropak"><a href="DCSDemo.aspx?line=NDC_Ropak">NDC Ropak</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="CRP"><a href="DCSDemo.aspx?line=CRP">CRP</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Cheese_Ropak"><a href="DCSDemo.aspx?line=Cheese_Ropak">Cheese Ropak</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
             </ul>
          </div>
        <div class="columnDiv">
            <ul id="sps" class="line">
                <li style="color: Black;"><b>Sps</b></li>
                <li class="red_border" id="Sps3"><a href="DCSDemo.aspx?line=Sps3">SPS 3</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps4"><a href="DCSDemo.aspx?line=Sps4">SPS 4</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps5"><a href="DCSDemo.aspx?line=Sps5">SPS 5</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps6"><a href="DCSDemo.aspx?line=Sps6">SPS 6</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps7"><a href="DCSDemo.aspx?line=Sps7">SPS 7</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps8"><a href="DCSDemo.aspx?line=Sps8">SPS 8</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps9"><a href="DCSDemo.aspx?line=Sps9">SPS 9</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps10"><a href="DCSDemo.aspx?line=Sps10">SPS 10</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Sps11"><a href="DCSDemo.aspx?line=Sps11">SPS 11</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Salt_Ropak"><a href="DCSDemo.aspx?line=Salt_Ropak">Salt Ropak</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Salt_Ropak2"><a href="DCSDemo.aspx?line=Salt_Ropak2">Salt Ropak 2</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Pepper_Ropak"><a href="DCSDemo.aspx?line=Pepper_Ropak">Pepper Ropak</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
                <li class="red_border" id="Combo_Ropak"><a href="DCSDemo.aspx?line=Combo_Ropak">Combo Ropak</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
             </ul>
          </div>
        <div class="columnDiv">
            <ul id="Shaker_Lines" class="line">
                <li style="color: Black;"><b>Shaker Line</b></li>
                <li class="red_border" id="Shaker"><a href="DCSDemo.aspx?line=Shaker">Shaker</a><br />
                    <div class="percent">--%</div>  <div class="time">--:--:--</div>
                </li>
             </ul>
          </div>
        </div>
   
    
</body>
</html>