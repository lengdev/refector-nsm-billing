<?php
foreach ($_POST as $key => $value) {
    echo  "$key: $value";
    if (strpos($key, "image") !== false) {
        file_put_contents("$key.jpg", $value);
    } else {
        echo "text data: $value";
    }
}
?>