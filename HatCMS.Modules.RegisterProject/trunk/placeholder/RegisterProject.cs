using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;
using System.Text;
using System.Collections.Generic;
using Hatfield.Web.Portal;
using HatCMS;

namespace HatCMS.Placeholders.RegisterProject
{
    public class RegisterProject : BaseCmsPlaceholder
    {
        protected string EOL = Environment.NewLine;

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency(@"
                    CREATE TABLE `registerproject` (
                      `ProjectId` int(11) NOT NULL AUTO_INCREMENT,
                      `Name` varchar(255) NOT NULL,
                      `Location` varchar(255) NOT NULL,
                      `Description` text NOT NULL,
                      `ContactPerson` varchar(255) NOT NULL,
                      `Email` varchar(255) NOT NULL,
                      `Telephone` varchar(30) NOT NULL,
                      `Cellphone` varchar(30) NOT NULL,
                      `Website` varchar(255) NOT NULL,
                      `FundingSource` varchar(255) NOT NULL,
                      `CreatedDateTime` datetime DEFAULT NULL,
                      `ClientIP` varchar(255) NOT NULL,
                      PRIMARY KEY (`ProjectId`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("RegisterProject.NameText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.LocationText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.DescriptionText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.ContactPersonText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.EmailText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.TelephoneText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.CellphoneText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.WebsiteText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.FundingSourceText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.MandatoryText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.SaveOkText"));
            ret.Add(new CmsConfigItemDependency("RegisterProject.SaveErrorText"));
/*
    <!-- Register Project -->
    <add key="RegisterProject.NameText" value="Project name|Nome do projeto" />
    <add key="RegisterProject.LocationText" value="Project location|Local do Projeto" />
    <add key="RegisterProject.DescriptionText" value="Project description|Descrição do projeto" />
    <add key="RegisterProject.ContactPersonText" value="Contact person|Pessoa de contato" />
    <add key="RegisterProject.EmailText" value="Email|E-mail" />
    <add key="RegisterProject.TelephoneText" value="Telephone (Area code - Phone number - Extension)|Telefone (O código de área - número - Extensão)" />
    <add key="RegisterProject.CellphoneText" value="Cell phone (Area code - Phone number)|Telefone celular (O código de área - número)" />
    <add key="RegisterProject.WebsiteText" value="Website URL|URL do site" />
    <add key="RegisterProject.FundingSourceText" value="Funding source|fonte de financiamento" />
    <add key="RegisterProject.SubmitButtonText" value="Register your project|Cadastre seu projeto" />
    <add key="RegisterProject.MandatoryText" value="Mandatory input field|Obrigatório campo de entrada" />
    <add key="RegisterProject.SaveOkText" value="Your project is registered.  Thank you.|Seu projeto é registrado. Obrigado." />
    <add key="RegisterProject.SaveErrorText" value="Server error, registration failed.|Erro de servidor, registro falhou." />
 */ 

            ret.Add(CmsFileDependency.UnderAppPath("js/_system/RegisterProject/RegisterProject.js", CmsDependency.ExistsMode.MustNotExist)); // now embedded.

            return ret.ToArray();
        }

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;
        }

        protected void addHeaderEntry(CmsPage page)
        {
            
            page.HeadSection.AddCSSStyleStatements(".registerProjectform { margin-top: 1em; }");
            page.HeadSection.AddCSSStyleStatements(".registerProjectform > label { display: block; }");
            page.HeadSection.AddCSSStyleStatements(".registerProjectform > input, .registerProjectform > textarea { margin-bottom: 1em; }");
            page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/jquery/jquery-1.4.1.min.js");
            page.HeadSection.AddEmbeddedJavascriptFile(JavascriptGroup.ControlOrPlaceholder, typeof(RegisterProject).Assembly, "RegisterProject.js");
        }

#region Multi-lang get method

        protected string getText(CmsLanguage lang, string configKey, string dftValue)
        {
            string[] msgArray = CmsConfig.getConfigValue(configKey, dftValue).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            CmsLanguage[] langArray = CmsConfig.Languages;
            int x = CmsLanguage.IndexOf(lang.shortCode, langArray);

            if (msgArray.Length < langArray.Length || x < 0)
                throw new Exception("Missing entry for " + configKey + "!");

            return msgArray[x];
        }

