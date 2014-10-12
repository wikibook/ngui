<?php

include "dbHelper.php";

//
$userKeyNo = $_REQUEST["userKeyNo"];


//
$mysqli = new mysqli($host, $user, $password, $dbname);
if (mysqli_connect_errno())
{
    echo 'Connect faild : '.mysqli_connect_error().'\n';
    $mysqli->close();
    exit();
}
//
$mysqli->set_charset("utf8");

$userKeyNo = $_REQUEST["userKeyNo"];
$payload = $_REQUEST["payload"];
$sku = $_REQUEST["sku"];

//결제 확인된 정보 체크
$sqlFindPayload = "SELECT * FROM `inappgoogle` WHERE `user` = ".$userKeyNo." AND `state` = 2 AND `sku` = '".$sku."' AND `payload` = '".$payload."'";
$resultFindPayload = SendSqlQuery($sqlFindPayload, "FindPayload", $mysqli);

if($resultFindPayload->num_rows<1)
{
    // 정보가 없는경우
    echo "noResult";
    $mysqli->close();
    exit();
}


//sku로 codeNo를 찾아낸다
$sqlFindCodeNo = "SELECT * FROM `inappitem` WHERE `sku` = '".$sku."'";
$resultFindCodeNo = SendSqlQuery($sqlFindCodeNo, "FindCodeNo", $mysqli);

if($resultFindCodeNo->num_rows<1)
{
    //sku가 등록되지 않은경우
    echo "noSkuInfo";
    $mysqli->close();
    exit();
}

$data_row = mysqli_fetch_array($resultFindCodeNo);
//price 정보 로딩
$sqlFindPrice = "SELECT * FROM `price` WHERE `codeNo` = ".$data_row["codeNo"];
$resultFindPrice = SendSqlQuery($sqlFindPrice, "FindPrice", $mysqli);

if($resultFindPrice->num_rows<1)
{
    //price 정보가 없는경우
    echo "noPriceInfo";
    $mysqli->close();
    exit();
}

$dataPrice = mysqli_fetch_array($resultFindPrice);


// 유저정보 갱신
$sqlUpdateGem = "UPDATE `usercore` SET `gems` = `gems` + ".$dataPrice["amount"]." WHERE `no` = ".$userKeyNo;
$resultUpdateGem = SendSqlQuery($sqlUpdateGem, "UpdateGem", $mysqli);

$dataPayload = mysqli_fetch_array($resultFindPayload);
// 지급되었다고기록
$sqlUpdatePayload = "UPDATE `inappgoogle` SET `state` = 20 WHERE `no` = ".$dataPayload["no"];
$resultUpdatePayload = SendSqlQuery($sqlUpdatePayload,"UpdatePayload",$mysqli);

//유저 보석 총 량 전달
$sqlFindGems = "SELECT `gems` FROM `usercore` WHERE `no` = ".$userKeyNo;
$resultFindGems = SendSqlQuery($sqlFindGems, "FindGems", $mysqli);

$dataGems = mysqli_fetch_array($resultFindGems);

echo $dataGems["gems"];
$mysqli->close();
exit();