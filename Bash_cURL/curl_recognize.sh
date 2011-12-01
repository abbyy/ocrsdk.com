#!/bin/bash

# Convert an image file by Abbyy Cloud OCR SDK using cURL
# Usage: cloud_recognize <input> <output> [-l language] [-f txt|rtf|docx|xlsx|pptx|pdfSearchable|pdfTextAndImages|xml]

# do not forget to set http_proxy variable if necessary

ServerUrl='http://cloud.ocrsdk.com'
ApplicationId="_my_application_"
Password="_mypassword_";


if [ -n "$ABBYY_APPID" ]; then
    ApplicationId="$ABBYY_APPID";
fi;

if [ -n "$ABBYY_PWD" ]; then
    Password="$ABBYY_PWD";
fi;


params=`getopt -o f:l: -- "$@"`
if [ $? != 0 ] ; then
    echo "Invalid arguments. Usage:"
    echo "$0 <input> <output> [-f output_format] [-l language]"
    echo "output_format: txt|rtf|docx|xlsx|pptx|pdfSearchable|pdfTextAndImages|xml"
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
            exit 1;
        fi
        shift;;
    esac
done

if [ -z $TargetFile ]; then
    echo "Invalid arguments" >&2;
    exit 1;
fi

echo "Uploading.."
response=`curl -s -S --user $ApplicationId:$Password --form "upload=@$SourceFile" "$ServerUrl/processImage?exportFormat=$OutFormat&language=$Language"`


#Select guid from response string
taskId=`echo $response | grep -o -E 'task id="[^"]*"' | cut -d '"' -f 2`
if [ -z $taskId ]; then
    echo "Error uploading file" >&2;
    exit 1;
fi

echo "Uploaded, task id is '$taskId'"

# Wait until image is processed
taskStatus="Queued"
echo -n "Waiting.."
while [ $taskStatus != "Completed" ]
do
    sleep 4
    echo -n "."
    response=`curl -s -S --user $ApplicationId:$Password $ServerUrl/getTaskStatus?taskId=$taskId`
    taskStatus=`echo $response | grep -o -E 'status="[^"]+"' | cut -d '"' -f 2`
done

echo

# Get result url
resultUrl=`echo $response | grep -o -E 'resultUrl="[^"]+"' | cut -d '"' -f 2`

# Get result
response=`curl -s -S -o $TargetFile $resultUrl`
echo "Done."
