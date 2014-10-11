<?php
// 기본 데이터를 로딩
include "dbHelper.php";

// 값을 전달받아 저장한다.
$userKeyNo = $_REQUEST["userKeyNo"];
$pID = $_REQUEST["pID"];

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


$sqlUserCoreData = "SELECT * FROM `usercore` WHERE `no` = ".$userKeyNo;
$resultUserCoreData = SendSqlQuery($sqlUserCoreData, "UserCoreData", $mysqli);
$dataUserCore = mysqli_fetch_array($resultUserCoreData);

$sqlPriceData = "SELECT * FROM `price` WHERE `codeNo` = ".$pID;
$resultPriceData = SendSqlQuery($sqlPriceData, "PriceData", $mysqli);
$dataPriceData = mysqli_fetch_array($resultPriceData);


$spendGems = $dataPriceData["price"];
$nowGems = $dataUserCore["gems"];
$addCoins = $dataPriceData["amount"];

// 보석이부족한경우.
if($nowGems < $spendGems)
{
    echo "none2";
    $mysqli->close();
    exit();
}

//결과반영
$sqlUpdateUserCore = "UPDATE `usercore` SET `gems` = `gems` - ".$spendGems.",
 `coins` = `coins` + ".$addCoins." WHERE `no` = ".$userKeyNo;
$resultUpdateUserCore = SendSqlQuery($sqlUpdateUserCore, "UpdateUserCore", $mysqli);


$returnGems = $nowGems - $spendGems;
$returnCoins = $dataUserCore["coins"] + $addCoins;

//반환 데이터생성
// 사용자 데이터가 존재하므로 리턴할 XML 데이터 구조 생성
$dom = new DOMDocument('1.0', 'UTF-8');
//root 노드 생성
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);


$gems = nodeMaker($dom, "gems", $returnGems);
$coins = nodeMaker($dom, "coins", $returnCoins);

$rootNode->appendChild($gems);
$rootNode->appendChild($coins);

// 반환되는 데이터의 헤더에결과값이 XML 형태라는 것을 명시
header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
// 결과 XML  반환
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();