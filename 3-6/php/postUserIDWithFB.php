<?php
// 기본 데이터를 로딩
include "dbHelper.php";


$userID = $_REQUEST["userID"];
$fbID = $_REQUEST["facebookID"];
// 예외처리 : 아이디가 입력되지 않은 경우
if($userID == null)
{
    echo "error : input your ID";
    exit();
}

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

// 중복 아이디가 있는지 찾아보는 쿼리문
$findUserID = "SELECT * FROM `usercore` WHERE `id` = '".$userID."' AND `facebook` = '".$fbID."'";
// 쿼리 실행
$resultFindUserID = SendSqlQuery($findUserID, "findUserID", $mysqli);

// 예외처리 : 중복 아이디가 발견된 경우
if($resultFindUserID->num_rows > 0)
{
    $datarow = mysqli_fetch_array($resultFindUserID);
    echo "exist".$datarow["no"];
    $mysqli->close();
    exit();
}

// 새로운 아이디를 넣은 행을  usercore 테이블 생성하는 쿼리문
$insertUserID =
    "INSERT INTO `usercore`
    (`no`, `id`, `gems`, `coins`, `hearts`, `highScore`, `loginTime`, `facebook`)
    VALUES
    (null, '".$userID."', 20, 1000, 5, 0, ".time().", '".$fbID."')";
$resultInsertUserID = SendSqlQuery($insertUserID, "insertUserID", $mysqli);

$saveInsertId = $mysqli->insert_id;

// 사용자 업그레이드 할당하는 쿼리문
$insertUpgrade =
    "INSERT INTO `userupgrade`
    (`no`, `user`)
    VALUES
    (null, '".$saveInsertId."')";
$resultInsertUpgrade = SendSqlQuery($insertUpgrade, "insertUpgrade", $mysqli);

// 행 번호를 결과로 리턴.
echo "done0".$saveInsertId;
// 데이터베이스 접속을 종료.
$mysqli->close();
exit();
?>