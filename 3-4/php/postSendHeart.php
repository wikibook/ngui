<?php
// 기본 데이터를 로딩
include "dbHelper.php";

// 값을 전달받아 저장한다.
$userKeyNo = $_REQUEST["userKeyNo"];
$userID = $_REQUEST["userID"];
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

$sendTime = time();

//send message
$sqlInsertSendHeartMsg = "INSERT INTO `usermessage`(`no`, `user`, `sender`, `senderNo`, `msgType`, `amount`, `time`)
VALUES (null, '".$friendUserKeyNo."', '".$userID."', '".$userKeyNo."', '3', '1', '".$sendTime."')";
$resultInsertSendHeartMsg = SendSqlQuery($sqlInsertSendHeartMsg, "InsertSendHeartMsg", $mysqli);

//보낸 시간업데이트
$sqlUpdateSendTime = "UPDATE `friendlist` SET `sendTime` = '".$sendTime."' WHERE `no` = ".$friendTableNo;
$resultUpdateSendTime = SendSqlQuery($sqlUpdateSendTime, "UpdateSendTime", $mysqli);

echo "done0".$sendTime.time();
$mysqli->close();
exit();