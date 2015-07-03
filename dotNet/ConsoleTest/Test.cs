using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Abbyy.CloudOcrSdk;

namespace ConsoleTest
{
    public enum ProcessingModeEnum
    {
        SinglePage,
        MultiPage,
        ProcessTextField,
        ProcessFields,
        ProcessMrz,
    };

    class Test
    {
        private RestServiceClient restClient;

        public Test()
        {
            restClient = new RestServiceClient();
            restClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

            //!!! Please provide your application id and password here
            // To create an application and obtain a password,
            // register at http://cloud.ocrsdk.com/Account/Register
            // More info on getting your application id and password at
            // http://ocrsdk.com/documentation/faq/#faq3

            /*
			// Name of application you created
            restClient.ApplicationId = "<your application id>";
			// Password should be sent to your e-mail after application was created
            restClient.Password = "<your password>";
             */

            // Display hint to provide credentials
            if (String.IsNullOrEmpty(restClient.ApplicationId) ||
                String.IsNullOrEmpty(restClient.Password))
            {
                throw new Exception("Please provide access credentials to Cloud OCR SDK service! See Test.cs file for details!");
            }

            Console.WriteLine(String.Format("Application id: {0}\n", restClient.ApplicationId));
        }

        /// <summary>
        /// Process directory or file with given path
        /// </summary>
        /// <param name="sourcePath">File or directory to be processed</param>
        /// <param name="outputPath">Path to directory to store results
        /// Will be created if it doesn't exist
        /// </param>
        /// <param name="processAsDocument">If true, all images are processed as a single document</param>
        public void ProcessPath(string sourcePath, string outputPath, 
            IProcessingSettings settings,
            ProcessingModeEnum processingMode)
        {
            List<string> sourceFiles = new List<string>();
            if (Directory.Exists(sourcePath))
            {
                sourceFiles.AddRange(Directory.GetFiles(sourcePath));
                sourceFiles.Sort();
            }
            else if (File.Exists(sourcePath))
            {
                sourceFiles.Add(sourcePath);
            }
            else
            {
                Console.WriteLine("Invalid source path");
                return;
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            if (processingMode == ProcessingModeEnum.SinglePage || 
                (processingMode == ProcessingModeEnum.MultiPage && sourceFiles.Count == 1) )
            {
                ProcessingSettings fullTextSettings = settings as ProcessingSettings;
                foreach (string filePath in sourceFiles)
                {
                    string outputFileName = Path.GetFileNameWithoutExtension(filePath);
                    string outputFilePath = Path.Combine(outputPath, outputFileName);

                    Console.WriteLine("Processing " + Path.GetFileName(filePath));

                    ProcessFile(filePath, outputFilePath, fullTextSettings);
                }
            }
            else if(processingMode == ProcessingModeEnum.MultiPage)
            {
                ProcessingSettings fullTextSettings = settings as ProcessingSettings;
                string outputFileName = "document";
                string outputFilePath = Path.Combine(outputPath, outputFileName);

                ProcessDocument(sourceFiles, outputFilePath, fullTextSettings);
            }
            else if (processingMode == ProcessingModeEnum.ProcessTextField)
            {
                TextFieldProcessingSettings fieldSettings = settings as TextFieldProcessingSettings;
                foreach (string filePath in sourceFiles)
                {
                    string outputFileName = Path.GetFileNameWithoutExtension(filePath);
                    string ext = ".xml";
                    string outputFilePath = Path.Combine(outputPath, outputFileName + ext);

                    Console.WriteLine("Processing " + Path.GetFileName(filePath));

                    ProcessTextField(filePath, outputFilePath, fieldSettings);
                }
            }
            else if (processingMode == ProcessingModeEnum.ProcessMrz)
            {
                foreach (string filePath in sourceFiles)
                {
                    string outputFileName = Path.GetFileNameWithoutExtension(filePath);
                    string ext = ".xml";
                    string outputFilePath = Path.Combine(outputPath, outputFileName + ext);

                    Console.WriteLine("Processing " + Path.GetFileName(filePath));

                    ProcessMrz(filePath, outputFilePath);
                }
            }
        }

        public void ProcessFile(string sourceFilePath, string outputFileBase, ProcessingSettings settings)
        {
            Console.WriteLine("Uploading..");
            Task task = restClient.ProcessImage(sourceFilePath, settings);

            task = waitForTask(task);

            if (task.Status == TaskStatus.Completed)
            {
                Console.WriteLine("Processing completed.");
                for (int i = 0; i < settings.OutputFormats.Count; i++)
                {
                    var outputFormat = settings.OutputFormats[i];
                    string ext = settings.GetOutputFileExt(outputFormat);
                    restClient.DownloadUrl(task.DownloadUrls[i], outputFileBase + ext);
                }
                Console.WriteLine("Download completed.");
            }
            else if (task.Status == TaskStatus.NotEnoughCredits)
            {
                Console.WriteLine("Not enough credits to process the file. Please add more pages to your application balance.");
            }
            else
            {
                Console.WriteLine("Error while processing the task");
            }
        }

        public void ProcessDocument(IEnumerable<string> _sourceFiles, string outputFileBase,
            ProcessingSettings settings)
        {
            string[] sourceFiles = _sourceFiles.ToArray();
            Console.WriteLine(String.Format("Recognizing {0} images as a document",
                sourceFiles.Length));

            Task task = null;
            for (int fileIndex = 0; fileIndex < sourceFiles.Length; fileIndex++)
            {
                string filePath = sourceFiles[fileIndex];
                Console.WriteLine("{0}: uploading {1}", fileIndex + 1, Path.GetFileName(filePath));

                task = restClient.UploadAndAddFileToTask(filePath, task == null ? null : task.Id);
            }

            // Start task
            Console.WriteLine("Starting task..");
            task = restClient.StartProcessingTask(task.Id, settings);

            task = waitForTask(task);

            if (task.Status == TaskStatus.Completed)
            {
                Console.WriteLine("Processing completed.");
                for( int i = 0; i < settings.OutputFormats.Count; i++ ) 
                {
                    var outputFormat = settings.OutputFormats[i];
                    string ext = settings.GetOutputFileExt(outputFormat);
                    restClient.DownloadUrl(task.DownloadUrls[i], outputFileBase + ext);
                }
                Console.WriteLine("Download completed.");
            }
            else
            {
                Console.WriteLine("Error while processing the task");
            }
        }

        /// <summary>
        /// Wait until task finishes and download result
        /// </summary>
        private void waitAndDownload(Task task, string outputFilePath)
        {
            task = waitForTask(task);

            if (task.Status == TaskStatus.Completed)
            {
                Console.WriteLine("Processing completed.");
                restClient.DownloadResult(task, outputFilePath);
                Console.WriteLine("Download completed.");
            }
            else
            {
                Console.WriteLine("Error while processing the task");
            }
        }

        private Task waitForTask(Task task)
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
                // at http://ocrsdk.com/documentation/apireference/listFinishedTasks/).
                System.Threading.Thread.Sleep(5000);
                task = restClient.GetTaskStatus(task.Id);
                Console.WriteLine(String.Format("Task status: {0}", task.Status));
            }
            return task;
        }

        public void ProcessTextField(string sourceFilePath, string outputFilePath, TextFieldProcessingSettings settings)
        {
            Console.WriteLine("Uploading..");
            Task task = restClient.ProcessTextField(sourceFilePath, settings);

            waitAndDownload(task, outputFilePath);
        }

        public void ProcessFields(string sourceFilePath, string xmlSettingsPath, string outputFilePath)
        {
            Console.WriteLine("Uploading");
            Task task = restClient.UploadAndAddFileToTask(sourceFilePath, null);
            Console.WriteLine("Processing..");
            task = restClient.ProcessFields(task, xmlSettingsPath);

            waitAndDownload(task, outputFilePath);
        }

        public void ProcessMrz(string sourceFilePath, string outputFilePath)
        {
            Console.WriteLine("Uploading");
            Task task = restClient.ProcessMrz(sourceFilePath);
            Console.WriteLine("Processing..");

            waitAndDownload(task, outputFilePath);
        }
    }
}
