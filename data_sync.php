<?php

phpinfo();

ini_set('display_errors', 'On');

//$c = new PDO("sqlsrv:Server=localhost;Database=thrivedcs", "thrivedcs", "WillThrive1");
//$sync_r = mssql_connect("localhost", "thrivedcs", "WillThrive1");
$sync_r = msql_connect();
print $sync_r;
print "test.php";

?>