        protected string getNameText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.NameText", "Name");
        }

        protected string getLocationText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.LocationText", "Location");
        }

        protected string getDescriptionText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.DescriptionText", "Description");
        }

        protected string getFundingSourceText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.FundingSourceText", "Funding source");
        }

        protected string getContactPersonText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.ContactPersonText", "Contact person");
        }

        protected string getEmailText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.EmailText", "Email");
        }

        protected string getTelephoneText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.TelephoneText", "Telephone (Area code - Phone number - Extension)");
        }

        protected string getCellphoneText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.CellphoneText", "Cell phone (Area code - Phone number)");
        }

        protected string getWebsiteText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.WebsiteText", "Website URL");
        }

        protected string getSubmitButtonText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.SubmitButtonText", "Register your project");
        }

        protected string getRequiredText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.MandatoryText", "Mandatory input field");
        }
        
        protected string getSaveOkText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.SaveOkText", "Your project is registered.  Thank you.");
        }

        protected string getSaveErrorText(CmsLanguage lang)
        {
            return getText(lang, "RegisterProject.SaveErrorText", "Server error, registration failed.");
        }
#endregion

        protected string getPhoneNumFromHtmlForm(string controlId, string prefix) {
            string area = PageUtils.getFromForm(controlId + prefix + "Area", "");
            string num = PageUtils.getFromForm(controlId + prefix + "Num", "");
            string ext = PageUtils.getFromForm(controlId + prefix + "Ext", "");

            StringBuilder sb = new StringBuilder();
            if (area != "")
            {
                sb.Append("(");
                sb.Append(area);
                sb.Append(") ");
            }
                
            if (num != "")
                sb.Append(num);

            if (ext != "")
            {
                sb.Append(" ext ");
                sb.Append(ext);
            }
            return sb.ToString();
        }

        protected string handleFormSubmit(CmsLanguage lang, string controlId)
        {
            if (PageUtils.getFromForm("registerProject_FormAction", "") != "add")
                return "";

            RegisterProjectDb.RegisterProjectData entity = new RegisterProjectDb.RegisterProjectData();
            entity.Name = PageUtils.getFromForm(controlId + "_Name", "");
            entity.Location = PageUtils.getFromForm(controlId + "_Location", "");
            entity.Description = PageUtils.getFromForm(controlId + "_Description", "");
            entity.ContactPerson = PageUtils.getFromForm(controlId + "_ContactPerson", "");
            entity.Email = PageUtils.getFromForm(controlId + "_Email", "");
            entity.Telephone = getPhoneNumFromHtmlForm(controlId, "_Telephone");
            entity.Cellphone = getPhoneNumFromHtmlForm(controlId, "_Cellphone");
            entity.Website = PageUtils.getFromForm(controlId + "_Website", "");
            entity.FundingSource = PageUtils.getFromForm(controlId + "_FundingSource", "");
            entity.CreatedDateTime = DateTime.Now;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
                entity.ClientIp = HttpContext.Current.Request.UserHostAddress;
            else
                entity.ClientIp = "";

            if (new RegisterProjectDb().insertData(entity) == false)
                return "<p style=\"font-weight: bold; color: red;\">" + getSaveErrorText(lang) + "</p>";
            else
                return "<p style=\"font-weight: bold; color: green;\">" + getSaveOkText(lang) + "</p>";
        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            addHeaderEntry(page);

            JavascriptEvent[] dummy = new JavascriptEvent[] { };
            JavascriptEvent[] jsEventArray = new JavascriptEvent[] { new JavascriptEvent(JavascriptEventName.onkeypress, "return numbersOnly(event);")};
            string controlId = "registerProject_" + page.ID + "_" + identifier + "_" + langToRenderFor.shortCode;
            string msg = handleFormSubmit(langToRenderFor, controlId);
            if (msg != "") {
                writer.Write(msg);
                return;
            }

            StringBuilder sb = new StringBuilder(msg + EOL);
            sb.Append("<form class=\"registerProjectform\" method=\"get\">" + EOL);

//            string rpLangSubmitted = "registerProject_Lang";
//            sb.Append(PageUtils.getHiddenInputHtml(rpLangSubmitted, rpLangSubmitted, "") + EOL);

            string rpFormAction = "registerProject_FormAction";
            sb.Append(PageUtils.getHiddenInputHtml(rpFormAction, rpFormAction, "add") + EOL);

            string rpName = controlId + "_Name";
            sb.Append("<label>" + EOL);
            sb.Append(getNameText(langToRenderFor) + " *" + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpName, rpName, "", 40, 60, "mandatoryField", dummy) + EOL);

            string rpLocation = controlId + "_Location";
            sb.Append("<label>" + EOL);
            sb.Append(getLocationText(langToRenderFor) + " *" + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpLocation, rpLocation, "", 40, 60, "mandatoryField", dummy) + EOL);

            string rpDescription = controlId + "_Description";
            sb.Append("<label>" + EOL);
            sb.Append(getDescriptionText(langToRenderFor) + " *" + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getTextAreaHtml(rpDescription, rpDescription, "", 60, 6, "mandatoryField") + EOL);

            string rpContactPerson = controlId + "_ContactPerson";
            sb.Append("<label>" + EOL);
            sb.Append(getContactPersonText(langToRenderFor) + " *" + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpContactPerson, rpContactPerson, "", 40, 60, "mandatoryField", dummy) + EOL);

            string rpEmail = controlId + "_Email";
            sb.Append("<label>" + EOL);
            sb.Append(getEmailText(langToRenderFor) + " *" + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpEmail, rpEmail, "", 40, 60, "mandatoryField", dummy) + EOL);

            string rpTelephone1 = controlId + "_TelephoneArea";
            string rpTelephone2 = controlId + "_TelephoneNum";
            string rpTelephone3 = controlId + "_TelephoneExt";
            sb.Append("<label>" + EOL);
            sb.Append(getTelephoneText(langToRenderFor) + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpTelephone1, rpTelephone1, "", 4, 5, "", jsEventArray) + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpTelephone2, rpTelephone2, "", 15, 15, "", jsEventArray) + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpTelephone3, rpTelephone3, "", 4, 5, "", jsEventArray) + EOL);

            string rpCellphone1 = controlId + "_CellphoneArea";
            string rpCellphone2 = controlId + "_cellphoneNum";
            sb.Append("<label>" + EOL);
            sb.Append(getCellphoneText(langToRenderFor) + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpCellphone1, rpCellphone1, "", 4, 5, "", jsEventArray) + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpCellphone2, rpCellphone2, "", 15, 15, "", jsEventArray) + EOL);

            string rpWebsite = controlId + "_Website";
            sb.Append("<label>" + EOL);
            sb.Append(getWebsiteText(langToRenderFor) + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpWebsite, rpWebsite, "", 40, 60) + EOL);

            string rpFundingSource = controlId + "_FundingSource";
            sb.Append("<label>" + EOL);
            sb.Append(getFundingSourceText(langToRenderFor) + EOL);
            sb.Append("</label>" + EOL);
            sb.Append(PageUtils.getInputTextHtml(rpFundingSource, rpFundingSource, "", 40, 60) + EOL);

            sb.Append("<p>" + EOL);
//            sb.Append("<input onclick=\"document.getElementById('" + rpLangSubmitted + "').value='" + langToRenderFor.shortCode + "';\" value=\"" + getSubmitButtonText(langToRenderFor) + "\" type=\"submit\" />" + EOL);
            sb.Append("<input value=\"" + getSubmitButtonText(langToRenderFor) + "\" type=\"submit\" />" + EOL);
            sb.Append("</p>" + EOL);

            sb.Append("<p>* " + getRequiredText(langToRenderFor) + "</p>" + EOL);

            sb.Append("</form>" + EOL);

            writer.Write(sb.ToString());
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string controlId = "registerProject_" + page.ID + "_" + identifier + "_" + langToRenderFor.shortCode;
            StringBuilder sb = new StringBuilder();

            writer.Write(sb.ToString());
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            return new Rss.RssItem[0]; // nothing to render in RSS.
        }

    }
}
