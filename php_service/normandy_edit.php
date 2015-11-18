<?php


header("Access-Control-Allow-Origin: *");







if( $_GET['op'] == 'update_edit_data' ){

	$id = $_GET['id'];
	$scale = $_GET['scale'];
	$time = date("Y-m-d H:i:s", strtotime(str_replace("::", ":", $_GET['time'])));
	$type = $_GET['type'];
	$line = $_GET['line'];
	$netwt = $_GET['netwt'];
	$shift = $_GET['shift'];

	$DBH = new PDO('sqlsrv:server=feedcomm.db.4956410.hostedresource.com;Database=feedcomm','Feedcomm','Dashboard1');



	$STH = $DBH->query("update Gais set Scale = '$scale', Time = '$time', Type = '$type', Line = $line, NetWt = $netwt, Shift = $shift where ID = $id;");
 
	# setting the fetch mode
	
	if ( $STH === false) 
   	{ die( sqlsrv_errors() ); }


	print '
Success';

}
elseif(  $_GET['op'] == 'get_gais_data'  ){

	//$start_date = date( "Y-m-d H:i:s", strtotime(str_replace("::", ":", $_GET['startDate'])));
	//$end_date = date( "Y-m-d H:i:s", strtotime(str_replace("::", ":", $_GET['endDate'])));


	$DBH = new PDO('sqlsrv:server=feedcomm.db.4956410.hostedresource.com;Database=feedcomm','Feedcomm','Dashboard1');
	
	//print "select * from Gais where Time >= '$start_date' and Time <= '$end_date' and Location = 'MILLARD' order by Time desc;";
	//$STH = $DBH->query("select * from Gais where Time >= '$start_date' and Time <= '$end_date' and Location = 'MILLARD' order by Time desc;");
    
	$STH = $DBH->query("select top 400 * from Gais where Location = 'FRZWLR' order by Time desc;");
	
	# setting the fetch mode
	$STH->setFetchMode(PDO::FETCH_ASSOC);	
	$adder = '<?xml version="1.0" encoding="UTF-8"?>'."\n";
	$adder .= "<Data>\n";

	while($row = $STH->fetch()) {
    		$adder .= "\t<Element>";
    		$adder .= "\t<Scale>".$row['Scale']. "</Scale>\n";
    		$adder .= "\t<Time>".date ( "Y-m-d H:i:sA", strtotime($row['Time']) ) . "</Time>\n";
		$adder .= "\t<Type>".$row['Type']."</Type>\n";
    		$adder .= "\t<Line>".$row['Line'] . "</Line>\n";
    		$adder .= "\t<NetWt>".$row['NetWt'] . "</NetWt>\n";
    		$adder .= "\t<Shift>".$row['Shift'] . "</Shift>\n";
    		$adder .= "\t<Location>".$row['Location'] . "</Location>\n";
    		$adder .= "\t<ID>".$row['ID'] . "</ID>\n";
    		$adder .= "\t</Element>";
	}
	$adder .= "</Data>";

	print $adder;

}
else{

	$start_date = date( "Y-m-d H:i:s", strtotime(str_replace("::", ":", $_GET['startDate'])));
	$end_date = date( "Y-m-d H:i:s", strtotime(str_replace("::", ":", $_GET['endDate'])));


	$DBH = new PDO('sqlsrv:server=feedcomm.db.4956410.hostedresource.com;Database=feedcomm','Feedcomm','Dashboard1');


	
	//print "select * from Gais where Time >= '$start_date' and Time <= '$end_date' and Location = 'MILLARD' order by Time desc;";
	$STH = $DBH->query("select * from Gais where Time >= '$start_date' and Time <= '$end_date' and Location = 'MILLARD' order by Time desc;");
 
	# setting the fetch mode
	$STH->setFetchMode(PDO::FETCH_ASSOC);	
	$adder = '<?xml version="1.0" encoding="UTF-8"?>'."\n";
	$adder .= "<Data>\n";

	while($row = $STH->fetch()) {
    		$adder .= "\t<Element>";
    		$adder .= "\t<Scale>".$row['Scale']. "</Scale>\n";
    		$adder .= "\t<Time>".date ( "Y-m-d H:i:sA", strtotime($row['Time']) ) . "</Time>\n";
		$adder .= "\t<Type>".$row['Type']."</Type>\n";
    		$adder .= "\t<Line>".$row['Line'] . "</Line>\n";
    		$adder .= "\t<NetWt>".$row['NetWt'] . "</NetWt>\n";
    		$adder .= "\t<Shift>".$row['Shift'] . "</Shift>\n";
    		$adder .= "\t<ID>".$row['ID'] . "</ID>\n";
    		$adder .= "\t</Element>";
	}
	$adder .= "</Data>";

	print $adder;

}


?>