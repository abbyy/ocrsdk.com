#!/bin/bash

# Convert an image file by Abbyy Cloud OCR SDK using cURL
# Usage: cloud_recognize <input> <output> [-l language] [-f txt|rtf|docx|xlsx|pptx|pdfSearchable|pdfTextAndImages|xml]

# before calling this script, set ABBYY_APPID and ABBYY_PWD environment variables
# do not forget to set http_proxy and https_proxy variables if necessary

ServerUrl='http://cloud.ocrsdk.com'
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
    echo "No application password specified. Plese execute"
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
taskStatus="Queued"
echo -n "Waiting.."
while [ $taskStatus != "Completed" ]
do
    sleep 4
    echo -n "."
    response=`curl -s -S --user "$ApplicationId:$Password" $ServerUrl/getTaskStatus?taskId=$taskId`
    taskStatus=`echo $response | grep -o -E 'status="[^"]+"' | cut -d '"' -f 2`
done

echo

# Get result url
resultUrl=`echo $response | grep -o -E 'resultUrl="[^"]+"' | cut -d '"' -f 2`

# Get result
response=`curl -s -S -o $TargetFile $resultUrl`
echo "Done."
