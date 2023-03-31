<?php
session_start();

$tologin = true;

if(isset($_SESSION['product'])) {
    if ($_SESSION['product'] != "")
	{
		$tologin = false;
	}
}

if ($tologin==true)
{
		header( 'Location: login' );
		exit();
}


$dir = "./" . 	$_SESSION["product"] . "/";
chdir ($dir);
array_multisort (array_map ('filemtime', ($files = glob ("*.*"))), SORT_DESC, $files);
$table .= "<table>";
foreach ($files as $filename)
{
 $table .=  "<tr>";
 $table .= "<td><a href='" . $dir . $filename . "'>" . $filename . "</a></td><td>" . filesize($filename) . "</td> <td>" . date ("F d Y H:i:s", filemtime($filename)) . "</td>";
 $table .=  "<tr>";
}

$table .=  "</ table>";

echo $table;   


 
//Abfrage der Nutzer ID vom Login
$userid = $_SESSION['userid'];
 
echo "Hallo User: ".$userid;
?>