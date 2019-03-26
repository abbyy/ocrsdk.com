using System;
using System.Net;
using System.Collections.Generic;
using System.IO;

namespace Sample
{

    class Processor
    {
        public Processor()
        {
            restClient = new RestServiceClient();
            restClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

            //!!! Please provide your application id and password in Config.txt
            // To create an application and obtain a password,
            // register at https://cloud.ocrsdk.com/Account/Register
            // More info on getting your application id and password at
            // https://ocrsdk.com/documentation/faq/#faq3

            // Name of application you created
            restClient.ApplicationId = "";
            // Password should be sent to your e-mail after application was created
            restClient.Password = "";
            var currentModuleLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string configFilePath = Path.Combine(currentModuleLocation, "Config.txt");
            if (File.Exists(configFilePath))
            {
                string[] lines = System.IO.File.ReadAllLines(configFilePath);
                foreach (string line in lines)
                {
                    const String appIdKey = "ApplicationId";
                    const String passwordKey = "Password";
                    const String serviceUrlKey = "ServiceUrl";
                    if (line.StartsWith("//"))
                    {
                        continue;
                    }
                    if (line.StartsWith(appIdKey))
                    {
                        restClient.ApplicationId = getValueByKey(line, appIdKey);
                    }
                    else if (line.StartsWith(passwordKey))
                    {
                        restClient.Password = getValueByKey(line, passwordKey);
                    }
                    else if (line.StartsWith(serviceUrlKey))
                    {
                        var serviceUrl = getValueByKey(line, serviceUrlKey);
                        serviceUrl = checkAndNormalizeServiceUrl(serviceUrl);
                        restClient.ServerUrl = serviceUrl;
                    }
                }
            }
            else
            {
                throw new Exception(
                    String.Format("Configuration file not found at path {0}", configFilePath));
            }

            // Display hint to provide credentials
            if (String.IsNullOrEmpty(restClient.ApplicationId) ||
                String.IsNullOrEmpty(restClient.Password))
            {
                throw new Exception("Please provide access credentials to Cloud OCR SDK service in Config.txt!");
            }

            Console.WriteLine(String.Format("Application id: {0}\n", restClient.ApplicationId));
        }

        public delegate void StepChangedActionHandler( string description );
        public StepChangedActionHandler StepChangedAction;

        public delegate void ProgressChangedActionHandler( int progress );
        public ProgressChangedActionHandler ProgressChangedAction;

        public string Process( string imagePath, ProcessingSettings settings )
        {
            string result = null;
            setProgress( 5 );

            setStep( "Uploading image..." );
            OcrSdkTask task = restClient.ProcessImage(imagePath, settings);

            setProgress( 70 );
            setStep("Processing...");
            task = waitForTask(task);

            if (task.Status == TaskStatus.Completed)
            {
                Console.WriteLine("Processing completed.");
                result = restClient.DownloadUrl(task.DownloadUrls[0]);
                setStep("Download completed.");
            }
            else if (task.Status == TaskStatus.NotEnoughCredits)
            {
                throw new Exception("Not enough credits to process the file. Please add more pages to your application balance.");
            }
            else
            {
                throw new Exception("Error while processing the task");
            }
            setProgress( 100 );
            return result;
        }

        private string getValueByKey(string line, string key)
        {
            string[] keyValue = line.Split( '=' );
            string value = "";
            if(keyValue.Length == 2 ) {
                value = keyValue[1].Trim();
            }
            return value;
        }

        private static string checkAndNormalizeServiceUrl(string serviceUrl)
        {
            Uri uriResult;
            var parsedOkay = Uri.TryCreate(serviceUrl, UriKind.Absolute, out uriResult);
            if (!parsedOkay || !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                throw new Exception(String.Format("Service URL '{0}' is invalid", serviceUrl));
            }
            return uriResult.ToString();
        }

        private OcrSdkTask waitForTask(OcrSdkTask task)
        {
            Console.WriteLine(String.Format("Task status: {0}", task.Status));
            while (task.IsTaskActive())
            {
                // Note: it's recommended that your application waits
                // at least 2 seconds before making the first getTaskStatus request
                // and also between such requests for the same task.
                // Making requests more often will not improve your application performance.
                // Note: if your application queues several files and waits for them
                // it's recommended that you use listFinishedTasks instead (which is described
                // at https://ocrsdk.com/documentation/apireference/listFinishedTasks/).
                System.Threading.Thread.Sleep(5000);
                task = restClient.GetTaskStatus(task.Id);
                Console.WriteLine(String.Format("Task status: {0}", task.Status));
            }
            return task;
        }

        private void setStep( string description )
        {
            if( StepChangedAction != null ) {
                StepChangedAction( description );
            }
        }

        private void setProgress( int progress )
        {
            if( ProgressChangedAction != null ) {
                if( progress > 100 ) {
                    progress = 100;
                }
                ProgressChangedAction( progress );
            }
        }

        private RestServiceClient restClient;
    }
}
