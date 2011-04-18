using System;
using System.Web;
using System.Web.UI;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal
{
    public class ApplicationUtils
    {
        public static void Application_Error_StandardEmailSender(HttpContext context, string fromEmailAddress, string[] toEmailAddresses, string smtpHostName)
        {
            string emailSubject = context.Request.Url.ToString().Split('/')[2] + " has an ASP.NET error at " + DateTime.Now.ToString("MMM dd yyyy HH:mm");

            System.Net.Mail.MailAddress from = new MailAddress(fromEmailAddress);
            List<System.Net.Mail.MailAddress> to = new List<MailAddress>();
            foreach (string toEmail in toEmailAddresses)
            {
                to.Add(new MailAddress(toEmail));
            } // foreach

            StringBuilder errDisplay = new StringBuilder();
            errDisplay.Append("<p><strong>Aw, Snap!</strong></p>");
            errDisplay.Append("<p>Something <em>definitely</em> went wrong with this webpage. An email has been sent to our staff letting them know that you had this error.</p>");
            errDisplay.Append("<p>To continue, please Reload, go to our <a href=\"" + HttpContext.Current.Request.ApplicationPath + "\">home page</a> or go to another page.</p>");

            Application_Error_StandardEmailSender(context, from, to.ToArray(), emailSubject, smtpHostName, errDisplay.ToString());
        }

        public static void Application_Error_StandardEmailSender(HttpContext context, System.Net.Mail.MailAddress fromEmailAddress, System.Net.Mail.MailAddress[] toEmailAddresses, string emailSubject, string smtpHostName, string htmlToDisplayToUser)
        {
            Exception ex = context.Server.GetLastError();
            if (ex is HttpException && ex.InnerException is System.Web.UI.ViewStateException)
            {
                context.Response.Redirect(context.Request.Url.AbsoluteUri);
                return;
            }
            StringBuilder msgBody = new StringBuilder();

            msgBody.Append("Date: " + DateTime.Now.ToString("MMM dd yyyy HH:mm") + "\n");
            msgBody.Append("URL: " + context.Request.Url + "\n");
            msgBody.Append("Referer: " + context.Request.ServerVariables["HTTP_REFERER"] + "\n");
            msgBody.Append("Requesting IP: " + context.Request.ServerVariables["REMOTE_HOST"] + "\n");
            msgBody.Append("Error Message: " + ex.ToString() + "\n");
            if (context.User.Identity.IsAuthenticated)
                msgBody.Append("User: " + context.User.Identity.Name + "\n");
            else
                msgBody.Append("User is not logged in." + "\n");

            if (context.Request.Form.AllKeys.Length > 0)
            {
                msgBody.Append("Form Values: " + "\n");
                foreach (string s in context.Request.Form.AllKeys)
                {
                    if (s.CompareTo("__VIEWSTATE") != 0)
                        msgBody.Append(s + ":" + context.Request.Form[s] + "\n");
                }
            }
            if (context.Handler is System.Web.SessionState.IRequiresSessionState || context.Handler is System.Web.SessionState.IReadOnlySessionState)
            {
                msgBody.Append("Session Values: " + "\n");

                foreach (string s in context.Session.Keys)
                    msgBody.Append(s + ":" + context.Session[s] + "\n");
            }
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage();
            email.IsBodyHtml = false;
            email.From = fromEmailAddress;
            foreach (System.Net.Mail.MailAddress to in toEmailAddresses)
            {
                email.To.Add(to);
            }
            email.Subject = emailSubject;
            email.Body = msgBody.ToString();
            try
            {
                System.Net.Mail.SmtpClient emailProvider = new System.Net.Mail.SmtpClient();
                emailProvider.Host = smtpHostName;
                emailProvider.Send(email);
            }
            catch (System.Net.Mail.SmtpException ex1)
            { }
            finally
            {
                context.Response.Write(htmlToDisplayToUser);
                context.Response.End();

            }
        } // if
    }
}

