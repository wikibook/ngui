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
$sqlFindMsg = "SELECT * FROM `usermessage` WHERE `user` = ".$userKeyNo;
$resultFindMsg = SendSqlQuery($sqlFindMsg, "FindMsg", $mysqli);


//
$dom = new DOMDocument('1.0', 'UTF-8');
//root element
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);
$response = $dom->createElement("usermessage");
$rootNode->appendChild($response);


//db php
while($datarow = mysqli_fetch_array($resultFindMsg))
{
    $dataNode = $dom->createElement('MessageData');

    $no = nodeMaker($dom, "no", $datarow["no"]);
    $sender = nodeMaker($dom, "sender", $datarow["sender"]);
    $msgType = nodeMaker($dom, "msgType", $datarow["msgType"]);
    $amount = nodeMaker($dom, "amount", $datarow["amount"]);

    $dataNode->appendChild($no);
    $dataNode->appendChild($sender);
    $dataNode->appendChild($msgType);
    $dataNode->appendChild($amount);

    $response->appendChild($dataNode);
}

//
header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();
?>