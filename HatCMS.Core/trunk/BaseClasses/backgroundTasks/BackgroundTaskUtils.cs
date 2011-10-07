using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS
{
    public class CmsBackgroundTaskUtils
    {

        private static CmsBackgroundTask[] getAllBackgroundTasks()
        {
            // -- note: do not cache this function.
            List<CmsBackgroundTask> ret = new List<CmsBackgroundTask>();
            Type[] backgroundTaskTypes = AssemblyHelpers.LoadAllAssembliesAndGetAllSubclassesOf(typeof(CmsBackgroundTask));
            foreach (Type taskType in backgroundTaskTypes)
            {
                try
                {
                    CmsBackgroundTask taskObj = (CmsBackgroundTask)taskType.Assembly.CreateInstance(taskType.FullName);
                    ret.Add(taskObj);
                }
                catch
                {
                    Console.Write("Error: could not load module info " + taskType.FullName);
                }
            } // foreach

            CmsBackgroundTask[] arr = ret.ToArray();
            return arr;

        }


        private static void RunAllBackgroundTasksOfType(CmsBackgroundTaskInfo.CmsTaskType typeOfTasksToRun)
        {
            CmsBackgroundTask[] allTasks = getAllBackgroundTasks();
            foreach (CmsBackgroundTask task in allTasks)
            {
                CmsBackgroundTaskInfo taskInfo = task.getBackgroundTaskInfo();
                if (taskInfo != null && taskInfo.TaskType == typeOfTasksToRun)
                {
                    task.RunBackgroundTask();
                }
            } // foreach
        }

        public static void RunAllApplicationStartBackgroundTasks()
        {
            RunAllBackgroundTasksOfType(CmsBackgroundTaskInfo.CmsTaskType.RunOnlyOnApplicationStart);
        }


        public static void RunAllApplicationEndBackgroundTasks()
        {
            RunAllBackgroundTasksOfType(CmsBackgroundTaskInfo.CmsTaskType.RunOnlyOnApplicationEnd);
        }

        private static string getLastStartPersistentVariableName(CmsBackgroundTask task)
        {
            return  "LastPeriodicTaskStartTime_" + task.GetType().FullName;
        }

        /// <summary>
        /// returns DateTime.MinValue if the task has never been run, or if the last run time could not be determined.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private static DateTime GetLastPeriodicTaskStartTime(CmsBackgroundTask task)
        {
            string persistentVariableName = getLastStartPersistentVariableName(task);
            CmsPersistentVariable v = CmsPersistentVariable.Fetch(persistentVariableName);
            if (v.Name == persistentVariableName)
            {
                return (DateTime)v.PersistedValue;
            }
            return DateTime.MinValue;

        }

        private static void SaveLastPeriodicTaskStartTime(CmsBackgroundTask task, DateTime startTime)
        {
            string persistentVariableName = getLastStartPersistentVariableName(task);
            CmsPersistentVariable v = CmsPersistentVariable.Fetch(persistentVariableName);
            v.Name = persistentVariableName;
            v.PersistedValue = startTime;

            v.SaveToDatabase();
        }

        /// <summary>        
        /// </summary>
        /// <param name="taskinfo"></param>
        /// <param name="lastRunTime"></param>
        /// <returns></returns>
        private static DateTime GetNextRunTime(CmsBackgroundTaskInfo taskinfo, DateTime lastRunTime)
        {
            if (lastRunTime == DateTime.MinValue)
                return DateTime.MinValue;

            switch (taskinfo.PeriodicTaskPeriod)
            {
                case CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryDay:
                    return lastRunTime.AddDays(1);
                    break;
                case CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryHour:
                    return lastRunTime.AddHours(1);
                    break;
                case CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryTwoDays:
                    return lastRunTime.AddDays(2);
                    break;
                case CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryWeek:
                    return lastRunTime.AddDays(7);
                    break;
                case CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod._NotPeriodic:
                    return DateTime.MaxValue;
                    break;
                default:
                    throw new ArgumentException("Invalid CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod specified");
                    break;
            }
        }

        public static void RunAllApplicablePeriodicTasks()
        {
            CmsBackgroundTask[] allTasks = getAllBackgroundTasks();
            foreach (CmsBackgroundTask task in allTasks)
            {
                CmsBackgroundTaskInfo taskInfo = task.getBackgroundTaskInfo();
                if (taskInfo != null && taskInfo.TaskType == CmsBackgroundTaskInfo.CmsTaskType.Periodic)
                {
                    DateTime lastRunTime = GetLastPeriodicTaskStartTime(task);
                    DateTime nextRunTime = GetNextRunTime(taskInfo, lastRunTime);
                    DateTime now = DateTime.Now;
                    if (nextRunTime <= now)
                    {
                        task.RunBackgroundTask();
                        SaveLastPeriodicTaskStartTime(task, now);
                    }
                }
            } // foreach

        }


    }
}
