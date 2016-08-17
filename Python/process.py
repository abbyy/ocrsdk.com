#!/usr/bin/python

# Usage: process.py <input file> <output file> [-language <Language>] [-pdf|-txt|-rtf|-docx|-xml]

import argparse
import base64
import getopt
import MultipartPostHandler
import os
import re
import sys
import time
import urllib2
import urllib

from AbbyyOnlineSdk import *


processor = AbbyyOnlineSdk()

if "ABBYY_APPID" in os.environ:
	processor.ApplicationId = os.environ["ABBYY_APPID"]

if "ABBYY_PWD" in os.environ:
	processor.Password = os.environ["ABBYY_PWD"]

# Proxy settings
if "http_proxy" in os.environ:
	proxyString = os.environ["http_proxy"]
	print "Using proxy at %s" % proxyString
	processor.Proxy = urllib2.ProxyHandler( { "http" : proxyString })


# Recognize a file at filePath and save result to resultFilePath
def recognizeFile( filePath, resultFilePath, language, outputFormat ):
	print "Uploading.."
	settings = ProcessingSettings()
	settings.Language = language
	settings.OutputFormat = outputFormat
	task = processor.ProcessImage( filePath, settings )
	if task == None:
		print "Error"
		return
	print "Id = %s" % task.Id
	print "Status = %s" % task.Status

	# Wait for the task to be completed
	sys.stdout.write( "Waiting.." )
	# Note: it's recommended that your application waits at least 2 seconds
	# before making the first getTaskStatus request and also between such requests
	# for the same task. Making requests more often will not improve your
	# application performance.
	# Note: if your application queues several files and waits for them
	# it's recommended that you use listFinishedTasks instead (which is described
	# at http://ocrsdk.com/documentation/apireference/listFinishedTasks/).

	while task.IsActive() == True :
		time.sleep( 5 )
		sys.stdout.write( "." )
		task = processor.GetTaskStatus( task )

	print "Status = %s" % task.Status
	
	if task.Status == "Completed":
		if task.DownloadUrl != None:
			processor.DownloadResult( task, resultFilePath )
			print "Result was written to %s" % resultFilePath
	else:
		print "Error processing task"



	
parser = argparse.ArgumentParser( description="Recognize a file via web service" )
parser.add_argument( 'sourceFile' )
parser.add_argument( 'targetFile' )

parser.add_argument( '-l', '--language', default='English', help='Recognition language (default: %(default))' )
group = parser.add_mutually_exclusive_group()
group.add_argument( '-txt', action='store_const', const='txt', dest='format', default='txt' )
group.add_argument( '-pdf', action='store_const', const='pdfSearchable', dest='format' )
group.add_argument( '-rtf', action='store_const', const='rtf', dest='format' )
group.add_argument( '-docx', action='store_const', const='docx', dest='format' )
group.add_argument( '-xml', action='store_const', const='xml', dest='format' )

args = parser.parse_args()

sourceFile = args.sourceFile
targetFile = args.targetFile
language = args.language
outputFormat = args.format

if os.path.isfile( sourceFile ):
	recognizeFile( sourceFile, targetFile, language, outputFormat )	
else:
	print "No such file: %s" % sourceFile
