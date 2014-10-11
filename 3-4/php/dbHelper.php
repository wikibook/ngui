<?php

$host = "localhost";
$user = "root";
$password = "1234";
$dbname = "farmdefence";

// 서버에 쿼리를 전달하여 결과를 리턴받는 함수
function SendSqlQuery($sqlQuery, $workName, $mysqli)
{
    if (!$resultQuery = $mysqli->query($sqlQuery))
    {
        echo "query error ".$workName." : ".mysqli_error($mysqli).$sqlQuery;
        exit();
    }
    return $resultQuery;
}

// XML 텍스트 노트를 만드는 함수
function nodeMaker($dom, $nodeName, $nodeString)
{
    $node = $dom->createElement($nodeName);
    $node_Text = $dom->createTextNode($nodeString);
    $node->appendChild($node_Text);

    return $node;
}

?>