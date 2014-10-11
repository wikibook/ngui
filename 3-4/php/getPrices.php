<?php

// 기본 데이터를 로딩
include "dbHelper.php";

// usercore 테이블 행번호 저장
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


//
$sqlAllPrice = "SELECT * FROM `price` ORDER BY `no`";
$resultAllPrice = SendSqlQuery($sqlAllPrice, "AllPrice", $mysqli);


//
$dom = new DOMDocument('1.0', 'UTF-8');
//root element
$rootNode = $dom->createElement('farmdefence');
$dom->appendChild($rootNode);
$response = $dom->createElement("price");
$rootNode->appendChild($response);


//db php
while($datarow = mysqli_fetch_array($resultAllPrice))
{
    $dataNode = $dom->createElement('PriceData');

    $codeNo = nodeMaker($dom, "codeNo", $datarow["codeNo"]);
    $price = nodeMaker($dom, "price", $datarow["price"]);
    $bonus = nodeMaker($dom, "bonus", $datarow["bonus"]);
    $amount = nodeMaker($dom, "amount", $datarow["amount"]);

    $dataNode->appendChild($codeNo);
    $dataNode->appendChild($price);
    $dataNode->appendChild($bonus);
    $dataNode->appendChild($amount);

    $response->appendChild($dataNode);
}

//
header("Content-type: text/xml; charset=UTF-8");header("Cache-Control: no-cache");header("Pragma: no-cache");
$xmlString = $dom->saveXML();
echo $xmlString;

$mysqli->close();
exit();
?>