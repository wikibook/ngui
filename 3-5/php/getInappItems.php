<?php

//
include "dbHelper.php";

//
$userKeyNo = $_REQUEST["userKeyNo"];


//
$mysqli = new mysqli($host, $user, $password, $dbname);
if (mysqli_connect_errno())
{
    echo 'Connect faild : '.mysqli_connect_error().'\n';
    $mysqli->close();
    exit();
}
//
$mysqli->set_charset("utf8");


//
$sqlFindInappItem = "SELECT * FROM `inappitem` ORDER BY `no`";
//
$resultInappItem = SendSqlQuery($sqlFindInappItem, "InappItem", $mysqli);


//
$dom = new DOMDocument('1.0', 'UTF-8');
//root element
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);
$response = $dom->createElement("inapp");
$rootNode->appendChild($response);


//db php
while($datarow = mysqli_fetch_array($resultInappItem))
{
    $dataNode = $dom->createElement('InappItemData');

    $no = nodeMaker($dom, "no", $datarow["no"]);
    $codeNo = nodeMaker($dom, "codeNo", $datarow["codeNo"]);
    $sku = nodeMaker($dom, "sku", $datarow["sku"]);


    $dataNode->appendChild($no);
    $dataNode->appendChild($codeNo);
    $dataNode->appendChild($sku);

    $response->appendChild($dataNode);
}

//
header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();
?>