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


// 데이터 생성여부검증
$sqlFind = "SHOW TABLES LIKE 'inappitem'";
$resultFind = SendSqlQuery($sqlFind, "Find", $mysqli);

if($resultFind->num_rows > 0)
{
    echo "error : exist default inapp data";
    $mysqli->close();
    exit();
}

$sqlCreateTable = "CREATE TABLE  `inappitem`
(`no` INT( 10 ) UNSIGNED NOT NULL AUTO_INCREMENT ,
 `codeNo` INT( 10 ) UNSIGNED NOT NULL DEFAULT  '0',
  `sku` VARCHAR( 30 ) NOT NULL ,
   PRIMARY KEY (  `no` ))
   ENGINE = INNODB DEFAULT CHARSET = utf8";
$resultCreateTable = SendSqlQuery($sqlCreateTable, "CreateTable", $mysqli);


$sqlAddRow = "INSERT INTO `farmdefence`.`inappitem` (`no`, `codeNo`, `sku`) VALUE
(null, 101, 'add_gem01'), (null, 102, 'add_gem02'), (null, 103, 'add_gem03'),
(null, 104, 'add_gem04'), (null, 105, 'add_gem05')";
$reslutAddRow = SendSqlQuery($sqlAddRow, "AddRow", $mysqli);

echo "complete";
$mysqli->close();
exit();