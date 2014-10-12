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
$orderId = $_REQUEST["orderId"];
$packageName = $_REQUEST['packageName'];
$productId = $_REQUEST['productId'];
$purchaseTime = $_REQUEST["purchaseTime"];
$purchaseState = $_REQUEST['purchaseState'];
$developerPayload = $_REQUEST['developerPayload'];
$purchaseToken = $_REQUEST["purchaseToken"];


$sqlFindPayload = "SELECT * FROM `inappgoogle` WHERE `user` = ".$userKeyNo." AND `state` = 1 AND `sku` = '".$productId."' AND `payload` = '".$developerPayload."'";
$resultFindPayload = SendSqlQuery($sqlFindPayload, "FindPayload", $mysqli);

if($resultFindPayload->num_rows<1)
{
    //$data_row = mysqli_fetch_array($resultFindPayload);
    //update receipt
    //$sqlUpdateReceipt = "UPDATE `inappgoogle` SET `state` = 10, `receipt`='".$receipt."' WHERE `no`= ".$data_row["no"];
    //$resultUpdateReceipt = SendSqlQuery($sqlUpdateReceipt, "UpdateReceipt", $mysqli);

    echo "0";
    $mysqli->close();
    exit();
}

$data_row = mysqli_fetch_array($resultFindPayload);
//update receipt

$receipt = "{\"orderId\":\"".$orderId."\", \"packageName\":\"".$packageName."\", \"productId\":\"".$productId."\", \"purchaseTime\":\"".$purchaseTime."\", \"purchaseState\":\"".$purchaseState."\", \"developerPayload\":\"".$developerPayload."\", \"purchaseToken\":\"".$purchaseToken."\"}";

$sqlUpdateReceipt = "UPDATE `inappgoogle` SET `state` = 2, `receipt`='".$receipt."' WHERE `no`= ".$data_row["no"];
$resultUpdateReceipt = SendSqlQuery($sqlUpdateReceipt, "UpdateReceipt", $mysqli);

echo "1";
$mysqli->close();
exit();


//https://www.googleapis.com/androidpublisher/v2/applications/
//[packageName]/purchases/products/[productId]/tokens/[token]