<?php

  // 1. Send image to Cloud OCR SDK using processImage call
  // 2.	Get response as xml
  // 3.	Read taskId from xml

  // !!!!!!!!!! Enter your data here !!!!!!!!!!
  $applicationId = 'my_application_id';
  $password = 'my_application_password';
  $fileName = 'myfile.jpg';

  // Get path to file that we are going to recognize
  $local_directory=dirname(__FILE__).'/images/';
  $filePath = $local_directory.'/'.$fileName;
  if(!file_exists($filePath))
  {
    die('File '.$filePath.' not found.');
  }

  // Recognizing with English language to rtf
  // You can use combination of languages like ?language=english,russian or
  // ?language=english,french,dutch
  // For details, see API reference for processImage method
  $url = 'http://cloud.ocrsdk.com/processImage?language=english&exportFormat=rtf';
  
  // Send HTTP POST request and ret xml response
  $ch = curl_init();
  curl_setopt($ch, CURLOPT_URL, $url);
  curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
  curl_setopt($ch, CURLOPT_USERPWD, "$applicationId:$password");
  curl_setopt($ch, CURLOPT_POST, 1);
  $post_array = array(
      "my_file"=>"@".$filePath,
  );
  curl_setopt($ch, CURLOPT_POSTFIELDS, $post_array); 
  $response = curl_exec($ch);
  curl_close($ch);

  // Parse xml response
  $xml = simplexml_load_string($response);
  $arr = $xml->task[0]->attributes();
  
  // Task id
  $taskid = $arr["id"];  
  
  // 4. Get task information in a loop until task processing finishes
  // 5. If response contains "Completed" staus - extract url with result
  // 6. Download recognition result (text) and display it

  $url = 'http://cloud.ocrsdk.com/getTaskStatus';
  $qry_str = "?taskid=$taskid";

  // Check task status in a loop until it is finished
  // TODO: support states indicating error
  do
  {
    sleep(5);
    $ch = curl_init();
    curl_setopt($ch, CURLOPT_URL, $url.$qry_str);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
    curl_setopt($ch, CURLOPT_USERPWD, "$applicationId:$password");
    $response = curl_exec($ch);
    curl_close($ch);
  
    // parse xml
    $xml = simplexml_load_string($response);
    $arr = $xml->task[0]->attributes();
  }
  while($arr["status"] != "Completed");

  // Result is ready. Download it

  $url = $arr["resultUrl"];   
  $ch = curl_init();
  curl_setopt($ch, CURLOPT_URL, $url);
  curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
  $response = curl_exec($ch);
  curl_close($ch);
 
  // Let user donwload rtf result
  header('Content-type: application/rtf');
  header('Content-Disposition: attachment; filename="file.rtf"');
  echo $response;
?>
