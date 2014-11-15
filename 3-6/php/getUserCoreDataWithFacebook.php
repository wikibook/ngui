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

if($resultFindUserData->num_rows < 1)
{
    //사용자 데이터가 존재하지 않는 경우. 에러 “none0” 반환 후 종료
    echo "none0";
    $mysqli->close();
    exit();
}

// 사용자 데이터를 PHP에서 사용하도록 로딩
$datarow = mysqli_fetch_array($resultFindUserData);

// 필요한 데이터 저장
$nowTime = time();
$lastTime = $datarow["loginTime"];
$nowHearts = $datarow["hearts"];

// 하트가 5보다 작은 경우
// 10분(600초)마다 1개의 하트가 지급되므로
// 로그인 시간차이를 계산하여 하트를 추가 지급한다.
if($nowHearts < 5)
{
    $spendTime = $nowTime - $lastTime;
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
else
{
    $sqlUpdateTime = "UPDATE `usercore`
            SET `loginTime` = '".$nowTime."'
            WHERE `no` = ".$userKeyNo;
    $resultUpdateTime = SendSqlQuery($sqlUpdateTime, "UpdateHearts", $mysqli);
    $lastTime = $nowTime;
}

// 사용자 데이터가 존재하므로 리턴할 XML 데이터 구조 생성
$dom = new DOMDocument('1.0', 'UTF-8');
//root 노드 생성
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);
$response = $dom->createElement("usercore");
$rootNode->appendChild($response);


// 사용자 데이터를 각각 노드로 생성 후 등록
$id = nodeMaker($dom, "ID", $datarow["id"]);
$gems = nodeMaker($dom, "gems", $datarow["gems"]);
$coins = nodeMaker($dom, "coins", $datarow["coins"]);
$hearts = nodeMaker($dom, "hearts", $nowHearts);
$highScore = nodeMaker($dom, "highScore", $datarow["highScore"]);
$loginTime = nodeMaker($dom, "loginTime", $lastTime);
$serverTime = nodeMaker($dom, "serverTime", $nowTime);
$facebook = nodeMaker($dom, "facebook", $datarow["facebook"]);

$response->appendChild($id);
$response->appendChild($gems);
$response->appendChild($coins);
$response->appendChild($hearts);
$response->appendChild($highScore);
$response->appendChild($loginTime);
$response->appendChild($serverTime);
$response->appendChild($facebook);


// userupgrade 테이블 데이터 요청
$sqlFindUserUpgrade = "SELECT * FROM `userupgrade` WHERE `user` = ".$userKeyNo;
$resultFindUserUpgrade = SendSqlQuery($sqlFindUserUpgrade, "FindUserUpgrade", $mysqli);

if($resultFindUserUpgrade->num_rows < 1)
{
    // userupgrade 테이블에 데이터가 없는 경우 신규 데이터를 작성
    $insertUpgrade =
        "INSERT INTO `userupgrade`
        (`no`, `user`)
        VALUES
        (null, '".$userKeyNo."')";
    $resultInsertUpgrade = SendSqlQuery($insertUpgrade, "insertUpgrade", $mysqli);

    $saveInsertId = $mysqli->insert_id;

    // 신규 데이터에 맞춰서 userupgrade 테이블 데이터를 노드로 작성한 후 등록
    $upgradeNo = nodeMaker($dom, "upgradeNo", $saveInsertId);
    $attLv = nodeMaker($dom, "attLv", "1");
    $defLv = nodeMaker($dom, "defLv", "1");
    $moneyLv = nodeMaker($dom, "moneyLv", "1");

    $response->appendChild($upgradeNo);
    $response->appendChild($attLv);
    $response->appendChild($defLv);
    $response->appendChild($moneyLv);
}
else
{
    //userupgrade 테이블에 데이터가 존재하므로 PHP에서 사용하도록 로딩
    $dataUpgradeRow = mysqli_fetch_array($resultFindUserUpgrade);

    $upgradeNo = nodeMaker($dom, "upgradeNo", $dataUpgradeRow["no"]);
    $attLv = nodeMaker($dom, "attLv", $dataUpgradeRow["attLv"]);
    $defLv = nodeMaker($dom, "defLv", $dataUpgradeRow["defLv"]);
    $moneyLv = nodeMaker($dom, "moneyLv", $dataUpgradeRow["moneyLv"]);

    $response->appendChild($upgradeNo);
    $response->appendChild($attLv);
    $response->appendChild($defLv);
    $response->appendChild($moneyLv);
}

// 반환되는 데이터의 헤더에결과값이 XML 형태라는 것을 명시
header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
// 결과 XML  반환
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();
?>
