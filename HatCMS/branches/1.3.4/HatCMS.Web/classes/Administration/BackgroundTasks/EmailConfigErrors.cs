using System;
using System.Collections.Generic;
using System.Text;
using HatCMS;
using System.Text;
using HatCMS.setup;
using System.Net.Mail;

namespace HatCms.Admin.BackgroundTasks
{
    public class EmailConfigErrors: CmsBackgroundTask
    {
        public override CmsBackgroundTaskInfo getBackgroundTaskInfo()
        {
            return new CmsBackgroundTaskInfo(CmsBackgroundTaskInfo.CmsTaskType.Periodic, CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryWeek);
        }

        public override void RunBackgroundTask()
        {
            string techEmail = CmsConfig.getConfigValue("TechnicalAdministratorEmail", "");
            string smtpServer = CmsConfig.getConfigValue("smtpServer", "");
            if (techEmail.IndexOf("@") < 1 || smtpServer.Trim() == "")
            {
                return; // don't run anything if there's no email address or smtp server defined.
            }

            CmsDependencyMessage[] msgs = setupPage.VerifyConfig();
            CmsDependencyMessage[] errorMessages = CmsDependencyMessage.GetAllMessagesByLevel(CmsDependencyMessage.MessageLevel.Error, msgs);
            if (errorMessages.Length > 0) // only email errors
            {
                StringBuilder msgBody = new StringBuilder();
                string subject = "HatCMS site errors";
                msgBody.Append("<div style=\"color: red;\">");
                string siteName = CmsConfig.getConfigValue("SiteName", "");
                if (siteName != "")
                {
                    msgBody.Append("The following errors were found in the " + siteName + ":");
                    subject = siteName+" Errors";
                }
                else
                {
                    msgBody.Append("The following errors were found in your configuration:");
                }
                
                msgBody.Append("</div>");
                msgBody.Append("<ul>");

                foreach (CmsDependencyMessage m in errorMessages)
                {
                    msgBody.Append("<li>" + m.Message + "</li>");
                }
                msgBody.Append("</ul>");

                MailMessage msg = new MailMessage(techEmail, techEmail, subject, msgBody.ToString());
                msg.IsBodyHtml = true;

                SmtpClient smtpclient = new SmtpClient(smtpServer);
                smtpclient.Send(msg);
            }
        }
    }
}
