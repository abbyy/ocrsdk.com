# OCR SDK Ruby sample
# Documentation available on http://ocrsdk.com/documentation/

require "rubygems"

# IMPORTANT!
# Make sure you have rest-client (see https://github.com/archiloque/rest-client for detaile) gem installed or install it:
#    gem install rest-client
require "rest_client"

require "rexml/document"

# IMPORTANT!
!!! Please provide your application id and password and remove this line !!!
# To create an application and obtain a password,
# register at http://cloud.ocrsdk.com/Account/Register
# More info on getting your application id and password at
# http://ocrsdk.com/documentation/faq/#faq3

# CGI.escape is needed to escape whitespaces, slashes and other symbols
# that could invalidate the URI if any
# Name of application you created
APPLICATION_ID = CGI.escape("my_application_id")
# Password should be sent to your e-mail after application was created
PASSWORD = CGI.escape("my_password")

# IMPORTANT!
# Specify path to image file you want to recognize
FILE_NAME = "/path/to/my/image.jpg"

# IMPORTANT!
# Specify recognition languages of document. For full list of available languaes see
# http://ocrsdk.com/documentation/apireference/processImage/
# Examples: 
#   English
#   English,German
#   English,German,Spanish
LANGUAGE = "English"

# OCR SDK base url with application id and password
BASE_URL = "http://#{APPLICATION_ID}:#{PASSWORD}@cloud.ocrsdk.com"

# Routine for OCR SDK error output
def output_response_error(response)
  # Parse response xml (see http://ocrsdk.com/documentation/specifications/status-codes)
  xml_data = REXML::Document.new(response)
  error_message = xml_data.elements["error/message"]
  puts "Error: #{error_message.text}" if error_message
end

# Upload and process the image (see http://ocrsdk.com/documentation/apireference/processImage)
puts "Image will be recognized with #{LANGUAGE} language."
puts "Uploading file.."
begin
  response = RestClient.post("#{BASE_URL}/processImage?language=#{LANGUAGE}&exportFormat=txt", :upload => { 
    :file => File.new(FILE_NAME, 'rb') 
  })  
rescue RestClient::ExceptionWithResponse => e
  # Show processImage errors
  output_response_error(e.response)
  raise
else
  # Get task id from response xml to check task status later
  xml_data = REXML::Document.new(response)
  task_id = xml_data.elements["response/task"].attributes["id"]
end

# Get task information in a loop until task processing finishes
puts "Processing image.."
begin
  # Make a small delay
  sleep(0.5)
  
  # Call the getTaskStatus function (see http://ocrsdk.com/documentation/apireference/getTaskStatus)
  response = RestClient.get("#{BASE_URL}/getTaskStatus?taskid=#{task_id}")
rescue RestClient::ExceptionWithResponse => e
  # Show getTaskStatus errors
  output_response_error(e.response)
  raise
else
  # Get the task status from response xml
  xml_data = REXML::Document.new(response)
  task_status = xml_data.elements["response/task"].attributes["status"]
  
  # Check if there were errors ..
  raise "The task hasn't been processed because an error occurred" if task_status == "ProcessingFailed"
  
  # .. or you don't have enough credits (see http://ocrsdk.com/documentation/specifications/task-statuses for other statuses)
  raise "You don't have enough money on your account to process the task" if task_status == "NotEnoughCredits"
end until task_status == "Completed"

# Get the result download link
download_url = xml_data.elements["response/task"].attributes["resultUrl"]

# Download the result
puts "Downloading result.."
recognized_text = RestClient.get(download_url)

# We have the recognized text - output it!
puts recognized_text
