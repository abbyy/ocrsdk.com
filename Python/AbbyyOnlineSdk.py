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
import xml.dom.minidom
import xml.sax.saxutils

class ProcessingSettings:
	Language = "English"
	OutputFormats = ["docx"]

class Task:
	Status = "Unknown"
	Id = None
	DownloadUrl = None
	def IsActive( self ):
		if self.Status == "InProgress" or self.Status == "Queued":
			return True
		else:
			return False

class AbbyyOnlineSdk:
	ServerUrl = "http://cloud.ocrsdk.com/"
	# To create an application and obtain a password,
	# register at http://cloud.ocrsdk.com/Account/Register
	# More info on getting your application id and password at
	# http://ocrsdk.com/documentation/faq/#faq3
	ApplicationId = "user"
	Password = "password"
	Proxy = None
	enableDebugging = 0

	def ProcessImage( self, filePath, settings ):
                outputFormats = ','.join(settings.OutputFormats)
		urlParams = urllib.urlencode({
			"language" : settings.Language,
			"exportFormat" : outputFormats
			})
		requestUrl = self.ServerUrl + "processImage?" + urlParams

		bodyParams = { "file" : open( filePath, "rb" )  }
		request = urllib2.Request( requestUrl, None, self.buildAuthInfo() )
		response = self.getOpener().open(request, bodyParams).read()
		if response.find( '<Error>' ) != -1 :
			return None
		# Any response other than HTTP 200 means error - in this case exception will be thrown

		# parse response xml and extract task ID
		task = self.DecodeResponse( response )
		return task

	def GetTaskStatus( self, task ):
		urlParams = urllib.urlencode( { "taskId" : task.Id } )
		statusUrl = self.ServerUrl + "getTaskStatus?" + urlParams
		request = urllib2.Request( statusUrl, None, self.buildAuthInfo() )
		response = self.getOpener().open( request ).read()
		task = self.DecodeResponse( response )
		return task

        def DownloadResult( self, resultUrl, outputPath):
                fileResponse = urllib2.urlopen(resultUrl).read()
                resultFile = open( outputPath, "wb" )
                resultFile.write( fileResponse )

	def DecodeResponse( self, xmlResponse ):
		""" Decode xml response of the server. Return Task object """
		dom = xml.dom.minidom.parseString( xmlResponse )
		taskNode = dom.getElementsByTagName( "task" )[0]
		task = Task()
		task.Id = taskNode.getAttribute( "id" )
		task.Status = taskNode.getAttribute( "status" )
		if task.Status == "Completed":
                        task.DownloadUrl = []
                        for resultAttr in ('resultUrl', 'resultUrl2', 'resultUrl3'):
                                result = taskNode.getAttribute(resultAttr)
                                if result:
                                        # xml escape needs to be unescaped
                                        task.DownloadUrl.append(xml.sax.saxutils.unescape(result))
		return task


	def buildAuthInfo( self ):
		return { "Authorization" : "Basic %s" % base64.encodestring( "%s:%s" % (self.ApplicationId, self.Password) ) }

	def getOpener( self ):
		if self.Proxy == None:
			self.opener = urllib2.build_opener( MultipartPostHandler.MultipartPostHandler,
			urllib2.HTTPHandler(debuglevel=self.enableDebugging))
		else:
			self.opener = urllib2.build_opener(
				self.Proxy,
				MultipartPostHandler.MultipartPostHandler,
				urllib2.HTTPHandler(debuglevel=self.enableDebugging))
		return self.opener
