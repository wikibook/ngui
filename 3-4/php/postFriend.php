<?php
// 기본 데이터를 로딩
include "dbHelper.php";

// 값을 전달받아 저장한다.
$userKeyNo = $_REQUEST["userKeyNo"];
$userID = $_REQUEST["userID"];
$friendID = $_REQUEST["friendID"];


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


// $friendID 와 같은 아이디가 실제로 존재하는지 체크.
$sqlFindUser = "SELECT * FROM `usercore` WHERE `id` = '".$friendID."'";
$resultFindUser = SendSqlQuery($sqlFindUser, "FindUser", $mysqli);

if($resultFindUser->num_rows < 1)
{
    echo "none0";
    $mysqli->close();
    exit();
}

// 친구가 될 사용자의 정보 변환.
$dataCore = mysqli_fetch_array($resultFindUser);
$friendKeyNo = $dataCore["no"];

//이미 친구이거나 친구요청을 보내거나받았는지 체크.
$sqlFindFriendRequest
    = "SELECT * FROM `friendlist`
    WHERE `user` = ".$userKeyNo."
    AND `friend` = ".$friendKeyNo;
$resultFindFriendRequest
    = SendSqlQuery($sqlFindFriendRequest, "FindFriendRequest", $mysqli);

if($resultFindFriendRequest->num_rows > 0)
{
    // 이미 친구 이거나 친구 요청등을 보낸 경우.
    $dataFriend = mysqli_fetch_array($resultFindFriendRequest);
    $state = $dataFriend["state"];
    echo "have".$state;
    $mysqli->close();
    exit();
}
else
{
    // 새로운 친구로 등록하기위한 과정 진행.
    $sendTime = time();
    $sqlInsertFriendRequest
        = "INSERT INTO `friendlist`(`no`, `user`, `friend`, `state`, `sendTime`)
        VALUES (null, ".$userKeyNo.", ".$friendKeyNo.", 0, ".$sendTime."),
        (null, ".$friendKeyNo.", ".$userKeyNo.", 1, ".$sendTime.")";
    $resultInsertFriendRequest
        = SendSqlQuery($sqlInsertFriendRequest, "InsertFriendRequest", $mysqli);

    //친구요청 메시지 등록.
    $sqlInsertMsg
        = "INSERT INTO `usermessage`(`no`, `user`, `sender`, `senderNo`, `msgType`, `time`)
        VALUES (null, ".$friendKeyNo.", '".$userID."', '".$userKeyNo."', 10, ".$sendTime.")";
    $resultInsertMsg = SendSqlQuery($sqlInsertMsg, "InsertMsg", $mysqli);

    echo "done0";
    $mysqli->close();
}