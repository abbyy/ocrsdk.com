#ABBYY Cloud OCR SDK demo sample for iOS

##Usage

###Initial setup

The easiest way to run this sample is [Cocoa Pods](http://cocoapods.org):

````Bash
pod install
````

Then open a generated `OCRSDKDemo.xcworkspace` workspace and build it (but specify your access credentials first!).

###Access credentials

Please modify [Sources/OCRDemoClient.m](Sources/OCRDemoClient.m) before running the program!

You need to provide your credentials to connect to Cloud OCR SDK. 
Set `kApplicationId` and `kPassword` constants.
To create an application and obtain a password, register at http://cloud.ocrsdk.com/Account/Register

###Recognition language

This sample is pre-configured to recognize English texts. If you recognize text in other language, you need to change the behaviour using 
`kRecognitionLanguages` and `kExportFormat` constants in [Sources/OCRMainViewController.m](Sources/OCRMainViewController.m)
