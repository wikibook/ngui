<?php

// 기본 데이터를 로딩
include "dbHelper.php";

// 사용자의 usercore 테이블 키 값을 전달받아 저장한다.
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


// 사용자 데이터를 요청
$sqlFindUserData = "SELECT * FROM `usercore` WHERE `no` = ".$userKeyNo;
$resultFindUserData = SendSqlQuery($sqlFindUserData, "FindUserData", $mysqli);

// 사용자 데이터를 PHP에서 사용하도록 로딩
$datarow = mysqli_fetch_array($resultFindUserData);

// 필요한 데이터 저장
$nowTime = time();
$lastTime = $datarow["loginTime"];
$nowHearts = $datarow["hearts"];

if($nowHearts < 5)
{
    $spendTime = $nowTime - $lastTime;
    if($spendTime >= 595 && $spendTime <= 599 )
    {
        $spendTime = 600;
    }
    $totalHearts = $nowHearts + floor($spendTime / 600);

    if($totalHearts > 5)
    {
        $totalHearts = 5;
    }

    if($totalHearts != $nowHearts)
    {
        $sqlUpdateHearts = "UPDATE `usercore`
            SET `hearts` = '".$totalHearts."', `loginTime` = '".$nowTime."'
            WHERE `no` = ".$userKeyNo;
        $resultUpdateHearts = SendSqlQuery($sqlUpdateHearts, "UpdateHearts", $mysqli);
        $lastTime = $nowTime;
        $nowHearts = $totalHearts;
    }
}

//server time //lastTime //heart 보낸다.
// 사용자 데이터가 존재하므로 리턴할 XML 데이터 구조 생성
$dom = new DOMDocument('1.0', 'UTF-8');
//root 노드 생성
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);
$response = $dom->createElement("result");
$rootNode->appendChild($response);

$serverTime = nodeMaker($dom, "serverTime", $nowTime);
$loginTime = nodeMaker($dom, "loginTime", $lastTime);
$hearts = nodeMaker($dom, "hearts", $nowHearts);

$response->appendChild($hearts);
$response->appendChild($loginTime);
$response->appendChild($serverTime);

// 반환되는 데이터의 헤더에결과값이 XML 형태라는 것을 명시
header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
// 결과 XML 반환
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();

?>