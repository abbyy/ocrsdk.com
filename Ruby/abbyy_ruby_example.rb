# OCR SDK Ruby sample
# Documentation available on https://ocrsdk.com/documentation/

require "rubygems"

# IMPORTANT!
# Make sure you have rest-client (see https://github.com/archiloque/rest-client for detaile) gem installed or install it:
#    gem install rest-client
require "rest_client"

require "rexml/document"

# IMPORTANT!
!!! Please provide your application id and password and remove this line !!!
# To create an application and obtain a password,
# register at https://cloud.ocrsdk.com/Account/Register
# More info on getting your application id and password at
# https://ocrsdk.com/documentation/faq/#faq3

# CGI.escape is needed to escape whitespaces, slashes and other symbols
# that could invalidate the URI if any
# Name of application you created
APPLICATION_ID = CGI.escape("my_application_id")
# Password should be sent to your e-mail after application was created
PASSWORD = CGI.escape("my_password")
# OCR SDK base url with application id and password
# Change to cloud-eu.ocrsdk.com if the application was created in US location
BASE_URL = "http://#{APPLICATION_ID}:#{PASSWORD}@cloud-eu.ocrsdk.com"

# IMPORTANT!
# Specify path to image file you want to recognize
FILE_NAME = "/path/to/my/image.jpg"

# IMPORTANT!
# Specify recognition languages of document. For full list of available languaes see
# https://ocrsdk.com/documentation/apireference/processImage/
# Examples: 
#   English
#   English,German
#   English,German,Spanish
LANGUAGE = "English"

# Routine for OCR SDK error output
def output_response_error(response)
  # Parse response xml (see https://ocrsdk.com/documentation/specifications/status-codes)
  xml_data = REXML::Document.new(response)
  error_message = xml_data.elements["error/message"]
  puts "Error: #{error_message.text}" if error_message
end

# Upload and process the image (see https://ocrsdk.com/documentation/apireference/processImage)
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
  task_element = xml_data.elements["response/task"]
  task_id = task_element.attributes["id"]
  # Obtain the task status here so that the loop below is not started
  # if your application account has not enough credits
  task_status = task_element.attributes["status"]
end

# Get task information in a loop until task processing finishes
puts "Waiting till image is processed.."
while task_status == "InProgress" or task_status == "Queued" do
  begin
    # Note: it's recommended that your application waits
    # at least 2 seconds before making the first getTaskStatus request
    # and also between such requests for the same task.
    # Making requests more often will not improve your application performance.
    # Note: if your application queues several files and waits for them
    # it's recommended that you use listFinishedTasks instead (which is described
    # at https://ocrsdk.com/documentation/apireference/listFinishedTasks/).
    # Wait a bit
    sleep(5)
  
    # Call the getTaskStatus function (see https://ocrsdk.com/documentation/apireference/getTaskStatus)

    # Note: a logical error in more complex surrounding code can cause
    # a situation where the code below tries to prepare for getTaskStatus request
    # while not having a valid task id. Such request would fail anyway.
    # It's highly recommended that you have an explicit task id validity check
    # right before preparing a getTaskStatus request.
    raise "Invalid task id used when preparing getTaskStatus request"\
      if ((!(defined? task_id)) || task_id.nil? ||task_id.empty?|| (task_id.include? "00000000-0"))
    response = RestClient.get("#{BASE_URL}/getTaskStatus?taskid=#{task_id}")
  rescue RestClient::ExceptionWithResponse => e
    # Show getTaskStatus errors
    output_response_error(e.response)
    raise
  else
    # Get the task status from response xml
    xml_data = REXML::Document.new(response)
    task_element = xml_data.elements["response/task"]
    task_status = task_element.attributes["status"]
  end
end

# Check if there were errors ..
raise "The task hasn't been processed because an error occurred" if task_status == "ProcessingFailed"
  
# .. or you don't have enough credits (see https://ocrsdk.com/documentation/specifications/task-statuses for other statuses)
raise "You don't have enough money on your account to process the task" if task_status == "NotEnoughCredits"

# Get the result download link
download_url = xml_data.elements["response/task"].attributes["resultUrl"]

# Download the result
puts "Downloading result.."
recognized_text = RestClient.get(download_url)

# We have the recognized text - output it!
puts recognized_text
