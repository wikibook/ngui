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


// 종류별로 다른 처리가 필요하다.
// 코인, 보석, 하트 (1,2,3)
// 친추 10
switch($msgTypeNo)
{
    case 1:
        $sqlUpdate = "UPDATE `usercore` SET `coins` = `coins` + ".$datarow["amount"]." WHERE `no` = ".$userKeyNo;
        break;
    case 2:
        $sqlUpdate = "UPDATE `usercore` SET `gems` = `gems` + ".$datarow["amount"]." WHERE `no` = ".$userKeyNo;
        break;
    case 3:
        $sqlUpdate = "UPDATE `usercore` SET `hearts` = `hearts` + ".$datarow["amount"]." WHERE `no` = ".$userKeyNo;
        break;
    case 10:
        $sqlUpdate = "UPDATE  `friendlist` SET  `state` =2
        WHERE (`user` =".$userKeyNo." AND `friend` =".$datarow["senderNo"].")
        OR (`user` =".$datarow["senderNo"]." AND  `friend` =".$userKeyNo.")";
        break;
}
$resultUpdate = SendSqlQuery($sqlUpdate, "Update", $mysqli);

// 메시지삭제
$sqlDeleteMsg = "DELETE FROM `usermessage` WHERE `no` = ".$msgTableKeyNo;
$resultDeleteMsg = SendSqlQuery($sqlDeleteMsg, "DeleteMsg", $mysqli);

//결과처리
if($msgTypeNo <  10)
{
    $sqlUserData = "SELECT * FROM `usercore` WHERE `no` = ".$userKeyNo;
    $resultUserData = SendSqlQuery($sqlUserData, "UserData", $mysqli);
    $dataUser = mysqli_fetch_array($resultUserData);

    switch($msgTypeNo)
    {
        case 1:
            echo "done0".$dataUser["coins"];
            break;
        case 2:
            echo "done0".$dataUser["gems"];
            break;
        case 3:
            echo "done0".$dataUser["hearts"];
            break;
    }
    $mysqli->close();
    exit();
}
else
{
    echo "done0".$datarow["sender"];
    $mysqli->close();
    exit();
}




