using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abbyy.CloudOcrSdk
{
    /// <summary>
    /// List of tasks on the server
    /// </summary>
    class TaskList
    {
        private Dictionary<TaskId, Task> allTasks = new Dictionary<TaskId, Task>();
        private RestServiceClient _restClient;
        private bool shouldStop = false;

        public TaskList(RestServiceClient restClient)
        {
            _restClient = restClient;
        }

        /// <summary>
        /// Is set to non-null if something goes wrong
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Starts a loop checking the status of active tasks on server
        /// The loop ends when another thread calls Stop()
        /// </summary>
        public void Start( object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Error = null;

            try
            {
                DateTime lastCheckTime = DateTime.UtcNow.AddHours(-1);
                while (!shouldStop)
                {
                    bool hasJobs = false;
                    lock (allTasks)
                    {
                        if (allTasks.Count > 0)
                            hasJobs = true;
                    }


                    // Time to sleep in seconds
                    int sleepTime = 1;

                    if (hasJobs)
                    {
                        Task[] serverTasks = _restClient.ListTasks(lastCheckTime);

                        lock (allTasks)
                        {
                            foreach (Task task in serverTasks)
                            {
                                //Console.WriteLine( String.Format( "{0}: {1}", task.Id, task.Status ) );
                                if (allTasks.ContainsKey(task.Id))
                                {
                                    allTasks[task.Id] = task;
                                }
                            }


                            // There is no need to check for statuses of all the tasks on the server - just check
                            // only those that have changed since last call
                            // This trick should work even if local time is incorrect
                            if (allTasks.Count > 0)
                                lastCheckTime = allTasks.Values.First().StatusChangeTime;
                            foreach (Task task in allTasks.Values)
                            {
                                if (task.StatusChangeTime > lastCheckTime)
                                    lastCheckTime = task.StatusChangeTime;
                            }
                        }

                        // Slightly decrease lastCheckTime to avoid losing tasks
                        lastCheckTime = lastCheckTime.AddSeconds(-2);

                        // Find out when to perform next check
                        // TODO
                        
                    }


                    // Sleep for 1 second
                    System.Threading.Thread.Sleep(sleepTime * 1000);
                }
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        public void Stop()
        {
            shouldStop = true;
        }

        public void AddTask(Task task)
        {
            lock (allTasks)
            {
                allTasks.Add(task.Id, task);
            }
        }

        public void DeleteTask(TaskId taskId)
        {
            lock (allTasks)
            {
                if (allTasks.ContainsKey(taskId))
                    allTasks.Remove(taskId);
            }
        }

        public bool IsTaskFinished(TaskId taskId)
        {
            TaskStatus status = TaskStatus.Unknown;
            lock (allTasks)
            {
                status = allTasks[taskId].Status;
            }

            if (status == TaskStatus.Unknown || Task.IsTaskActive(status))
                return false;
            else
                return true;
        }

        public TaskStatus GetTaskStatus(TaskId taskId)
        {
            lock (allTasks)
            {
                return allTasks[taskId].Status;
            }
        }

        public Task GetTask(TaskId taskId)
        {
            lock (allTasks)
            {
                return allTasks[taskId];
            }
        }
    }
}
