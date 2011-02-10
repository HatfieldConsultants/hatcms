using System;
using System.Net;
using System.Net.Mail;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    #region Data Holding Classes
    public class UserFeedbackFormInfo
    {
        public string EmailAddressesToNotify = "";
        public string ThankyouMessage = "Thank you for sending us your comments";
        public int FormFieldDisplayWidth = 50;
        public string TextAreaQuestion = "Comments";
    }

    public class UserFeedbackSubmittedData
    {
        public DateTime dateTimeSubmitted; 
        public string Name = "";
        public string EmailAddress = "";
        public string Location = "";
        public string TextAreaQuestion = "";
        public string TextAreaValue = "";
        public string ReferringUrl = "";        
    }
    #endregion

    public class UserFeedback : BaseCmsPlaceholder
    {
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency("userfeedbackform", new string[] {
                "pageid", "identifier", "LangCode", "EmailAddressesToNotify", "ThankyouMessage", "FormFieldDisplayWidth", "TextAreaQuestion"
                }));
            ret.Add(new CmsDatabaseTableDependency("userfeedbacksubmitteddata", new string[] {
                "dateTimeSubmitted", "Name", "EmailAddress", "Location", "TextAreaQuestion", "TextAreaValue", "ReferringUrl"
                }));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("UserFeedback.ValuesPreloadedText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.CompleteAllText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.NameText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.EmailText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.LocationText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.SubmitButtonText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.ErrorEnterNameText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.ErrorEnterEmailText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.ErrorEnterValidEmailText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.ErrorEnterTextAreaQuestionText"));
            ret.Add(new CmsConfigItemDependency("UserFeedback.ErrorSavingText"));

            ret.Add(new CmsConfigItemDependency("smtpServer"));
            return ret.ToArray();
        }

#region Multi-lang get method

        protected string getValuesPreloadedText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.ValuesPreloadedText", "Note: some form values have been pre-loaded from a previous submission.", lang);
        }

        protected string getCompleteAllText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.CompleteAllText", "Please complete all portions of this form", lang);
        }

        protected string getNameText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.NameText", "Name", lang);
        }

        protected string getEmailText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.EmailText", "E-mail address", lang);
        }

        protected string getLocationText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.LocationText", "Location (city/province)", lang);
        }

        protected string getSubmitButtonText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.SubmitButtonText", "Submit feedback", lang);
        }

        protected string getErrorEnterNameText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.ErrorEnterNameText", "Error: please enter your name.", lang);
        }

        protected string getErrorEnterEmailText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.ErrorEnterEmailText", "Error: please enter your E-mail address.", lang);
        }

        protected string getErrorEnterValidEmailText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.ErrorEnterValidEmailText", "Error: please enter a valid E-mail address.", lang);
        }

        protected string getErrorEnterTextAreaQuestionText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.ErrorEnterTextAreaQuestionText", "Error: please enter ", lang);
        }

        protected string getErrorSavingText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("UserFeedback.ErrorSavingText", "Error: your submission could not be saved. Please inform the webmaster using an alternative method.", lang);
        }

