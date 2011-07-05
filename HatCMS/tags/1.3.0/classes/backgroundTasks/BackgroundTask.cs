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
    /// <summary>
    /// all background tasks must inherit from this CmsBackgroundTask type.
    /// </summary>
    public abstract class CmsBackgroundTask
    {
        public abstract CmsBackgroundTaskInfo getBackgroundTaskInfo();
        public abstract void RunBackgroundTask();
    }
}
