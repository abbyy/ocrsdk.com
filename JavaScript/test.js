// Demo sample using ABBYY Cloud OCR SDK from Node.js

if (((typeof process) == 'undefined') || ((typeof window) != 'undefined')) {
	throw new Error("This code must be run on server side under NodeJS");
}

!!! Please provide your application id and password and remove this line !!!
// To create an application and obtain a password,
// register at https://cloud.ocrsdk.com/Account/Register
// More info on getting your application id and password at
// https://ocrsdk.com/documentation/faq/#faq3

// Change to http://cloud-westus.ocrsdk.com for applications created in US location
// Change to https for secure connection
var serviceUrl = 'http://cloud-eu.ocrsdk.com';
// Name of application you created
var appId = '';
// Password should be sent to your e-mail after application was created
var password = '';

var imagePath = 'myFile.jpg';
var outputPath = 'result.txt';

try {
	console.log("ABBYY Cloud OCR SDK Sample for Node.js");

	var ocrsdkModule = require('./ocrsdk.js');
	var ocrsdk = ocrsdkModule.create(appId, password, serviceUrl);

	if (appId.length == 0 || password.length == 0) {
		throw new Error("Please provide your application id and password!");
	}
	
	if( imagePath == 'myFile.jpg') {
		throw new Error( "Please provide path to your image!")
	}

	function downloadCompleted(error) {
		if (error) {
			console.log("Error: " + error.message);
			return;
		}
		console.log("Done.");
	}

	function processingCompleted(error, taskData) {
		if (error) {
			console.log("Error: " + error.message);
			return;
		}

		if (taskData.status != 'Completed') {
			console.log("Error processing the task.");
			if (taskData.error) {
				console.log("Message: " + taskData.error);
			}
			return;
		}

		console.log("Processing completed.");
		console.log("Downloading result to " + outputPath);

		ocrsdk
				.downloadResult(taskData.resultUrl.toString(), outputPath,
						downloadCompleted);
	}

	function uploadCompleted(error, taskData) {
		if (error) {
			console.log("Error: " + error.message);
			return;
		}

		console.log("Upload completed.");
		console.log("Task id = " + taskData.id + ", status is " + taskData.status);
		if (!ocrsdk.isTaskActive(taskData)) {
			console.log("Unexpected task status " + taskData.status);
			return;
		}

		ocrsdk.waitForCompletion(taskData.id, processingCompleted);
	}

	var settings = new ocrsdkModule.ProcessingSettings();
	// Set your own recognition language and output format here
	settings.language = "English"; // Can be comma-separated list, e.g. "German,French".
	settings.exportFormat = "txt"; // All possible values are listed in 'exportFormat' parameter description 
                                   // at https://ocrsdk.com/documentation/apireference/processImage/

	console.log("Uploading image..");
	ocrsdk.processImage(imagePath, settings, uploadCompleted);

} catch (err) {
	console.log("Error: " + err.message);
}
