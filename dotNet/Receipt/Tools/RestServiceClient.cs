using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sample
{
    public class ProcessingErrorException : System.Net.WebException
    {
        public ProcessingErrorException(string message, System.Net.WebException e)
            : base(message, e)
        {
        }
    }

    public class OcrSdkTask
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
        /// Number of files in the task
        /// </summary>
        public int FilesCount = 1;

        /// <summary>
        /// Task cost in credits
        /// </summary>
        public int Credits = 0;

        /// <summary>
        /// Task description provided by user
        /// </summary>
        public string Description = null;

        /// <summary>
        /// Url to download processed tasks. In our case it has only one member
        /// </summary>
        public List<string> DownloadUrls = null;

        /// <summary>
        /// Error description when task processing failed
        /// </summary>
        public string Error = null;

        public OcrSdkTask()
        {
            Status = TaskStatus.Unknown;
            Id = new TaskId("<unknown>");
        }

        public OcrSdkTask(TaskId id, TaskStatus status)
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
            // Important note!!!
            // Below is the optimal way to setup authentication.
            // When using HttpClient do set
            // HttpClient.DefaultRequestHeaders.Authorization instead as
            // described here http://stackoverflow.com/a/23914662
            // Settings .Credentials property (as well as setting
            // HttpClientHandler.Credentials when using HttpClient)
            // causes suboptimal application behavior, unneeded extra
            // roundtrips and reduced performance.
            // In details, if you set .Credentials then the request is first sent
            // without authentication headers, gets rejected with HTTP 401
            // and is resent. That would double the number of requests and reduce
            // your application performance. The code below ensures that the
            // right headers are sent every time and this gets better performance.
            // It is highly recommended that you use Fiddler or equivalent software
            // to validate how your application interacts with the service.
			Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string toEncode = username + ":" + password;
            string baseEncoded = Convert.ToBase64String(encoding.GetBytes(toEncode));
			request.Headers.Add( "Authorization", "Basic " + baseEncoded );
		}
	}

    public class RestServiceClient
    {
        public RestServiceClient()
        {
            ServerUrl = "http://cloud.ocrsdk.com/";
            Proxy = WebRequest.DefaultWebProxy;
			RequestAuthSetup = new BasicRequestAuthSetup();
        }

        /// <summary>
        /// Url of the server
        /// </summary>
        public string ServerUrl
        {
            get
            {
                return _serverAddress;
            }
            set
            {
                // validate and normalize the URL
                Uri uri = new Uri(value);
                _serverAddress = uri.ToString();
            }
        }

        public string ApplicationId { get; set; }
        public string Password { get; set; }

        public IWebProxy Proxy { get; set; }

		public IRequestAuthSetup RequestAuthSetup { get; set; }

        /// <summary>
        /// Upload a file to service synchronously and start processing
        /// </summary>
        /// <param name="filePath">Path to an image to process</param>
        /// <param name="settings">Language and output format</param>
        /// <returns>Id of the task. Check task status to see if you have enough units to process the task</returns>
        /// <exception cref="ProcessingErrorException">thrown when something goes wrong</exception>
        public OcrSdkTask ProcessImage(string filePath, ProcessingSettings settings )
        {
            string url = String.Format("{0}processReceipt?{1}", ServerUrl,  settings.AsUrlParams);

            try
            {
                // Build post request
                WebRequest request = createPostRequest(url);
                writeFileToRequest(filePath, request);

                XDocument response = performRequest(request);
                OcrSdkTask task = ServerXml.GetTaskStatus(response);

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
                        if (result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return "Credentials to Cloud OCR SDK service are not valid! Try to enter new values in Config.txt!";
                        }
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
        public OcrSdkTask UploadAndAddFileToTask(string filePath, TaskId taskToAddFile)
        {
            string url = String.Format("{0}submitImage", ServerUrl);
            if (taskToAddFile != null)
                url = url + "?taskId=" + Uri.EscapeDataString(taskToAddFile.ToString());

            // Build post request
            WebRequest request = createPostRequest(url);
            writeFileToRequest(filePath, request);

            XDocument response = performRequest(request);
            OcrSdkTask task = ServerXml.GetTaskStatus(response);

            return task;
        }

        public string DownloadUrl(string url)
        {
            try
            {
                WebRequest request = createGetRequest(url);

                string result = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }
                return result;
            }
            catch (System.Net.WebException e)
            {
                throw new ProcessingErrorException(e.Message, e);
            }
        }

        public OcrSdkTask GetTaskStatus(TaskId task)
        {
            string url = String.Format("{0}getTaskStatus?taskId={1}", ServerUrl,
                Uri.EscapeDataString(task.ToString()));

            WebRequest request = createGetRequest(url);
            XDocument response = performRequest(request);
            OcrSdkTask serverTask = ServerXml.GetTaskStatus(response);
            return serverTask;
        }

        #region Request management functions

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

        HttpWebRequest createRequest(String url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            setupRequest(url, request);
            return request;
        }

        private void setupRequest(string serverUrl, HttpWebRequest request)
        {
            if (Proxy != null)
                request.Proxy = Proxy;

            // Support authentication in case url is ABBYY SDK
            // Warning! Please read the important note in BasicRequestAuthSetup.Run()
            // before trying to reimplement Basic authentication
            if (serverUrl.StartsWith(ServerUrl, StringComparison.InvariantCultureIgnoreCase))
            {
				RequestAuthSetup.Run(request, ApplicationId, Password);
            }

            // Set user agent string so that the service is able to collect statistics
            {
                var userAgentStoredSetting = System.Configuration.ConfigurationManager.AppSettings["HttpClientUserAgent"];
                if (String.IsNullOrEmpty(userAgentStoredSetting))
                {
                    request.UserAgent = ".NET Cloud OCR SDK Client";
                }
                else
                {
                    request.UserAgent = userAgentStoredSetting;
                }
            }
        }

        private HttpWebRequest createPostRequest(string url)
        {
            HttpWebRequest request = createRequest(url);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            return request;
        }

        private HttpWebRequest createGetRequest(string url)
        {
            HttpWebRequest request = createRequest(url);
            request.Method = "GET";
            return request;
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
