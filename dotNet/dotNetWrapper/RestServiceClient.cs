using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Abbyy.CloudOcrSdk
{
    public class ProcessingErrorException : System.Net.WebException
    {
        public ProcessingErrorException(string message, System.Net.WebException e)
            : base(message, e)
        {
        }
    }

    public class Task
    {
        public TaskId Id;
        public TaskStatus Status;

        /// <summary>
        /// When task was created. Can be null if no information
        /// </summary>
        public DateTime RegistrationTime;

        /// <summary>
        /// Last activity time. Can be null if no information
        /// </summary>
        public DateTime StatusChangeTime;

        /// <summary>
        /// Number of pages in task
        /// </summary>
        public int PagesCount = 1;

        /// <summary>
        /// Task cost in credits
        /// </summary>
        public int Credits = 0;

        /// <summary>
        /// Task description provided by user
        /// </summary>
        public string Description = null;

        /// <summary>
        /// Url to download processed tasks
        /// </summary>
        public List<string> DownloadUrls = null;

        /// <summary>
        /// Error description when task processing failed
        /// </summary>
        public string Error = null;

        public Task()
        {
            Status = TaskStatus.Unknown;
            Id = new TaskId("<unknown>");
        }

        public Task(TaskId id, TaskStatus status)
        {
            Id = id;
            Status = status;
        }

        public bool IsTaskActive()
        {
            return IsTaskActive(Status);
        }

        // Task is submitted or is processing
        public static bool IsTaskActive( TaskStatus status ) 
        {
            switch (status)
            {
                case TaskStatus.Submitted:
                case TaskStatus.Queued:
                case TaskStatus.InProgress:
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum TaskStatus
    {
        Unknown,
        Submitted,
        Queued,
        InProgress,
        Completed,
        ProcessingFailed,
        Deleted, 
        NotEnoughCredits
    }

    public class TaskId : IEquatable<TaskId>
    {
        public TaskId(string id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public bool Equals(TaskId b)
        {
            return b._id == _id;
        }

        private readonly string _id;
    }

    public interface IRequestAuthSetup {
		void Run( WebRequest request, String username, String password );
	}

	public class BasicRequestAuthSetup : IRequestAuthSetup {
		public void Run( WebRequest request, String username, String password )
		{
			Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string toEncode = username + ":" + password;
            string baseEncoded = Convert.ToBase64String(encoding.GetBytes(toEncode));
			request.Headers.Add( "Authorization", "Basic: " + baseEncoded );
		}
	}

    public class RestServiceClient
    {
        public RestServiceClient()
        {
            ServerUrl = "http://cloud.ocrsdk.com/";
            IsSecureConnection = false;
            Proxy = WebRequest.DefaultWebProxy;
			RequestAuthSetup = new BasicRequestAuthSetup();
        }

        /// <summary>
        /// Url of the server
        /// On set, IsSecureConnection property is changed url contains protocol (http:// or https://)
        /// </summary>
        public string ServerUrl
        {
            get
            {
                if (IsSecureConnection)
                    return "https://" + _serverAddress;
                else
                    return "http://" + _serverAddress;
            }
            set
            {
                if (value.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                {
                    IsSecureConnection = false;
                }
                else if (value.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    IsSecureConnection = true;
                }

                // Trim http(s):// from the beginning
                _serverAddress = System.Text.RegularExpressions.Regex.Replace(value, "^https?://", "");
            }
        }

        public string ApplicationId { get; set; }
        public string Password { get; set; }

        public IWebProxy Proxy { get; set; }

		public IRequestAuthSetup RequestAuthSetup { get; set; }

        /// <summary>
        /// Does the connection use SSL or not. Set this property after ServerUrl
        /// </summary>
        public bool IsSecureConnection { get; set; }

        /// <summary>
        /// Upload a file to service synchronously and start processing
        /// </summary>
        /// <param name="filePath">Path to an image to process</param>
        /// <param name="settings">Language and output format</param>
        /// <returns>Id of the task. Check task status to see if you have enough units to process the task</returns>
        /// <exception cref="ProcessingErrorException">thrown when something goes wrong</exception>
        public Task ProcessImage(string filePath, ProcessingSettings settings)
        {
            string url = String.Format("{0}/processImage?{1}", ServerUrl,  settings.AsUrlParams);

            if (!String.IsNullOrEmpty(settings.Description))
            {
                url = url + "&description=" + Uri.EscapeDataString(settings.Description);
            }

            try
            {
                // Build post request
                WebRequest request = WebRequest.Create(url);
                setupPostRequest(url, request);
                writeFileToRequest(filePath, request);

                XDocument response = performRequest(request);
                Task task = ServerXml.GetTaskStatus(response);

                return task;
            }
            catch (System.Net.WebException e )
            {
                String friendlyMessage = retrieveFriendlyMessage( e );
				if (friendlyMessage != null)
				{
					throw new ProcessingErrorException(friendlyMessage, e);
				}
				throw new ProcessingErrorException("Cannot upload file", e);
            }
        }

        private string retrieveFriendlyMessage( System.Net.WebException fromException )
        {
            try
            {
                using (HttpWebResponse result = (HttpWebResponse)fromException.Response)
                {
                    // try extract the user-friendly text that might have been supplied
                    // by the service.
                    try
                    {
                        using (Stream stream = result.GetResponseStream())
                        {
                            XDocument responseXml = XDocument.Load( new XmlTextReader( stream ) );
                            XElement messageElement = responseXml.Root.Element("message");
                            String serviceMessage = messageElement.Value;
                            if (!String.IsNullOrEmpty(serviceMessage))
                            {
                                return serviceMessage;
                            }
                        }
                    } catch
                    {
                    }
                    try
                    {
                        String protocolMessage = result.StatusDescription;
                        if (!String.IsNullOrEmpty(protocolMessage))
                        {
                            return protocolMessage;
                        }
                    }
                    catch
                    {
                    }
                }
            } catch
            {
            }
            return null;
        }

        /// <summary>
        /// Upload image of a multipage document to server.
        /// </summary>
        /// <param name="filePath">Path to an image to process</param>
        /// <param name="taskToAddFile">Id of multipage document. If null, a new document is created</param>
        /// <returns>Id of document to which image was added</returns>
        public Task UploadAndAddFileToTask(string filePath, TaskId taskToAddFile )
        {
            string url = String.Format("{0}/submitImage", ServerUrl );
            if (taskToAddFile != null)
                url = url + "?taskId=" + Uri.EscapeDataString(taskToAddFile.ToString());

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task task = ServerXml.GetTaskStatus(response);

            return task;
        }

        public Task StartProcessingTask(TaskId taskId, ProcessingSettings settings)
        {
            string url = String.Format("{0}/processDocument?taskId={1}&{2}", ServerUrl, 
                Uri.EscapeDataString( taskId.ToString() ),
                settings.AsUrlParams);

            if (!String.IsNullOrEmpty(settings.Description))
            {
                url = url + "&description=" + Uri.EscapeDataString(settings.Description);
            }

            // Build get request
            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);
            XDocument response = performRequest(request);
            Task serverTask = ServerXml.GetTaskStatus(response);
            return serverTask;
        }

        /// <summary>
        /// Perform text recognition of a field
        /// Throws an exception if something goes wrong
        /// </summary>
        /// <returns>Id of created task</returns>
        public Task ProcessTextField(string filePath, TextFieldProcessingSettings settings)
        {
            string url = String.Format("{0}/processTextField{1}", ServerUrl, settings.AsUrlParams);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task task = ServerXml.GetTaskStatus(response);

            return task;
        }

        /// <summary>
        /// Perform barcode recognition of a field
        /// Throws an exception if something goes wrong
        /// </summary>
        /// <returns>Id of created task</returns>
        public Task ProcessBarcodeField(string filePath, BarcodeFieldProcessingSettings settings)
        {
            string url = String.Format("{0}/processBarcodeField{1}", ServerUrl, settings.AsUrlParams);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task task = ServerXml.GetTaskStatus(response);

            return task;
        }

        public Task ProcessCheckmarkField(string filePath, CheckmarkFieldProcessingSettings settings)
        {
            string url = String.Format("{0}/processCheckmarkField{1}", ServerUrl, settings.AsUrlParams);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task task = ServerXml.GetTaskStatus(response);

            return task;
        }

        public Task ProcessBusinessCard(string filePath, BusCardProcessingSettings settings)
        {
            string url = String.Format("{0}/processBusinessCard?{1}", ServerUrl, settings.AsUrlParams);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task serverTask = ServerXml.GetTaskStatus(response);
            return serverTask;
        }

        /// <summary>
        /// Perform fields recognition of uploaded document.
        /// </summary>
        /// <param name="task">Task created by UploadAndAddFileToTask method</param>
        /// <param name="settingsPath">Path to file with xml processing settings.</param>
        public Task ProcessFields(Task task, string settingsPath)
        {
            if (!File.Exists(settingsPath))
                throw new FileNotFoundException("Settings file doesn't exist.", settingsPath);

            string url = String.Format("{0}/processFields?taskId={1}", ServerUrl, task.Id);

            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(settingsPath, request);

            XDocument response = performRequest(request);
            Task result = ServerXml.GetTaskStatus(response);

            return result;
        }

        /// <summary>
        /// Recognize Machine-Readable Zone of an official document (Passport, ID, Visa etc)
        /// </summary>
        public Task ProcessMrz(string filePath)
        {
            string url = String.Format("{0}/processMRZ", ServerUrl);
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task serverTask = ServerXml.GetTaskStatus(response);
            return serverTask;
        }

        public Task CaptureData(string filePath, string templateName)
        {
            string url = String.Format("{0}/captureData?template={1}", ServerUrl, templateName);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            Task serverTask = ServerXml.GetTaskStatus(response);
            return serverTask;
        }
       

        public void DownloadUrl(string url, string outputFile)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                setupGetRequest(url, request);

                using (HttpWebResponse result = (HttpWebResponse) request.GetResponse())
                {
                    using (Stream stream = result.GetResponseStream())
                    {
                        // Write result directly to file
                        using (Stream file = File.OpenWrite(outputFile))
                        {
                            copyStream(stream, file);
                        }
                    }
                }
            }
            catch (System.Net.WebException e)
            {
                throw new ProcessingErrorException(e.Message, e);
            }
        }

        /// <summary>
        /// Download task that has finished processing and save it to given path
        /// </summary>
        /// <param name="task">Id of a task</param>
        /// <param name="outputFile">Path to save a file</param>
        public void DownloadResult(Task task, string outputFile)
        {
            if (task.Status != TaskStatus.Completed)
            {
                throw new ArgumentException("Cannot download result for not completed task");
            }

            try
            {
                if (File.Exists(outputFile))
                    File.Delete(outputFile);

               
                if (task.DownloadUrls == null || task.DownloadUrls.Count == 0)
                {
                    throw new ArgumentException("Cannot download task without download url");
                }

                string url = task.DownloadUrls[0];
                DownloadUrl(url, outputFile);
            }
            catch (System.Net.WebException e)
            {
                throw new ProcessingErrorException(e.Message, e);
            }
        }

        public Task GetTaskStatus(TaskId task)
        {
            string url = String.Format("{0}/getTaskStatus?taskId={1}", ServerUrl,
                Uri.EscapeDataString(task.ToString()));

            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);
            XDocument response = performRequest(request);
            Task serverTask = ServerXml.GetTaskStatus(response);
            return serverTask;
        }

        /// <summary>
        /// List all tasks modified within last 7 days
        /// </summary>
        public Task[] ListTasks()
        {
            DateTime now = DateTime.UtcNow;
            return ListTasks(now.AddDays(-7));
        }

        /// <summary>
        /// List all tasks which status changed since given UTC timestamp
        /// </summary>
        public Task[] ListTasks( DateTime changedSince )
        {
            string url = String.Format("{0}/listTasks?fromDate={1}", ServerUrl, 
                Uri.EscapeDataString(changedSince.ToUniversalTime().ToString("s")+"Z"));

            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);
            XDocument response = performRequest(request);

            Task[] tasks = ServerXml.GetAllTasks(response);
            return tasks;
        }

        /// <summary>
        /// Get list of tasks that are no more queued on a server.
        /// The tasks can be processed, failed, or not started becuase there is 
        /// not enough credits to process them.
        /// </summary>
        public Task[] ListFinishedTasks()
        {
            string url = String.Format("{0}/listFinishedTasks", ServerUrl);
            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);
            XDocument response = performRequest(request);

            Task[] tasks = ServerXml.GetAllTasks(response);
            return tasks;
        }

        /// <summary>
        /// Delete task on a server. This function cannot delete tasks that are being processed.
        /// </summary>
        public Task DeleteTask(Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.Deleted:
                case TaskStatus.InProgress:
                case TaskStatus.Unknown:
                    throw new ArgumentException("Invalid task status: " + task.Status + ". Cannot delete");
            }

            string url = String.Format("{0}/deleteTask?taskId={1}", ServerUrl, Uri.EscapeDataString(task.Id.ToString()));
            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);

            XDocument response = performRequest(request);
            Task serverTask = ServerXml.GetTaskStatus(response);
            return serverTask;
        }

        /// <summary>
        /// Activate application on a new mobile device
	    /// </summary>
        /// <param name="deviceId">string that uniquely identifies current device</param>
        /// <returns>string that should be added to application id for all API calls</returns>
        public string ActivateNewInstallation(string deviceId)
        {
            string url = String.Format("{0}/activateNewInstallation?deviceId={1}", ServerUrl, Uri.EscapeDataString(deviceId));
            WebRequest request = WebRequest.Create(url);
            setupGetRequest(url, request);

            XDocument response = performRequest(request);
            string installationId = response.Root.Elements().First().Value;

            return installationId;
        }

        #region Request management functions

        private static void copyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        private XDocument performRequest(WebRequest request)
        {
            using (HttpWebResponse result = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = result.GetResponseStream())
                {
                    return XDocument.Load( new XmlTextReader( stream ) );
                }
            }
        }

        private void setupRequest(string serverUrl, WebRequest request)
        {
            if (Proxy != null)
                request.Proxy = Proxy;

            // Support authentication in case url is ABBYY SDK
            if (serverUrl.StartsWith(ServerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
				RequestAuthSetup.Run(request, ApplicationId, Password);
            }

            // Set user agent string so that server is able to collect statistics
            ((HttpWebRequest)request).UserAgent = ".Net Cloud OCR SDK client";
        }

        private void setupPostRequest(string serverUrl, WebRequest request)
        {
            setupRequest(serverUrl, request);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
        }

        private void setupGetRequest(string serverUrl, WebRequest request)
        {
            setupRequest(serverUrl, request);
            request.Method = "GET";
        }

        private void writeFileToRequest( string filePath, WebRequest request )
		{
			using( BinaryReader reader = new BinaryReader( File.OpenRead( filePath ) ) ) {
				request.ContentLength = reader.BaseStream.Length;
				using( Stream stream = request.GetRequestStream() ) {
					byte[] buf = new byte[reader.BaseStream.Length];
					while( true ) {
						int bytesRead = reader.Read( buf, 0, buf.Length );
						if( bytesRead == 0 ) {
							break;
						}
						stream.Write( buf, 0, bytesRead );
					}
				}
			}
        }

        #endregion

        /// <summary>
        /// Address of the server excluding protocol
        /// </summary>
        private string _serverAddress;
    }


}
