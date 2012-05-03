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
        ProcessTextField
    };

    class Test
    {
        private RestServiceClient restClient;

        public Test()
        {
            restClient = new RestServiceClient();
            restClient.ServerUrl = Properties.Settings.Default.ServerAddress;
            restClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

            if (!String.IsNullOrEmpty(Properties.Settings.Default.ApplicationId))
                restClient.ApplicationId = Properties.Settings.Default.ApplicationId;

            if (!String.IsNullOrEmpty(Properties.Settings.Default.Password))
                restClient.Password = Properties.Settings.Default.Password;

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
                    string ext = fullTextSettings.OutputFileExt;
                    string outputFilePath = Path.Combine(outputPath, outputFileName + ext);

                    Console.WriteLine("Processing " + Path.GetFileName(filePath));

                    ProcessFile(filePath, outputFilePath, fullTextSettings);
                }
            }
            else if(processingMode == ProcessingModeEnum.MultiPage)
            {
                ProcessingSettings fullTextSettings = settings as ProcessingSettings;
                string outputFileName = "document";
                string ext = fullTextSettings.OutputFileExt;
                string outputFilePath = Path.Combine(outputPath, outputFileName + ext);

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
            else if (task.Status == TaskStatus.NotEnoughCredits)
            {
                Console.WriteLine("Not enough credits to process the file. Please add more pages to your application balance.");
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

        public void ProcessTextField(string sourceFilePath, string outputFilePath, TextFieldProcessingSettings settings)
        {
            Console.WriteLine("Uploading..");
            Task task = restClient.ProcessTextField(sourceFilePath, settings);

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
    }
}
