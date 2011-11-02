#!/usr/bin/python

# Usage: recognize.py <input file> <output file> [-language <Language>] [-pdf|-txt|-rtf|-docx|-xml]

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

if "FRE_LOGIN" in os.environ:
	processor.Username = os.environ["FRE_LOGIN"]

if "FRE_PWD" in os.environ:
	processor.Password = os.environ["FRE_PWD"]

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
	while True :
		task = processor.GetTaskStatus( task )
		if task.IsActive() == False:
			print
			break
		sys.stdout.write( "." )
		time.sleep( 4 )

	print "Status = %s" % task.Status
	
	if task.DownloadUrl != None:
		processor.DownloadResult( task, resultFilePath )
		print "Result was written to %s" % resultFilePath



	
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