#endregion

        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }               

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {

            UserFeedbackDb db = new UserFeedbackDb();
            UserFeedbackFormInfo formInfo = db.getUserFeedbackFormInfo(page, identifier, langToRenderFor, true);
            string ControlId = "UserFeedbackInfo_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode + "_";

            string action = PageUtils.getFromForm(ControlId + "Action", "");
            if (action.Trim().ToLower() == "update")
            {
                formInfo.EmailAddressesToNotify = PageUtils.getFromForm(ControlId + "EmailAddressesToNotify", formInfo.EmailAddressesToNotify);                
                formInfo.ThankyouMessage = PageUtils.getFromForm(ControlId + "ThankyouMessage", formInfo.ThankyouMessage);
                formInfo.TextAreaQuestion = PageUtils.getFromForm(ControlId + "TextAreaQuestion", formInfo.TextAreaQuestion);
                db.saveUpdatedUserFeedbackFormInfo(page, identifier, langToRenderFor, formInfo);
            }
            
            StringBuilder html = new StringBuilder();
            html.Append("<table>");
            html.Append("<tr>");
            html.Append("<td valign=\"top\">");
            html.Append("Email addresses to notify of form submissions: ");
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            html.Append(PageUtils.getInputTextHtml(ControlId + "EmailAddressesToNotify", ControlId + "EmailAddressesToNotify", formInfo.EmailAddressesToNotify, 50, 255));
            html.Append("<br>");
            html.Append("<font size=\"1\">(seperate addresses with semi-colons (;)</font>");
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td valign=\"top\">");
            html.Append("Text area question text: ");
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            html.Append(PageUtils.getInputTextHtml(ControlId + "TextAreaQuestion", ControlId + "TextAreaQuestion", formInfo.TextAreaQuestion, 50, 255));            
            html.Append("</td>");
            html.Append("</tr>");
            
            html.Append("<tr>");
            html.Append("<td valign=\"top\">HTML message to display to user upon successful submission:");
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            html.Append(PageUtils.getTextAreaHtml(ControlId + "ThankyouMessage", ControlId + "ThankyouMessage", formInfo.ThankyouMessage, 30, 5));
            html.Append("</td>");
            html.Append("</tr>");
            html.Append("</table>");

            html.Append(PageUtils.getHiddenInputHtml(ControlId + "Action", "update"));
            

            writer.Write(html.ToString());
        } // RenderEdit

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            UserFeedbackDb db = new UserFeedbackDb();
            UserFeedbackFormInfo formInfo = db.getUserFeedbackFormInfo(page, identifier, langToRenderFor, true);
            string ControlId = "UserFeedbackInfo_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode + "_";

            string _errorMessage = "";
            string action = PageUtils.getFromForm(ControlId + "Action", "");
            UserFeedbackSubmittedData submittedData = new UserFeedbackSubmittedData();
            bool formValuesLoadedFromSession = false;
            if (action.Trim().ToLower() == "send")
            {
                submittedData.Name = (PageUtils.getFromForm(ControlId + "Name", ""));
                submittedData.Name = submittedData.Name.Trim();
                submittedData.EmailAddress = PageUtils.getFromForm(ControlId + "Email", "");
                submittedData.EmailAddress = submittedData.EmailAddress.Trim();
                submittedData.Location = PageUtils.getFromForm(ControlId + "Location", "");
                submittedData.Location = submittedData.Location.Trim();
                submittedData.TextAreaValue = PageUtils.getFromForm(ControlId + "Comments", "");
                submittedData.TextAreaValue = submittedData.TextAreaValue.Trim();
                submittedData.ReferringUrl = PageUtils.getFromForm(ControlId + "Referer", "");

                // -- validate user submitted values
                if (submittedData.Name == "")
                    _errorMessage = getErrorEnterNameText(langToRenderFor);
                else if (submittedData.EmailAddress == "")
                    _errorMessage = getErrorEnterEmailText(langToRenderFor);
                else if (!PageUtils.isValidEmailAddress(submittedData.EmailAddress))
                    _errorMessage = getErrorEnterValidEmailText(langToRenderFor);
                else if (submittedData.TextAreaValue == "")
                    _errorMessage = getErrorEnterTextAreaQuestionText(langToRenderFor) + formInfo.TextAreaQuestion;
                else
                {
                    // -- save the submitted value                    
                    submittedData.dateTimeSubmitted = DateTime.Now;
                    submittedData.TextAreaQuestion = formInfo.TextAreaQuestion;

                    if (db.saveUserFeedbackSubmittedData(submittedData))
                    {
                        // -- success
                        //    save submitted values to the current session
                        System.Web.SessionState.HttpSessionState session = System.Web.HttpContext.Current.Session;
                        session[ControlId + "Name"] = submittedData.Name;
                        session[ControlId + "Email"] = submittedData.EmailAddress;
                        session[ControlId + "Location"] = submittedData.Location;
                        //    send notification email message
                        sendAdministratorNotification(formInfo, submittedData);
                        //   output the Thankyou message
                        writer.Write("<p><strong>" + formInfo.ThankyouMessage + "</strong></p>");
                        return;
                    }
                    else
                    {
                        _errorMessage = getErrorSavingText(langToRenderFor);
                    }
                }
            } // if save posted values
            else
            {
                // -- get previously submitted values from the current session
                System.Web.SessionState.HttpSessionState session = System.Web.HttpContext.Current.Session;
                if (session[ControlId + "Name"] != null)
                {
                    submittedData.Name = session[ControlId + "Name"].ToString();
                    formValuesLoadedFromSession = true;
                }
                if (session[ControlId + "Email"] != null)
                {
                    submittedData.EmailAddress = session[ControlId + "Email"].ToString();
                    formValuesLoadedFromSession = true;
                }
                if (session[ControlId + "Location"] != null)
                {
                    submittedData.Location = session[ControlId + "Location"].ToString();
                    formValuesLoadedFromSession = true;
                }
            }


            StringBuilder html = new StringBuilder();

            if (_errorMessage != "")
            {
                html.Append("<p class=\"FormErrorMessage\">" + _errorMessage + "</p>");
            }

            string formId = "UserFeedback";
            html.Append(page.getFormStartHtml(formId));

            html.Append("<em>" + getCompleteAllText(langToRenderFor) + "</em> ");
            if (formValuesLoadedFromSession)
                html.Append(getValuesPreloadedText(langToRenderFor));

            html.Append("<table>");
            html.Append("<tr>");
            html.Append("<td valign=\"top\">" + getNameText(langToRenderFor) + ":");
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            html.Append(PageUtils.getInputTextHtml(ControlId + "Name", ControlId + "Name", submittedData.Name, formInfo.FormFieldDisplayWidth, 255));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td valign=\"top\">" + getEmailText(langToRenderFor) + ":");
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            html.Append(PageUtils.getInputTextHtml(ControlId + "Email", ControlId + "Email", submittedData.EmailAddress, formInfo.FormFieldDisplayWidth, 255));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td valign=\"top\">" + getLocationText(langToRenderFor) + ":");
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            html.Append(PageUtils.getInputTextHtml(ControlId + "Location", ControlId + "Location", submittedData.Location, formInfo.FormFieldDisplayWidth, 255));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td valign=\"top\">");
            html.Append(formInfo.TextAreaQuestion);
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            html.Append(PageUtils.getTextAreaHtml(ControlId + "Comments", ControlId + "Comments", "", formInfo.FormFieldDisplayWidth, 7));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("</table>");
                        
            html.Append(PageUtils.getHiddenInputHtml(ControlId + "Action", "send"));
            html.Append(PageUtils.getHiddenInputHtml(ControlId + "Referer", PageUtils.getFromForm("r","(unknown)")));

            html.Append("<input type=\"submit\" value=\"" + getSubmitButtonText(langToRenderFor) + "\">");
            html.Append(page.getFormCloseHtml(formId));

            writer.Write(html.ToString());

        } // RenderView


        private bool sendAdministratorNotification(UserFeedbackFormInfo formInfo, UserFeedbackSubmittedData submittedData)
        {
            if (formInfo.EmailAddressesToNotify.Trim() == "")
                return true;

            string smtpServer = CmsConfig.getConfigValue("smtpServer", "smtp.hatfieldgroup.com");

            string toAddress = formInfo.EmailAddressesToNotify;
            
            string[] toArray = toAddress.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

            string fromAddress = submittedData.EmailAddress;            

            string subject = CmsContext.currentPage.Title;

            string body = "The following feedback was sent from the "+System.Web.HttpContext.Current.Request.Url.Host+" website" + Environment.NewLine + Environment.NewLine;
            body += "Date: " + DateTime.Now.ToString("MMM dd yyyy hh:mm tt") + Environment.NewLine;
            body += "From: " + submittedData.Name+" ("+submittedData.EmailAddress +")"+ Environment.NewLine;
            body += "Location: " + submittedData.Location + Environment.NewLine;
            body += "Referencing Location: " + submittedData.ReferringUrl + Environment.NewLine + Environment.NewLine;
            body += submittedData.TextAreaQuestion+":"+ Environment.NewLine+ Environment.NewLine;
            body += submittedData.TextAreaValue;
                                      
            try
            {
                // SmtpMail.SmtpServer = smtpServer;

                Hatfield.Web.Portal.Net.SmtpDirect.SmtpServerHostName = smtpServer;
                
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(fromAddress);

                foreach (string to in toArray)
                {
                    msg.To.Add(to);
                }

                msg.IsBodyHtml = false;

                msg.Subject = subject;
                msg.Body = body;
                bool b = Hatfield.Web.Portal.Net.SmtpDirect.Send(msg);
                if (!b)
                {                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            
            return false;
            
        }


    } // GoogleMap
}
