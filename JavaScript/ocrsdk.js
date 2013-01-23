if (typeof process == 'undefined' || process.argv[0] != "node") {
	throw new Error("This code must be run on server side under NodeJS");
}

var http = require("http");
var https = require("https");
var url = require("url");
var sys = require("sys");
var events = require("events");
var fs = require('fs');

var xml2js = null;
try {
	xml2js = require('xml2js');
} catch (err) {
	throw new Error("xml2js module not found. Please install it with 'npm install xml2js'");
}

exports.create = function(applicationId, password) {
	return new ocrsdk(applicationId, password);
}

exports.ProcessingSettings = ProcessingSettings;

/**
 * TaskData object used in functions below has the following important fields:
 * {string} id 
 * {string} status 
 * {string} resultUrl
 * 
 * It is mapped from xml described at
 * http://ocrsdk.com/documentation/specifications/status-codes/
 */

/**
 * Create a new ocrsdk object.
 * 
 * @constructor
 * @param {string} applicationId 	Application Id.
 * @param {string} password 		Password for the application you received in e-mail.
 */
function ocrsdk(applicationId, password) {
	this.appId = applicationId;
	this.password = password;

	this.serverUrl = "http://cloud.ocrsdk.com"; // You can change it to
												// https://cloud.ocrsdk.com if
												// you need secure channel
}

/**
 * Settings used to process image
 */
function ProcessingSettings() {
	this.language = "English"; // Recognition language or comma-separated list
								// of languages.
	this.exportFormat = "txt"; // Output format. One of: txt, rtf, docx, xlsx,
								// pptx, pdfSearchable, pdfTextAndImages, xml.
	this.customOptions = ''; // Other custom options passed to RESTful call,
								// like 'profile=documentArchiving'
}

/**
 * Upload file to server and start processing.
 * 
 * @param {string} filePath 					Path to the file to be processed.
 * @param {ProcessingSettings} [settings] 		Image processing settings.
 * @param {function(error, taskData)} callback 	The callback function.
 */
ocrsdk.prototype.processImage = function(filePath, settings, userCallback) {

	if (!fs.existsSync(filePath) || !fs.statSync(filePath).isFile()) {
		userCallback(new Error("file " + filePath + " doesn't exist"), null);
		return;
	}

	if (settings == null) {
		settings = new ProcessingSettings();
	}

	var urlOptions = settings.asUrlParams();
	var req = this._createTaskRequest('POST', '/processImage' + urlOptions,
			userCallback);

	var fileContents = fs.readFileSync(filePath);
	req.write(fileContents);
	req.end();
}

/**
 * Get current task status.
 * 
 * @param {string} taskId 						Task identifier as returned in taskData.id.
 * @param {function(error, taskData)} callback 	The callback function.
 */
ocrsdk.prototype.getTaskStatus = function(taskId, userCallback) {
	var req = this._createTaskRequest('GET', '/getTaskStatus?taskId=' + taskId,
			userCallback);
	req.end();
}

/**
 * Wait until task processing is finished. You need to check task status after
 * processing to see if you can download result.
 * 
 * @param {string} taskId 						Task identifier as returned in taskData.id.
 * @param {function(error, taskData)} callback 	The callback function.
 */
ocrsdk.prototype.waitForCompletion = function(taskId, userCallback) {
	// Call getTaskStatus every second until task is completed

	var recognizer = this;

	function waitFunction() {
		recognizer.getTaskStatus(taskId, function(error, taskData) {
			if (error) {
				userCallback(error, null);
				return;
			}

			console.log("Task status is " + taskData.status);

			if (taskData.status == 'Completed'
					|| taskData.status == 'ProcessingFailed'
					|| taskData.status == 'NotEnoughCredits') {
				userCallback(null, taskData);
			} else {
				setTimeout(waitFunction, 1000);
			}
		});
	}

	waitFunction();
}

/**
 * Download result of document processing. Task needs to be in 'Completed' state
 * to call this function.
 * 
 * @param {string} resultUrl 				URL where result is located
 * @param {string} outputFilePath 			Path where to save downloaded file
 * @param {function(error)} userCallback 	The callback function.
 */
ocrsdk.prototype.downloadResult = function(resultUrl, outputFilePath,
		userCallback) {
	var file = fs.createWriteStream(outputFilePath);

	var parsed = url.parse(resultUrl);

	var req = https.request(parsed, function(response) {
		response.on('data', function(data) {
			file.write(data);
		});

		response.on('end', function() {
			file.end();
			userCallback(null);
		});
	});

	req.on('error', function(e) {
		userCallback(error);
	});

	req.end();

}

/**
 * Create http GET or POST request to cloud service with given path and
 * parameters.
 * 
 * @param {string} method 				'GET' or 'POST'.
 * @param {string} urlPath 				RESTful verb with parameters, e.g. '/processImage/language=French'.
 * @param {function(error, TaskData)} 	User callback which is called when request is executed.
 * @return {http.ClientRequest} 		Created request which is ready to be started.
 */
ocrsdk.prototype._createTaskRequest = function(method, urlPath,
		taskDataCallback) {

	/**
	 * Convert server xml response to TaskData. Calls taskDataCallback after.
	 * 
	 * @param data	Server XML response.
	 */
	function parseXmlResponse(data) {
		var response = new Object();

		var parser = new xml2js.Parser({
			explicitCharKey : false,
			trim : true,
			explicitRoot : true,
			mergeAttrs : true
		});
		parser.parseString(data, function(err, objResult) {
			if (err) {
				taskDataCallback(err, null);
				return;
			}

			response = objResult;
		});

		if (response == null) {
			return;
		}

		if (response.response == null || response.response.task == null
				|| response.response.task[0] == null) {
			if (response.error != null) {
				taskDataCallback(new Error(response.error.message), null);
			} else {
				taskDataCallback(new Error("Unknown server resonse"), null);
			}

			return;
		}

		var task = response.response.task[0];

		taskDataCallback(null, task);
	}

	function getServerResponse(res) {
		res.setEncoding('utf8');
		res.on('data', parseXmlResponse);
	}

	var requestOptions = url.parse(this.serverUrl + urlPath);
	requestOptions.auth = this.appId + ":" + this.password;
	requestOptions.method = method;
	requestOptions.headers = {
		'User-Agent' : "node.js client library"
	};

	var req = null;
	if (requestOptions.protocol == 'http:') {
		req = http.request(requestOptions, getServerResponse);
	} else {
		req = https.request(requestOptions, getServerResponse);
	}

	req.on('error', function(e) {
		taskDataCallback(e, null);
	});

	return req;
}

/**
 * Convert processing settings to string passed to RESTful request.
 */
ProcessingSettings.prototype.asUrlParams = function() {
	var result = '';

	if (this.language.length != null) {
		result = '?language=' + this.language;
	} else {
		result = '?language=English';
	}

	if (this.exportFormat.length != null) {
		result += '&exportFormat=' + this.exportFormat;
	} else {
		result += "&exportFormat=txt"
	}

	if (this.customOptions.length != 0) {
		result += '?' + this.customOptions;
	}

	return result;
}
