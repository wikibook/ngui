<?php
// 기본 데이터를 로딩
include "dbHelper.php";

// 값을 전달받아 저장한다.
$userKeyNo = $_REQUEST["userKeyNo"];
$msgTableKeyNo = $_REQUEST["msgTableKeyNo"];
$msgTypeNo = $_REQUEST["msgTypeNo"];


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


$sqlFindMsg = "SELECT * FROM `usermessage`
    WHERE `no` = ".$msgTableKeyNo." AND `user` = ".$userKeyNo." AND `msgType` = ".$msgTypeNo;
$resultFindMsg = SendSqlQuery($sqlFindMsg, "FindMsg", $mysqli);

if($resultFindMsg->num_rows<1)
{
    // error : 메시지가 존재하지않는 경우
    echo "none0";
    $mysqli->close();
    exit();
}

$datarow = mysqli_fetch_array($resultFindMsg);


// 친구 리시트에서 삭제
$sqlDelete = "DELETE FROM `friendlist`
        WHERE (`user` =".$userKeyNo." AND `friend` =".$datarow["senderNo"].")
        OR (`user` =".$datarow["senderNo"]." AND  `friend` =".$userKeyNo.")";
$resultDelete = SendSqlQuery($sqlDelete, "Delete", $mysqli);

// 메시지삭제
$sqlDeleteMsg = "DELETE FROM `usermessage` WHERE `no` = ".$msgTableKeyNo;
$resultDeleteMsg = SendSqlQuery($sqlDeleteMsg, "DeleteMsg", $mysqli);

//결과처리

echo "done0";
$mysqli->close();
exit();




