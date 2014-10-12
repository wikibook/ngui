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
$sku = $_REQUEST["sku"];



// 데이터 생성여부검증
$sqlFind = "SHOW TABLES LIKE 'inappgoogle'";
$resultFind = SendSqlQuery($sqlFind, "Find", $mysqli);

if($resultFind->num_rows < 1)
{
    $sqlCreateTable = "CREATE TABLE  `inappgoogle` (
    `no` INT( 10 ) UNSIGNED NOT NULL AUTO_INCREMENT ,
     `user` INT( 10 ) UNSIGNED NOT NULL ,
     `state` TINYINT( 3 ) UNSIGNED NOT NULL DEFAULT  '0',
     `sku` VARCHAR( 10 ) NOT NULL ,
     `payload` VARCHAR( 18 ) NOT NULL ,
     `receipt` TEXT NOT NULL ,
    PRIMARY KEY (  `no` )
    ) ENGINE = INNODB DEFAULT CHARSET = utf8";
    $resultCreateTable = SendSqlQuery($sqlCreateTable, "CreateTable", $mysqli);
}


$nowpayload = uniqid("fd");

//payload  기록
$sqlRecordPayload = "INSERT INTO `inappgoogle` (`no`, `user`, `state`, `sku`, `payload`) VALUES
(null, ".$userKeyNo.", 1, '".$sku."', '".$nowpayload."')";
$resultRecordPayload = SendSqlQuery($sqlRecordPayload, "RecordPayload", $mysqli);

echo $nowpayload;

$mysqli->close();
exit();