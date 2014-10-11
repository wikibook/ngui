<?php
// 기본 데이터를 로딩
include "dbHelper.php";

// 값을 전달받아 저장한다.
$userKeyNo = $_REQUEST["userKeyNo"];

// 데이터베이스에 접속
$mysqli = new mysqli($host, $user, $password, $dbname);
if (mysqli_connect_errno())
{
    echo 'Connect faild : '.mysqli_connect_error().'\n';
    $mysqli->close();
    exit();
}
// utf8로 charset 변경
$mysqli->set_charset("utf8");

// 사용자의 전체 친구리스트 로딩하여 제외리스트 생성.
$sqlFindAllFriend = "SELECT * FROM `friendlist` WHERE `user` = ".$userKeyNo;
$resultFindAllFriend = SendSqlQuery($sqlFindAllFriend, "FindAllFriend", $mysqli);

$removeList = "(".$userKeyNo;
while($datarow = mysqli_fetch_array($resultFindAllFriend))
{
    $removeList = $removeList.", ".$datarow["friend"];
}
$removeList = $removeList.")";


// $removeList에 있는 유저를 제외하고 3개 선택.
$sqlFindRecommendedFriend
    = "SELECT * FROM `usercore` WHERE `no` NOT IN ".$removeList."
    ORDER BY RAND() LIMIT 0, 3";
$resultFindRecommenedFriend
    = SendSqlQuery($sqlFindRecommendedFriend, "FindRecommendedFriend", $mysqli);


// 결과 생성
$dom = new DOMDocument('1.0', 'UTF-8');
//root 노드 생성
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);
$response = $dom->createElement("recommendFriend");
$rootNode->appendChild($response);

while($datarow=mysqli_fetch_array($resultFindRecommenedFriend))
{
    $dataNode = $dom->createElement('RecommendedData');

    $name = nodeMaker($dom, "name", $datarow["id"]);
    $dataNode->appendChild($name);

    $response->appendChild($dataNode);
}

header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();
?>