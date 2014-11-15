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
$sqlFindFriend = "SELECT `friendlist`.*, `usercore`.`id` AS `name`, `usercore`.`highScore` AS `score`, `usercore`.`facebook` AS `facebook`
    FROM `friendlist`, `usercore`
    WHERE `friendlist`.`user` = ".$userKeyNo." AND `usercore`.`no` = `friendlist`.`friend`";
//
$resultFindFriend = SendSqlQuery($sqlFindFriend, "FindFriend", $mysqli);


//
$dom = new DOMDocument('1.0', 'UTF-8');
//root element
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);
$response = $dom->createElement("friendlist");
$rootNode->appendChild($response);


//db php
while($datarow = mysqli_fetch_array($resultFindFriend))
{
    $dataNode = $dom->createElement('FriendData');

    $no = nodeMaker($dom, "no", $datarow["no"]);
    $friend = nodeMaker($dom, "friend", $datarow["friend"]);
    $name = nodeMaker($dom, "name", $datarow["name"]);
    $score = nodeMaker($dom, "score", $datarow["score"]);
    $state = nodeMaker($dom, "state", $datarow["state"]);
    $sendTime = nodeMaker($dom, "sendTime", $datarow["sendTime"]);
    $facebook = nodeMaker($dom, "facebook", $datarow["facebook"]);

    $dataNode->appendChild($no);
    $dataNode->appendChild($friend);
    $dataNode->appendChild($name);
    $dataNode->appendChild($score);
    $dataNode->appendChild($friend);
    $dataNode->appendChild($state);
    $dataNode->appendChild($sendTime);
    $dataNode->appendChild($facebook);

    $response->appendChild($dataNode);
}

//
header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();
?>