<?php

// 기본 데이터를 로딩
include "dbHelper.php";

// 값을 전달받아 저장한다.
$userKeyNo = $_REQUEST["userKeyNo"];
$friendUserKeyNo = $_REQUEST["friendUserKeyNo"];
$friendTableNo = $_REQUEST["friendTableNo"];


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


// 서버 데이터상에서 친구인지 체크한다.
$sqlFindFriend = "SELECT * FROM `friendlist`
    WHERE `no` = ".$friendTableNo."
    AND `user` = ".$userKeyNo."
    AND `friend` = ".$friendUserKeyNo."
    AND `state` = 2";
$resultFindFriend = SendSqlQuery($sqlFindFriend, "FindFriend", $mysqli);

// 사용자 데이터를 PHP에서 사용하도록 로딩
$datarow = mysqli_fetch_array($resultFindFriend);

// 필요한 데이터 저장
$nowTime = time();
$lastTime = $datarow["sendTime"];

$spendTime = $nowTime -$lastTime;
if($spendTime < 3596)
{
    //아직 하트를 보낼 수 없는 경우
    echo "reCal".$nowTime.$lastTime;
    $mysqli->close();
    exit();
}

////하트 선물 메시지등록
//$sqlInsertNewHeartMsg = "INSERT INTO `usermessage`(`no`, `user`, `msgType`, `time`)
//    VALUES (null, '".$userKeyNo."', '3', '".$nowTime."')";
//$resultInsertNewHeartMsg = SendSqlQuery($sqlInsertNewHeartMsg, "InsertNewHeartMsg", $mysqli);
//
////하트 선물 시간 업데이트
//$sqlUpdateSendTime = "UPDATE `friendlist` SET `sendTime` = ".$nowTime." WHERE `no` = ".$friendTableNo;
//$resultUpdateSendTime = SendSqlQuery($sqlUpdateSendTime, "UpdateSendTime", $mysqli);

echo "done0";
$mysqli->close();
exit();