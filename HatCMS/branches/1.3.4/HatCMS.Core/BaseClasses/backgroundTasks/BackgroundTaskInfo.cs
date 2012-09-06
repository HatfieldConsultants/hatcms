using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    public class CmsBackgroundTaskInfo
    {

        public enum CmsTaskType { Periodic, RunOnlyOnApplicationStart, RunOnlyOnApplicationEnd };
        /// <summary>
        /// note: period should be in hourly chunks.
        /// </summary>
        public enum CmsPeriodicTaskPeriod { _NotPeriodic = 0, EveryHour = 1, EveryDay = 24, EveryTwoDays = 48, EveryWeek = 168 }


        public CmsTaskType TaskType;
        public CmsPeriodicTaskPeriod PeriodicTaskPeriod;


        public CmsBackgroundTaskInfo(CmsTaskType backgroundTaskType, CmsPeriodicTaskPeriod taskPeriod)
        {
            TaskType = backgroundTaskType;
            PeriodicTaskPeriod = taskPeriod;
        } // constructor
    }
}
