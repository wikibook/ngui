<?php

// 기본 데이터를 로딩
include "dbHelper.php";

// usercore 테이블 행번호 저장
$userKeyNo = $_REQUEST["userKeyNo"];
$getCoins = $_REQUEST["getCoins"];
$nowScore = $_REQUEST["score"];


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


// 사용자 데이터를 요청
$sqlFindUserData = "SELECT * FROM `usercore` WHERE `no` = ".$userKeyNo;
$resultFindUserData = SendSqlQuery($sqlFindUserData, "FindUserData", $mysqli);
$dataUserCore = mysqli_fetch_array($resultFindUserData);

$resultScore = $dataUserCore["highScore"];

$sqlUpdate = "UPDATE `usercore` SET `coins` = `coins` + ".$getCoins;
if($nowScore > $dataUserCore["highScore"])
{
    $sqlUpdate = $sqlUpdate.", `highScore` = ".$nowScore;
    $resultScore = $nowScore;
}

$sqlUpdate = $sqlUpdate." WHERE `no` = ".$userKeyNo;
$resultUpdate = SendSqlQuery($sqlUpdate, "Update", $mysqli);

$resultCoinsValue = $dataUserCore["coins"] + $getCoins;
$resultCoins = str_pad($resultCoinsValue, 7, "0", STR_PAD_LEFT);
echo 'done0'.$resultCoins.$resultScore;
$mysqli->close();
exit();