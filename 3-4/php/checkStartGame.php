<?php

// 기본 데이터를 로딩
include "dbHelper.php";

// usercore 테이블 행번호 저장
$userKeyNo = $_REQUEST["userKeyNo"];
$resurrection = $_REQUEST["resurrection"];


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



$resultResurrectionCount = 0;
$resultResurrectionUse = 0;
// 즉시 부활 사용여부 업데이트.
if($resurrection > -1)
{
    $sqlFindRessurrection = "SELECT * FROM `useritem` WHERE `user` = ".$userKeyNo." AND `itemNo` = 302";
    $resultFindRessurrection = SendSqlQuery($sqlFindRessurrection, "FindRessurrection", $mysqli);
    $dataFindRessurrection = mysqli_fetch_array($resultFindRessurrection);

    if($resurrection == 1)
    {
        $resultResurrectionCount =$dataFindRessurrection["amount"] -1;

        if($resultResurrectionCount <= 0)
        {
            $resultResurrectionCount = 0;
            $resultResurrectionUse = 0;
        }
        else
        {
            $resultResurrectionUse = $resurrection;
        }
    }
    else
    {
        $resultResurrectionCount = $dataFindRessurrection["amount"];
        $resultResurrectionUse = 0;
    }



    $sqlUpdateRessurrection = "UPDATE `useritem` SET `use` = ".$resultResurrectionUse.", `amount` = ".$resultResurrectionCount." WHERE `no` = ".$dataFindRessurrection["no"];
    $resultUpdateRessurrection = SendSqlQuery($sqlUpdateRessurrection, "UpdateRessurrection", $mysqli);
}


// 사용자 데이터를 요청
$sqlFindUserData = "SELECT * FROM `usercore` WHERE `no` = ".$userKeyNo;
$resultFindUserData = SendSqlQuery($sqlFindUserData, "FindUserData", $mysqli);
$dataUserCore = mysqli_fetch_array($resultFindUserData);

// 하트 보유 체크.
if($dataUserCore["hearts"] < 1)
{
    echo "none0";
    $mysqli->close();
    exit();
}

$returnTime = $dataUserCore["loginTime"];
$resultValue = $dataUserCore["hearts"] - 1;

// 하트 1개 차감.
$sqlUpdateHeart = "UPDATE `usercore` SET `hearts` = ".$resultValue;
if($resultValue == 4)
{
    $nowTime = time();
    $returnTime = $nowTime;
    $sqlUpdateHeart = $sqlUpdateHeart.", `loginTime` = ".$nowTime;
}
$sqlUpdateHeart = $sqlUpdateHeart." WHERE `no` = ".$userKeyNo;
$resultUpdateHeart = SendSqlQuery($sqlUpdateHeart, "UpdateHeart", $mysqli);

$returnTime = str_pad($resultValue, 10, "0", STR_PAD_LEFT);
$resultHeart = str_pad($resultValue, 3, "0", STR_PAD_LEFT);
echo "done0".$resultHeart.$returnTime.$resultResurrectionUse.$resultResurrectionCount;
$mysqli->close();
exit();