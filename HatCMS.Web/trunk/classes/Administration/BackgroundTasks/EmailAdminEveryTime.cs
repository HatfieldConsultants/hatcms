using System;
using System.Collections.Generic;
using System.Text;
using HatCMS;
using System.Text;
using HatCMS.setup;
using System.Net.Mail;
using HatCMS;

namespace HatCms.Admin.BackgroundTasks
{
    public class EmailAdminEveryHour : CmsBackgroundTask
    {
        public override CmsBackgroundTaskInfo getBackgroundTaskInfo()
        {
            return new CmsBackgroundTaskInfo(CmsBackgroundTaskInfo.CmsTaskType.Periodic, CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryHour);
        }

        public override void RunBackgroundTask()
        {
            string techEmail = CmsConfig.getConfigValue("TechnicalAdministratorEmail", "");
            string smtpServer = CmsConfig.getConfigValue("smtpServer", "");
            if (techEmail.IndexOf("@") < 1 || smtpServer.Trim() == "")
            {
                return; // don't run anything if there's no email address or smtp server defined.
            }

            string configSiteName = CmsConfig.getConfigValue("SiteName", "");
            if (configSiteName != "")
                configSiteName = " [" + configSiteName + "] ";
            string msgBody = "Hourly email from " + System.Web.Hosting.HostingEnvironment.SiteName + configSiteName +":  " + DateTime.Now.ToString("MMM d yyyy HH:mm:ss");

            MailMessage msg = new MailMessage(techEmail, techEmail, msgBody, msgBody);
            msg.IsBodyHtml = true;

            SmtpClient smtpclient = new SmtpClient(smtpServer);
            smtpclient.Send(msg);

        }
    }
}
