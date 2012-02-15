using System;
using System.Collections.Generic;
using System.Text;
using HatCMS;

namespace HatCms.Admin.BackgroundTasks
{
    public class ReindexAllPagesBackgroundTask : CmsBackgroundTask
    {
        public override CmsBackgroundTaskInfo getBackgroundTaskInfo()
        {
            CmsBackgroundTaskInfo ret = new CmsBackgroundTaskInfo(CmsBackgroundTaskInfo.CmsTaskType.Periodic, CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryDay);
            return ret;
        }

        public override void RunBackgroundTask()
        {
            HatCMS.Controls.SearchResults.ReIndexAllPages();
        }
    }
}
