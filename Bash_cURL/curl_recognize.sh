#!/bin/bash

# Convert an image file by Abbyy Cloud OCR SDK using cURL
# Usage: cloud_recognize <input> <output> [-l language] [-f txt|rtf|docx|xlsx|pptx|pdfSearchable|pdfTextAndImages|xml]

# before calling this script, set ABBYY_APPID and ABBYY_PWD environment variables
# do not forget to set http_proxy and https_proxy variables if necessary

# Change to 'cloud-westus.ocrsdk.com' if you created your application in US location
ServerUrl='http://cloud-eu.ocrsdk.com'
# To create an application and obtain a password,
# register at https://cloud.ocrsdk.com/Account/Register
# More info on getting your application id and password at
# https://ocrsdk.com/documentation/faq/#faq3
ApplicationId=""
Password=""

echo "ABBYY Cloud OCR SDK demo recognition script"
echo

if [ -n "$ABBYY_APPID" ]; then
    ApplicationId="$ABBYY_APPID";
elif [ -z "$ApplicationId" ]; then
    echo "No application id specified. Please execute"
    echo "\"export ABBYY_APPID=<your app id>\""
    exit 1
fi;

if [ -n "$ABBYY_PWD" ]; then
    Password="$ABBYY_PWD";
elif [ -z $Password ]; then 
    echo "No application password specified. Please execute"
    echo "\"export ABBYY_PWD=<your app password>\""
	echo "The password should be sent to you after application was created."
    exit 1
fi;

function printUsage {
    echo "Usage:" 
    echo "$0 <input> <output> [-f output_format] [-l language]"
    echo "output_format: txt|rtf|docx|xlsx|pptx|pdfSearchable|pdfTextAndImages|xml"
    echo "Some language examples: Russian Russian,English English,ChinesePRC etc. For full list see ocrsdk documentation"
}

params=`getopt f:l: "$@"`
if [ $? != 0 ] ; then
    echo "Invalid arguments."
    printUsage >&2
    exit 1;
fi

OutFormat="txt"
Language="english"

eval set -- "$params"
while true; do
    case "$1" in
        -f) OutFormat="$2"; shift 2;;
        -l) Language="$2"; shift 2;;
        --) shift;;
        *) if [ -z $1 ]; then
            break;
        elif [ -z $SourceFile ]; then
            SourceFile=$1;
        elif [ -z $TargetFile ]; then
            TargetFile=$1;
        else
            echo "Invalid argument: $1" >&2;
            printUsage
            exit 1;
        fi
        shift;;
    esac
done

if [ -z $TargetFile ]; then
    echo "Invalid arguments." >&2;
    printUsage >&2;
    exit 1;
fi

if [ ! -e "$SourceFile" ]; then
    echo "Source file $SourceFile doesn't exist";
    exit 1;
fi

sourceFileName=`basename "$SourceFile"`
echo "Recognizing $sourceFileName with $Language language. Result will be saved in $OutFormat format.."

echo "Uploading.."
response=`curl -s -S --user "$ApplicationId:$Password" --form "upload=@$SourceFile" "$ServerUrl/processImage?exportFormat=$OutFormat&language=$Language"`


#Select guid from response string
taskId=`echo $response | grep -o -E 'task id="[^"]*"' | cut -d '"' -f 2`
if [ -z $taskId ]; then
    echo "Error uploading file" >&2;
    exit 1;
fi

taskStatus=`echo $response | grep -o -E 'status="[^"]+"' | cut -d '"' -f 2`
if [ $taskStatus == "NotEnoughCredits" ]; then
	echo "Not enough credits to process the document. Please add more pages to your application's account."
	exit 1
fi

echo "Uploaded, task id is '$taskId'"

# Wait until image is processed
# Note: it's recommended that your application waits
# at least 2 seconds before making the first getTaskStatus request
# and also between such requests for the same task.
# Making requests more often will not improve your application performance.
# Note: if your application queues several files and waits for them
# it's recommended that you use listFinishedTasks instead (which is described
# at https://ocrsdk.com/documentation/apireference/listFinishedTasks/).
echo -n "Waiting.."
while [ $taskStatus == "Queued" ] || [ $taskStatus == "InProgress" ]
do
    sleep 5
    echo -n "."
    response=`curl -s -S --user "$ApplicationId:$Password" $ServerUrl/getTaskStatus?taskId=$taskId`
    taskStatus=`echo $response | grep -o -E 'status="[^"]+"' | cut -d '"' -f 2`
done

if [ $taskStatus != "Completed" ]; then
    echo "Unexpected task status $taskStatus"
    exit 1
fi

echo

# Get result url by treating the returned XML as text
resultUrl=`echo $response | grep -o -E 'resultUrl="[^"]+"' | cut -d '"' -f 2`
# Now replace all occurences of "&amp;" with "&"
resultUrl="${resultUrl//&amp;/&}"

# Get result
response=`curl -s -S -o $TargetFile $resultUrl`
echo "Done."
