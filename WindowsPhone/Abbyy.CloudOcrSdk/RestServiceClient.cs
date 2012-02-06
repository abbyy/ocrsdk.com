using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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
        public static bool IsTaskActive(TaskStatus status)
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
            string url = String.Format("{0}/processImage?{1}", ServerUrl, settings.AsUrlParams);

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
            catch (System.Net.WebException e)
            {
                throw new ProcessingErrorException("Cannot upload file: " + e.Message, e);
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

        public void DownloadResult(Task task, string outputFile)
        {
            if (task.Status != TaskStatus.Completed)
            {
                throw new ArgumentException("Cannot download result for not completed task");
            }

            try
            {
                if (task.DownloadUrl == null)
                {
                    throw new ArgumentException("Cannot download task without download url");
                }

                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (storage.FileExists(outputFile))
                        storage.DeleteFile(outputFile);

                    string url = task.DownloadUrl;

                    WebRequest request = WebRequest.Create(url);
                    setupGetRequest(url, request);

                    using (HttpWebResponse result = (HttpWebResponse)request.GetResponse())
                    {
                        using (Stream stream = result.GetResponseStream())
                        {
                            // Write result directly to file
                            using (Stream file = storage.OpenFile(outputFile, FileMode.Create))
                            {
                                copyStream(stream, file);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException e)
            {
                throw new ProcessingErrorException(e.Message, e);
            }
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
                    return XDocument.Load(stream);
                }
            }
        }

        private void setupRequest(string serverUrl, WebRequest request)
        {
            // Support authentication in case url is ABBYY SDK
            if (serverUrl.StartsWith(ServerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                request.Credentials = new NetworkCredential(ApplicationId, Password);
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

        private void writeFileToRequest(string filePath, WebRequest request)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream file = storage.OpenFile(filePath, FileMode.Open);
                using (BinaryReader reader = new BinaryReader(file))
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        byte[] buf = new byte[reader.BaseStream.Length];
                        while (true)
                        {
                            int bytesRead = reader.Read(buf, 0, buf.Length);
                            if (bytesRead == 0)
                            {
                                break;
                            }
                            stream.Write(buf, 0, bytesRead);
                        }
                    }
                }
                file.Close();
            }
        }

        #endregion

        /// <summary>
        /// Address of the server excluding protocol
        /// </summary>
        private string _serverAddress;
    }
}
