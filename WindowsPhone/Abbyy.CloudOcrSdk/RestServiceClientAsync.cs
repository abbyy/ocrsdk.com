using System;
using System.Net;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace Abbyy.CloudOcrSdk
{
    public class UploadCompletedEventArgs : ProgressChangedEventArgs
    {
        private Task _task;

        public UploadCompletedEventArgs(Task task, object userState) :
            base(50, userState)
        {
            _task = task;
        }

        public Task Result
        {
            get
            {
                return _task;
            }
        }

    }

    public class TaskEventArgs : AsyncCompletedEventArgs
    {
        private Task _task;

        public TaskEventArgs(Task task,
            Exception e, bool canceled, object state)
            : base(e, canceled, state)
        {
            _task = task;
        }

        public Task Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return _task;
            }
        }
    }

    public class RestServiceClientAsync
    {
        public RestServiceClientAsync(RestServiceClient client)
        {
            _syncClient = client;
        }

        /// <summary>
        /// Submit and process image asynchronously. 
        /// Performs callbacks:
        ///   UploadFileCompleted
        ///   TaskProcessingCompleted
        /// </summary>
        /// <param name="filePath">Path to file in isolated storage</param>
        /// <param name="settings"></param>
        /// <param name="userState"></param>
        public void ProcessImageAsync(string filePath, ProcessingSettings settings, object userState)
        {
            BackgroundWorker w = new BackgroundWorker();
            w.DoWork += new DoWorkEventHandler((sender, e) =>
                {
                    Task task = null;
                    try
                    {
                        task = _syncClient.ProcessImage(filePath, settings);
                        UploadCompletedEventArgs uploadArgs = new UploadCompletedEventArgs(task, userState);
                        onUploadFileCompleted(sender, uploadArgs);

                        // Wait until task finishes
                        while (true)
                        {
                            task = _syncClient.GetTaskStatus(task.Id);
                            if (!task.IsTaskActive())
                            {
                                break;
                            }
                            Thread.Sleep(1000);
                        }

                        if (task.Status == TaskStatus.NotEnoughCredits)
                        {
                            throw new Exception("Not enough credits to process image. Please add more pages to your application's account.");
                        }

                        TaskEventArgs taskArgs = new TaskEventArgs(task, null, false, userState);

                        onProcessingCompleted(sender, taskArgs);
                    }
                    catch (Exception ex)
                    {
                        TaskEventArgs taskArgs = new TaskEventArgs(task, ex, false, userState);
                        onProcessingCompleted(sender, taskArgs);
                    }
                }
            );

            w.RunWorkerAsync();
        }


        /// <summary>
        /// Download file asynchronously
        /// Performs DownloadFileCompleted callback
        /// </summary>
        public void DownloadFileAsync(Task task, string outputPath, object userState)
        {
            BackgroundWorker w = new BackgroundWorker();
            w.DoWork += new DoWorkEventHandler((sender, e) =>
                {
                    try
                    {
                        _syncClient.DownloadResult(task, outputPath);
                        TaskEventArgs taskArgs = new TaskEventArgs(task, null, false, userState);
                        onDownloadFileCompleted(sender, taskArgs);
                    }
                    catch (Exception ex)
                    {
                        TaskEventArgs taskArgs = new TaskEventArgs(task, ex, false, userState);
                        onDownloadFileCompleted(sender, taskArgs);
                    }
                }
                );

            w.RunWorkerAsync();
        }

        /// <summary>
        /// Called when file upload operation is completed
        /// </summary>
        public event EventHandler<UploadCompletedEventArgs> UploadFileCompleted;

        /// <summary>
        /// Called when server finished processing the task
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskProcessingCompleted;

        /// <summary>
        /// Called when recognition results were downloaded
        /// </summary>
        public event EventHandler<TaskEventArgs> DownloadFileCompleted;

        #region event calllers

        private void onUploadFileCompleted(object sender, UploadCompletedEventArgs e)
        {
            if (UploadFileCompleted != null)
            {
                UploadFileCompleted(sender, e);
            }
        }

        private void onProcessingCompleted(object sender, TaskEventArgs e)
        {
            if (TaskProcessingCompleted != null)
            {
                TaskProcessingCompleted(sender, e);
            }
        }

        private void onDownloadFileCompleted(object sender, TaskEventArgs e)
        {
            if (DownloadFileCompleted != null)
            {
                DownloadFileCompleted(sender, e);
            }
        }

        #endregion

        private RestServiceClient _syncClient;
    }
}
