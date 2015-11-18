<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TXI.aspx.cs" Inherits="TXI" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>TXI Dashboard</title>
<link href="plugins/jquery-ui-1.8.2.custom.css" rel="stylesheet" type="text/css" />
<link href='http://fonts.googleapis.com/css?family=Fjalla+One' rel='stylesheet' type='text/css'>
<link href="TXI.css" rel="stylesheet" type="text/css" />
<link rel="icon" href="images/TXIFavicon.png" type="image/png">
<script src="Scripts/jquery-1.4.1.min.js" type="text/javascript"></script>
<script src="Scripts/jquery.jmodal.js" type="text/javascript"></script>
<script src="plugins/jquery-ui-1.8.2.custom.min.js" type="text/javascript"></script>
<script src="Scripts/global.js" type="text/javascript"></script>

<script type="text/javascript">
    $(document).ready(function () {
        getStatus();
        getKPIs();

        setInterval(getStatus, 1000);
        setInterval(getKPIs, 1000 * 60);
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

    function calculatePercent(val) {
        if (!isNaN(val)) {
            return (val * 100).toFixed(2) + "%";
        }

        return "0.00%";
    }

    function getKPIs() {
        $.ajax({
            url: 'GetKPI.ashx',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                $.each(data, function (line, val) {
                    var tbl = $('#tbl' + line);

                    tbl.find(".mtd .mtbf").text(val.mtd.MTBF.toFixed(2));
                    tbl.find(".mtd .rf").text(calculatePercent(val.mtd.RF));
                    tbl.find(".mtd .uf").text(calculatePercent(val.mtd.UF));

                    tbl.find(".qtd .mtbf").text(val.qtd.MTBF.toFixed(2));
                    tbl.find(".qtd .rf").text(calculatePercent(val.qtd.RF));
                    tbl.find(".qtd .uf").text(calculatePercent(val.qtd.UF));

                    tbl.find(".ytd .mtbf").text(val.ytd.MTBF.toFixed(2));
                    tbl.find(".ytd .rf").text(calculatePercent(val.ytd.RF));
                    tbl.find(".ytd .uf").text(calculatePercent(val.ytd.UF));
                    console.log(line);

                });
            },
            error: function (xhr, textStatus, errorThrown) {
                $('div.mtd').text(0);
                $('div.qtd').text(0);
                $('div.ytd').text(0);
            }
        });
    }
</script>
</head>
<body>
	<div id="header">
        <!--<div id="editcode"><a href="DCSDemoReasonCodes.aspx">Edit Reason Codes</a></div>-->
    	<div id="logo_content">
        </div>
    </div>
	<div id="main">
		<div class="columnDiv">
			<ul id="casting" class="line">
				<li class="red_border" id="Raw_Mill_2"><a href="DCSDemo.aspx?line=Raw_Mill_2">Raw Mill 2</a><br />
					<div class="time">--:--:--</div>
					<div class="table" id="tblRaw_Mill_2">
						<table>
							<tr>
								<td></td>
								<td><strong>MTBF</strong></td>
								<td><strong>RF</strong></td>
								<td><strong>UF</strong></td>
							</tr>
							<tr class="mtd">
								<td><strong>MTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="qtd">
								<td><strong>QTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="ytd">
								<td><strong>YTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
						</table>
					</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Raw_Mill_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Raw_Mill_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Raw_Mill_2" target="_blank">RC</a>
					</div>
				</li>
				<li class="red_border" id="Kiln_2"><a href="DCSDemo.aspx?line=Kiln_2">Kiln 2</a><br />
					<div class="time">--:--:--</div>
					<div class="table" id="tblKiln_2">
						<table>
							<tr>
								<td></td>
								<td><strong>MTBF</strong></td>
								<td><strong>RF</strong></td>
								<td><strong>UF</strong></td>
							</tr>
							<tr class="mtd">
								<td><strong>MTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="qtd">
								<td><strong>QTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="ytd">
								<td><strong>YTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
						</table>
					</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Kiln_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Kiln_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Kiln_2" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="finishing" class="line">
				<li class="red_border" id="Coal_Mill_C"><a href="DCSDemo.aspx?line=Coal_Mill_C">Coal Mill C</a><br />
					<div class="time">--:--:--</div>
					<div class="table" id="tblCoal_Mill_C">
						<table>
							<tr>
								<td></td>
								<td><strong>MTBF</strong></td>
								<td><strong>RF</strong></td>
								<td><strong>UF</strong></td>
							</tr>
							<tr class="mtd">
								<td><strong>MTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="qtd">
								<td><strong>QTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="ytd">
								<td><strong>YTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
						</table>
					</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Coal_Mill_C" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Coal_Mill_C" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Coal_Mill_C" target="_blank">RC</a>
					</div>
				</li>
				<li class="red_border" id="Finish_Mill_3"><a href="DCSDemo.aspx?line=Finish_Mill_3">Finish Mill 3</a><br />
					<div class="time">--:--:--</div>
					<div class="table" id="tblFinish_Mill_3">
						<table>
							<tr>
								<td></td>
								<td><strong>MTBF</strong></td>
								<td><strong>RF</strong></td>
								<td><strong>UF</strong></td>
							</tr>
							<tr class="mtd">
								<td><strong>MTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="qtd">
								<td><strong>QTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="ytd">
								<td><strong>YTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
						</table>
					</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Finish_Mill_3" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Finish_Mill_3" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Finish_Mill_3" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div class="columnDiv">
			<ul id="other" class="line">
				<li class="red_border" id="Finish_Mill_1"><a href="DCSDemo.aspx?line=Finish_Mill_1">Finish Mill 1</a><br />
					<div class="time">--:--:--</div>
					<div class="table" id="tblFinish_Mill_1">
						<table>
							<tr>
								<td></td>
								<td><strong>MTBF</strong></td>
								<td><strong>RF</strong></td>
								<td><strong>UF</strong></td>
							</tr>
							<tr class="mtd">
								<td><strong>MTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="qtd">
								<td><strong>QTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="ytd">
								<td><strong>YTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
						</table>
					</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Finish_Mill_1" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Finish_Mill_1" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Finish_Mill_1" target="_blank">RC</a>
					</div>
				</li>
				<li  class="red_border" id="Cooler_2"><a href="DCSDemo.aspx?line=Cooler_2">Cooler 2</a><br />
					<div class="time">--:--:--</div>
					<div class="table" id="tblCooler_2">
						<table>
							<tr>
								<td></td>
								<td><strong>MTBF</strong></td>
								<td><strong>RF</strong></td>
								<td><strong>UF</strong></td>
							</tr>
							<tr class="mtd">
								<td><strong>MTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="qtd">
								<td><strong>QTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
							<tr class="ytd">
								<td><strong>YTD</strong></td>
								<td class="mtbf">0</td>
								<td class="rf">0.00%</td>
								<td class="uf">0.00%</td>
							</tr>
						</table>
					</div>
					<div class="buttons">
						<a href="DCSDemo.aspx?line=Cooler_2" target="_blank">OI</a>
						<a href="DCSDashboard.aspx?line=Cooler_2" target="_blank">DB</a>
						<a href="DCSDemoReasonCodes.aspx?line=Cooler_2" target="_blank">RC</a>
					</div>
				</li>
			</ul>
		</div>
		<div id="thrive">
			<a target="_blank" href="/"><img src="images/txithrive.png" alt="Thrive DCS" /></a>
		</div>
	</div>
</body>
</html>