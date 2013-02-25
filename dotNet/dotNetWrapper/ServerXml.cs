using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Abbyy.CloudOcrSdk
{
    class ServerXml
    {
        public static TaskId GetTaskId(XDocument xml)
        {
            string id = xml.Root.Element("task").Attribute("id").Value;
            return new TaskId(id);
        }

        public static Task GetTaskStatus(XDocument xml)
        {
            return getTaskInfo( xml.Root.Element("task") );
        }

        private static TaskStatus statusFromString(string status)
        {
            switch (status.ToLower())
            {
                case "submitted":
                    return TaskStatus.Submitted;
                case "queued":
                    return TaskStatus.Queued;
                case "inprogress":
                    return TaskStatus.InProgress;
                case "completed":
                    return TaskStatus.Completed;
                case "processingfailed":
                    return TaskStatus.ProcessingFailed;
                case "deleted":
                    return TaskStatus.Deleted;
                case "notenoughcredits":
                    return TaskStatus.NotEnoughCredits;
                default:
                    return TaskStatus.Unknown;
            }
        }

        public static Task[] GetAllTasks(XDocument xml)
        {
            List<Task> result = new List<Task>();
            XElement xResponse = xml.Root;
            foreach (XElement xTask in xResponse.Elements("task"))
            {
                Task task = getTaskInfo(xTask);
                result.Add(task);
            }

            return result.ToArray();
        }


        /// <summary>
        /// Get task data from xml node "task"
        /// </summary>
        private static Task getTaskInfo(XElement xTask)
        {
            TaskId id = new TaskId(xTask.Attribute("id").Value);
            TaskStatus status = statusFromString(xTask.Attribute("status").Value);

            Task task = new Task();
            task.Id = id;
            task.Status = status;

            XAttribute xRegistrationTime = xTask.Attribute("registrationTime");
            if (xRegistrationTime != null)
            {
                DateTime time;
                if (DateTime.TryParse(xRegistrationTime.Value, out time))
                    task.RegistrationTime = time;
            }

            XAttribute xStatusChangeTime = xTask.Attribute("statusChangeTime");
            if (xStatusChangeTime != null)
            {
                DateTime time;
                if (DateTime.TryParse(xStatusChangeTime.Value, out time))
                    task.StatusChangeTime = time;
            }

            XAttribute xPagesCount = xTask.Attribute("filesCount");
            if (xPagesCount != null)
            {
                int pagesCount;
                if (Int32.TryParse(xPagesCount.Value, out pagesCount))
                    task.PagesCount = pagesCount;
            }

            XAttribute xCredits = xTask.Attribute("credits");
            if (xCredits != null)
            {
                int credits;
                if( Int32.TryParse( xCredits.Value, out credits ))
                    task.Credits = credits;
            }

            XAttribute xDescription = xTask.Attribute("description");
            if (xDescription != null)
                task.Description = xDescription.Value;

            XAttribute xResultUrl = xTask.Attribute("resultUrl");
            if (xResultUrl != null)
            {
                task.DownloadUrls = new List<string>{xResultUrl.Value};
                for (int i = 2; i < 10; i++)
                {
                    XAttribute xResultUrlI = xTask.Attribute("resultUrl" + i);
                    if (xResultUrlI != null)
                    {
                        task.DownloadUrls.Add(xResultUrlI.Value);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            XAttribute xError = xTask.Attribute("error");
            if (xError != null)
            {
                task.Error = xError.Value;
            }

            return task;
        }
    }
}
