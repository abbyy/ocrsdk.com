using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Abbyy.CloudOcrSdk
{
    public class UploadCompletedEventArgs : ProgressChangedEventArgs
    {
        private Task _task;

        public UploadCompletedEventArgs(Task task, object userState) :
            base( 50, userState )
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

    public class ListTaskEventArgs : AsyncCompletedEventArgs
    {
        private Task[] _tasks;

        public ListTaskEventArgs( Task[] tasks,
            Exception e, bool canceled, object state)
            : base(e, canceled, state)
        {
            _tasks = tasks;
        }

        public Task[] Result
        {
            get
            {
                RaiseExceptionIfNecessary();
                return _tasks;
            }
        }
    }

    public class RestServiceClientAsync
    {
        private RestServiceClient _syncClient;
        private TaskList _taskList;

        private Dictionary<object, AsyncOperation> processJobs = new Dictionary<object, AsyncOperation>();
        private Dictionary<object, AsyncOperation> downloadJobs = new Dictionary<object, AsyncOperation>();
        private Dictionary<object, AsyncOperation> taskListJobs = new Dictionary<object, AsyncOperation>();

        // Delegates used to start execution on a worker thread
        private delegate void downloadWorkerEventHandler(Task task, string filePath, AsyncOperation asyncOp);
        private delegate void processFileWorkerEventHandler(string filePath, IProcessingSettings settings, AsyncOperation asyncOp);
        private delegate void processFieldWorkerEventHandler(string filePath, IProcessingSettings settings, AsyncOperation asyncOp);
        private delegate void listTasksWorkerEventHandler(AsyncOperation asyncOp);


        #region Worker threads

        /// <summary>
        /// Enter infinite loop and wait for task to complete
        /// </summary>
        /// <returns>Details about completed task</returns>
        private Task waitUntilTaskFinishes(Task task)
        {
            
            while (true)
            {
                if (_taskList.Error != null)
                {
                    _taskList.DeleteTask(task.Id);
                    throw new Exception(_taskList.Error.Message, _taskList.Error);
                }

                TaskStatus taskStatus = _taskList.GetTaskStatus(task.Id);
                if (_taskList.IsTaskFinished(task.Id))
                {
                    Task result = _taskList.GetTask(task.Id);
                    _taskList.DeleteTask(task.Id);

                    return result;
                }

                Thread.Sleep(1000);
            }
        }

        // This method performs the actual prime number computation.
        // It is executed on the worker thread.
        private void processFileWorker(string filePath, IProcessingSettings settings,
            AsyncOperation asyncOp)
        {
            Exception e = null;

            // Check that the task is still active.
            // The operation may have been canceled before
            // the thread was scheduled.

            Task task = null;
            try
            {
                if (settings is ProcessingSettings)
                {
                    task = _syncClient.ProcessImage(filePath, settings as ProcessingSettings);
                }
                else if (settings is BusCardProcessingSettings)
                {
                    task = _syncClient.ProcessBusinessCard(filePath, settings as BusCardProcessingSettings);
                }
                else if (settings is CaptureDataSettings)
                {
                    string templateName = (settings as CaptureDataSettings).TemplateName;
                    task = _syncClient.CaptureData(filePath, templateName);
                }

                // Notify subscriber that upload completed
                Task uploadedTask = new Task(task.Id, TaskStatus.Submitted);
                UploadCompletedEventArgs uploadCompletedEventArgs = new UploadCompletedEventArgs(uploadedTask, asyncOp.UserSuppliedState);
                asyncOp.Post(onUploadCompletedDelegate, uploadCompletedEventArgs);

                startTaskMonitorIfNecessary();

                _taskList.AddTask(task);

                task = waitUntilTaskFinishes(task); // task is modified on server
            }
            catch (Exception ex)
            {
                e = ex;
            }

            processCompletionMethod(task, e, false, asyncOp);
        }

        private void processFieldWorker(string filePath, IProcessingSettings settings,
            AsyncOperation asyncOp)
        {
            Exception e = null;

            Task task = new Task();
            try
            {
                if (settings is TextFieldProcessingSettings)
                {
                    task = _syncClient.ProcessTextField(filePath, settings as TextFieldProcessingSettings);
                }
                else if (settings is BarcodeFieldProcessingSettings)
                {
                    task = _syncClient.ProcessBarcodeField(filePath, settings as BarcodeFieldProcessingSettings);
                }
                else if (settings is CheckmarkFieldProcessingSettings)
                {
                    task = _syncClient.ProcessCheckmarkField(filePath, settings as CheckmarkFieldProcessingSettings);
                }
                else
                {
                    throw new ArgumentException("Invalid type of processing settings");
                }

                // Notify that upload was completed
                Task uploadedTask = new Task(task.Id, TaskStatus.Submitted);
                UploadCompletedEventArgs uploadCompletedEventArgs = new UploadCompletedEventArgs(uploadedTask, asyncOp.UserSuppliedState);
                asyncOp.Post(onUploadCompletedDelegate, uploadCompletedEventArgs);

                // Wait until task finishes
                startTaskMonitorIfNecessary();

                _taskList.AddTask(task);
                task = waitUntilTaskFinishes(task);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            lock (processJobs)
            {
                processJobs.Remove(asyncOp.UserSuppliedState);
            }

            bool canceled = false;

            // Package the results of the operation in EventArgs
            TaskEventArgs ev = new TaskEventArgs(task, e, canceled, asyncOp.UserSuppliedState);

            // End the task. The asyncOp object is responsible 
            // for marshaling the call.
            asyncOp.PostOperationCompleted(onProcessingCompleteDelegate, ev);
        }

        private void downloadFileWorker(Task task, string outputFilePath,
            AsyncOperation asyncOp)
        {
            Exception e = null;

            try
            {
                _syncClient.DownloadResult(task, outputFilePath);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            downloadCompletionMethod(task, e, false, asyncOp);
        }

        private void listTasksWorker(AsyncOperation asyncOp)
        {
            Exception e = null;

            Task[] tasks = null;
            try
            {
                tasks = _syncClient.ListTasks();
            }
            catch (Exception ex)
            {
                e = ex;
            }

            lock (downloadJobs)
            {
                taskListJobs.Remove(asyncOp.UserSuppliedState);
            }

            ListTaskEventArgs args = new ListTaskEventArgs(tasks, e, false, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onListTasksCompletedDelegate, args);
        }

        #endregion

        #region Completion methods
        // This is the method that the underlying, free-threaded 
        // asynchronous behavior will invoke.  This will happen on
        // a worker thread
        private void processCompletionMethod(
            Task task,
            Exception exception,
            bool canceled,
            AsyncOperation asyncOp)
        {
            // If the task was not previously canceled,
            // remove the task from the lifetime collection.
            if (!canceled)
            {
                lock (processJobs)
                {
                    processJobs.Remove(asyncOp.UserSuppliedState);
                }
            }

            // Package the results of the operation in EventArgs
            TaskEventArgs e = new TaskEventArgs(task, exception, canceled, asyncOp.UserSuppliedState);

            // End the task. The asyncOp object is responsible 
            // for marshaling the call.
            asyncOp.PostOperationCompleted(onProcessingCompleteDelegate, e);

            // Note that after the call to OperationCompleted, 
            // asyncOp is no longer usable, and any attempt to use it
            // will cause an exception to be thrown.
        }

        private void downloadCompletionMethod(
            Task task,
            Exception exception,
            bool canceled,
            AsyncOperation asyncOp)
        {
            if (!canceled)
            {
                lock (downloadJobs)
                {
                    downloadJobs.Remove(asyncOp.UserSuppliedState);
                }
            }

            TaskEventArgs e = new TaskEventArgs(task, exception, canceled, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onDownloadCompletedDelegate, e);
        }

        #endregion

        #region Events in user threads

        protected virtual void initializeDelegates()
        {
            onUploadCompletedDelegate = new SendOrPostCallback(uploadCompleted);
            onProcessingCompleteDelegate = new SendOrPostCallback(processingCompleted);
            onDownloadCompletedDelegate = new SendOrPostCallback(downloadCompleted);
            onListTasksCompletedDelegate = new SendOrPostCallback(listTasksCompleted);
        }

        private SendOrPostCallback onUploadCompletedDelegate;
        private SendOrPostCallback onProcessingCompleteDelegate;
        private SendOrPostCallback onDownloadCompletedDelegate;
        private SendOrPostCallback onListTasksCompletedDelegate;

        // This method is invoked via the AsyncOperation object,
        // so it is guaranteed to be executed on the correct thread.
        private void uploadCompleted(object operationState)
        {
            UploadCompletedEventArgs e = operationState as UploadCompletedEventArgs;

            onUploadFileCompleted(null, e);
        }

        private void processingCompleted(object operationState)
        {
            TaskEventArgs e = operationState as TaskEventArgs;

            onProcessingCompleted(null, e);
        }

        private void downloadCompleted(object operationState)
        {
            TaskEventArgs e = operationState as TaskEventArgs;

            onDownloadFileCompleted(null, e);
        }

        private void listTasksCompleted(object operationState)
        {
            ListTaskEventArgs e = operationState as ListTaskEventArgs;

            onListTasksCompleted(null, e);
        }

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

        private void onListTasksCompleted(object sender, ListTaskEventArgs e)
        {
            if (ListTasksCompleted != null)
            {
                ListTasksCompleted(sender, e);
            }
        }

        #endregion

        BackgroundWorker taskListWorker = null;

        /// <summary>
        /// Start thread that waits for completion of tasks
        /// </summary>
        private void startTaskMonitorIfNecessary()
        {
            if (taskListWorker == null || !taskListWorker.IsBusy)
            {
                lock (this)
                {
                    if (taskListWorker != null && taskListWorker.IsBusy)
                        return;

                    taskListWorker = new BackgroundWorker();
                    taskListWorker.DoWork += new DoWorkEventHandler(_taskList.Start);
                    taskListWorker.RunWorkerAsync();
                }
            }
        }

        /// <summary>
        /// Upload file and start recognition asynchronously
        /// Performs callbacks:
        ///   UploadFileCompleted
        ///   TaskProcessingCompleted
        /// </summary>
        private void processFileAsync(string filePath, IProcessingSettings settings, object taskId)
        {
            // Create an AsyncOperation for taskId.
            AsyncOperation asyncOp =
                AsyncOperationManager.CreateOperation(taskId);

            // Multiple threads will access the task dictionary,
            // so it must be locked to serialize access.
            lock (processJobs)
            {
                if (processJobs.ContainsKey(taskId))
                {
                    throw new ArgumentException(
                        "Task ID parameter must be unique",
                        "taskId");
                }

                processJobs[taskId] = asyncOp;
            }

            // Start the asynchronous operation.
            processFileWorkerEventHandler workerDelegate = new processFileWorkerEventHandler(processFileWorker);
            workerDelegate.BeginInvoke(
                filePath, settings,
                asyncOp,
                null,
                null);
        }

        /// <summary>
        /// Start one of 3 possible field-processing tasks
        /// </summary>
        private void processFieldAsync(string filePath, IProcessingSettings settings, object taskId)
        {
            AsyncOperation asynOp = AsyncOperationManager.CreateOperation(taskId);
            lock (processJobs)
            {
                if (processJobs.ContainsKey(taskId))
                {
                    throw new ArgumentException("Task ID parameter must be unique", "taskId");
                }

                processJobs[taskId] = asynOp;
            }

            processFieldWorkerEventHandler workerDelegate = new processFieldWorkerEventHandler(processFieldWorker);
            workerDelegate.BeginInvoke(
                filePath, settings,
                asynOp,
                null, null);
        }

        public RestServiceClientAsync(RestServiceClient syncClient)
        {
            _syncClient = syncClient;
            _taskList = new TaskList(_syncClient);

            initializeDelegates();
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

        public event EventHandler<ListTaskEventArgs> ListTasksCompleted;

        public void ProcessImageAsync(string filePath, ProcessingSettings settings, object taskId)
        {
            processFileAsync(filePath, settings, taskId);
        }

        public void ProcessBusinessCardAsync(string filePath, BusCardProcessingSettings settings, object taskId)
        {
            processFileAsync(filePath, settings, taskId);
        }

        public void CaptureDataAsync(string filePath, string templateName, object taskId)
        {
            CaptureDataSettings settings = new CaptureDataSettings();
            settings.TemplateName = templateName;
            processFileAsync(filePath, settings, taskId);
        }

        /// <summary>
        /// Call ProcessTextField asynchronously.
        /// Performs callbacks:
        ///   UploadFileCompleted
        ///   TaskProcessingCompleted
        /// </summary>
        public void ProcessTextFieldAsync(string filePath, TextFieldProcessingSettings settings, object taskId)
        {
            processFieldAsync(filePath, settings, taskId);
        }

        /// <summary>
        /// Call ProcessBarcodeField asynchronously.
        /// Performs callbacks:
        ///   UploadFileCompleted
        ///   TaskProcessingCompleted
        /// </summary>
        public void ProcessBarcodeFieldAsync(string filePath, BarcodeFieldProcessingSettings settings, object taskId)
        {
            processFieldAsync(filePath, settings, taskId);
        }

        /// <summary>
        /// Call ProcessCheckmarkField asynchronously.
        /// Performs callbacks:
        ///   UploadFileCompleted
        ///   TaskProcessingCompleted
        /// </summary>
        public void ProcessCheckmarkFieldAsync(string filePath, CheckmarkFieldProcessingSettings settings, object taskId)
        {
            processFieldAsync(filePath, settings, taskId);
        }

        /// <summary>
        /// Download file asynchronously
        /// Performs DownloadFileCompleted callback
        /// </summary>
        public void DownloadFileAsync(Task task, string outputPath, object userTaskId)
        {
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userTaskId);

            lock (downloadJobs)
            {
                if (downloadJobs.ContainsKey(userTaskId))
                {
                    throw new ArgumentException("Task ID parameter must be unique", "userTaskId");
                }
                downloadJobs[userTaskId] = asyncOp;
            }

            // Start the asynchronous operation.
            downloadWorkerEventHandler workerDelegate = new downloadWorkerEventHandler(downloadFileWorker);
            workerDelegate.BeginInvoke(task, outputPath, asyncOp,
                null, null);
        }

        public void ListTasksAsync( object userTaskId )
        {
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(userTaskId);

            lock (taskListJobs)
            {
                if (taskListJobs.ContainsKey(userTaskId))
                {
                    throw new ArgumentException("Task ID parameter must be unique", "userTaskId");
                }
                taskListJobs[userTaskId] = asyncOp;
            }

            // Start the asynchronous operation.
            listTasksWorkerEventHandler workerDelegate = new listTasksWorkerEventHandler(listTasksWorker);
            workerDelegate.BeginInvoke(asyncOp, null, null);
        }

        private class CaptureDataSettings : IProcessingSettings
        {
            public string AsUrlParams
            {
                get { throw new NotImplementedException(); }
            }

            public string TemplateName = "MRZ";
        }
    }
}
