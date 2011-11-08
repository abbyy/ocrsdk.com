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
    class Test
    {
        private RestServiceClient restClient;
        private RestServiceClientAsync restClientAsync;

        public Test()
        {
            restClient = new RestServiceClient();
            restClient.ServerUrl = Properties.Settings.Default.ServerAddress;
            restClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

            if (!String.IsNullOrEmpty(Properties.Settings.Default.UserName))
                restClient.UserName = Properties.Settings.Default.UserName;

            if (!String.IsNullOrEmpty(Properties.Settings.Default.Password))
                restClient.Password = Properties.Settings.Default.Password;

            restClientAsync = new RestServiceClientAsync(restClient);

            Console.WriteLine(String.Format("User: {0}\n", restClient.UserName));
        }

        /// <summary>
        /// Process directory or file with given path
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="outputFilePath">Path to directory to store results
        /// Will be created if it doesn't exist
        /// </param>
        /// <param name="processAsDocument">If true, all images are processed as a single document</param>
        public void ProcessPath(string sourcePath, string outputPath, ProcessingSettings settings,
            bool processAsDocument)
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

            if (!processAsDocument || sourceFiles.Count == 1)
            {
                foreach (string filePath in sourceFiles)
                {
                    string outputFileName = Path.GetFileNameWithoutExtension(filePath);
                    string ext = settings.OutputFileExt;
                    string outputFilePath = Path.Combine(outputPath, outputFileName + ext);

                    Console.WriteLine("Processing " + Path.GetFileName(filePath));

                    ProcessFile(filePath, outputFilePath, settings);
                }
            }
            else
            {
                string outputFileName = "document";
                string ext = settings.OutputFileExt;
                string outputFilePath = Path.Combine(outputPath, outputFileName + ext);

                ProcessDocument(sourceFiles, outputFilePath, settings);
            }
        }

        public void ProcessFile(string sourceFilePath, string outputFilePath, ProcessingSettings settings)
        {
            Console.WriteLine("Uploading..");
            Task task = restClient.ProcessImage(sourceFilePath, settings);

            // For field-level
            /*
            var flSettings = new TextFieldProcessingSettings();
            TaskId taskId = restClient.ProcessTextField(sourceFilePath, flSettings);
             */

            TaskId taskId = task.Id;

            while (true)
            {
                task = restClient.GetTaskStatus(taskId);
                if (!Task.IsTaskActive(task.Status))
                    break;

                Console.WriteLine(String.Format("Task status: {0}", task.Status));
                System.Threading.Thread.Sleep(1000);
            }

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

        public void ProcessDocument(IEnumerable<string> _sourceFiles, string outputFilePath,
            ProcessingSettings settings)
        {
            string[] sourceFiles = _sourceFiles.ToArray();
            Console.WriteLine(String.Format("Recognizing {0} images as a document",
                sourceFiles.Length));

            TaskId taskId = null;
            for (int fileIndex = 0; fileIndex < sourceFiles.Length; fileIndex++)
            {
                string filePath = sourceFiles[fileIndex];
                Console.WriteLine("{0}: uploading {1}", fileIndex + 1, Path.GetFileName(filePath));

                taskId = restClient.UploadAndAddFileToTask(filePath, taskId);
            }

            // Start task
            Console.WriteLine("Starting task..");
            restClient.StartProcessingTask(taskId, settings);

            Task task = null;
            while (true)
            {
                task = restClient.GetTaskStatus(taskId);
                if (!Task.IsTaskActive(task.Status))
                    break;

                Console.WriteLine(String.Format("Task status: {0}", task.Status));
                System.Threading.Thread.Sleep(1000);
            }

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
    }
}
