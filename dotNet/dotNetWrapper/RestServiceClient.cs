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
        public string DownloadUrl = null;

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

   
   

    public class RestServiceClient
    {
        public RestServiceClient()
        {
            ServerUrl = "http://cloud.ocrsdk.com/";
            IsSecureConnection = false;
            Proxy = WebRequest.DefaultWebProxy;

            // FIXME: remove this after server has valid certificate
            System.Net.ServicePointManager.ServerCertificateValidationCallback += 
                (se, cert, chain, sslerror) =>
            {
                return true;
            };

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

        public string UserName { get; set; }
        public string Password { get; set; }

        public IWebProxy Proxy { get; set; }

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
                throw new ProcessingErrorException("Cannot upload file: " + e.Message, e);
            }
        }

        /// <summary>
        /// Upload image of a multipage document to server.
        /// </summary>
        /// <param name="filePath">Path to an image to process</param>
        /// <param name="taskToAddFile">Id of multipage document. If null, a new document is created</param>
        /// <returns>Id of document to which image was added</returns>
        public TaskId UploadAndAddFileToTask(string filePath, TaskId taskToAddFile )
        {
            string url = String.Format("{0}/submitImage", ServerUrl );
            if (taskToAddFile != null)
                url = url + "?taskId=" + Uri.EscapeDataString(taskToAddFile.ToString());

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            TaskId taskId = ServerXml.GetTaskId(response);

            return taskId;
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
        public TaskId ProcessTextField(string filePath, TextFieldProcessingSettings settings)
        {
            string url = String.Format("{0}/processTextField{1}", ServerUrl, settings.AsUrlParams);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            TaskId taskId = ServerXml.GetTaskId(response);

            return taskId;
        }

        /// <summary>
        /// Perform barcode recognition of a field
        /// Throws an exception if something goes wrong
        /// </summary>
        /// <returns>Id of created task</returns>
        public TaskId ProcessBarcodeField(string filePath, BarcodeFieldProcessingSettings settings)
        {
            string url = String.Format("{0}/processBarcodeField{1}", ServerUrl, settings.AsUrlParams);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            TaskId taskId = ServerXml.GetTaskId(response);

            return taskId;
        }

        public TaskId ProcessCheckmarkField(string filePath, CheckmarkFieldProcessingSettings settings)
        {
            string url = String.Format("{0}/processCheckmarkField{1}", ServerUrl, settings.AsUrlParams);

            // Build post request
            WebRequest request = WebRequest.Create(url);
            setupPostRequest(url, request);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            TaskId taskId = ServerXml.GetTaskId(response);

            return taskId;
        }

        /// <summary>
        /// Download filePath that has finished processing and save it to given path
        /// </summary>
        /// <param name="task">Id of a task</param>
        /// <param name="outputFile">Path to save a filePath</param>
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

               
                if (task.DownloadUrl == null)
                {
                    throw new ArgumentException("Cannot download task without download url");
                }

                string url = task.DownloadUrl;

                // Emergency code. In normal situations it shouldn't be called
                /*
                url = String.Format("{0}/{1}?taskId={2}", ServerUrl, _getResultUrl,
                    Uri.EscapeDataString(task.Id.ToString()));
                 */

                WebRequest request = WebRequest.Create(url);
                setupGetRequest(url, request);

                using (HttpWebResponse result = (HttpWebResponse)request.GetResponse())
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

        private string encodeUserPassword(string user, string pass)
        {
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string toEncode = user + ":" + pass;
            string baseEncoded = Convert.ToBase64String(encoding.GetBytes(toEncode));
            return baseEncoded;
        }

        private void setupRequest(string serverUrl, WebRequest request)
        {
            if (Proxy != null)
                request.Proxy = Proxy;

            // Support authentication in case url is ABBYY SDK
            if (serverUrl.StartsWith(ServerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                CredentialCache cache = new CredentialCache();
                cache.Add(new Uri(serverUrl), "Digest", new NetworkCredential(UserName, Password));
                request.Credentials = cache;
                request.Timeout = 300 * 1000;
                request.Headers.Add("Authorization", "Basic: " + encodeUserPassword(UserName, Password));
            }
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
