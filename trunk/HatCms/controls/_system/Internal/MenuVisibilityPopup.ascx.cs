using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;
using System.Text;
using System.Collections.Generic;

namespace HatCMS.controls._system.Internal
{
    public partial class MenuVisibilityPopup : System.Web.UI.UserControl
    {
        protected static string EOL = Environment.NewLine;

        protected void Page_Load(object sender, EventArgs e)
        {
            CmsPageHeadSection h = CmsContext.currentPage.HeadSection;
            h.AddCSSStyleStatements("th { border-bottom: 1px solid #B0B0B0; text-align: left; }");
            h.AddCSSStyleStatements("td { padding-right: 1em; }");
            h.AddCSSStyleStatements("span { font-size: larger; font-weight: bold; color: #B0B0B0; }");
            h.AddCSSStyleStatements(".err { color: red; }");
            h.AddCSSStyleStatements(".success { color: green; font-weight: bold; }");
            h.AddCSSStyleStatements(".highlight { background-color: #FFFF66; }");
            h.AddCSSStyleStatements(".rmk { font-size: smaller; }");
        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("ChangeMenuVisibilityPath", "/_admin/actions/MenuVisibilityPopup"), CmsConfig.Languages));
            return ret.ToArray();
        }

        /// <summary>
        /// Get the page ID of the window opener page
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        protected int getTargetPageId(HtmlTextWriter writer)
        {
            int pageId = PageUtils.getFromForm("target", Int32.MinValue);
            if (pageId < 0)
            {
                writer.WriteLine("<p class=\"err\">Invalid Target parameter. No page to change.</p>");
                return Int32.MinValue;
            }

            if (!CmsContext.pageExists(pageId))
            {
                writer.WriteLine("<p class=\"err\">Target page does not exist. No page to change.</p>");
                return Int32.MinValue;
            }

            return pageId;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            int pageId = getTargetPageId(writer);
            if (pageId == Int32.MinValue)
                return;

            Stack<CmsPage> cmsPageStack = new Stack<CmsPage>();
            getCmsPageTree(cmsPageStack, pageId);

            string controlId = "MenuVisibilityPopup_";

            if (PageUtils.getFromForm(controlId + "action", "") == "updateForm")
                writer.Write(handleHtmlFormSubmit(cmsPageStack, pageId, controlId));
            else
                writer.Write(displayHtmlForm(cmsPageStack, pageId, controlId));
        }

        /// <summary>
        /// Read the parent cms page until reaching the website home page
        /// </summary>
        /// <param name="cmsPageStack"></param>
        /// <param name="pageId"></param>
        protected void getCmsPageTree(Stack<CmsPage> cmsPageStack, int pageId)
        {
            CmsPage cmsPage = CmsContext.getPageById(pageId);
            cmsPageStack.Push(cmsPage);
            int parentId = cmsPage.ParentID;
            if (parentId > 0)
                getCmsPageTree(cmsPageStack, parentId);
        }

        /// <summary>
        /// Handle when user clicks update button.  Save the ShowInMenu indicator.
        /// </summary>
        /// <param name="cmsPageStack"></param>
        /// <param name="pageId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string handleHtmlFormSubmit(Stack<CmsPage> cmsPageStack, int pageId, string controlId)
        {
            StringBuilder msg = new StringBuilder();
            int count = cmsPageStack.Count;
            CmsPage page = new CmsPage();
            for (int x = 0; x < count; x++) // update all parent page (until reaching master home page)
            {
                page = cmsPageStack.Pop();
                msg.Append(updateCmsPage(page, controlId));
            }

            CmsPage[] childPageArray = page.ChildPages;
            foreach (CmsPage p in childPageArray) // update all child pages
                msg.Append(updateCmsPage(p, controlId));

            if (msg.Length == 0) // no error
                return displayPostSubmitHtml(page);
            else // error during update
                return msg.ToString();
        }

        /// <summary>
        /// Display a message to indicate update is success.  Display a close popup button.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        protected string displayPostSubmitHtml(CmsPage page)
        {
            StringBuilder script = new StringBuilder("<script>" + EOL);
            script.Append("function go(url){" + EOL);
            script.Append("opener.location.href = url;" + EOL);
            script.Append("window.close();\n}");
            script.Append("</script>" + EOL);
            script.Append("<p class=\"success\">Menu Visibility has successfully been changed.</p>");
            script.Append("<input type=\"button\" onclick=\"go('" + page.Url + "');\" value=\"Close this window\" />");
            return script.ToString();
        }

        /// <summary>
        /// Display a html form and let user to update the ShowInMenu indicator.
        /// </summary>
        /// <param name="cmsPageStack"></param>
        /// <param name="pageId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string displayHtmlForm(Stack<CmsPage> cmsPageStack, int pageId, string controlId)
        {
            StringBuilder html = new StringBuilder(CmsContext.currentPage.getFormStartHtml(controlId + "form"));
            html.Append("<table width=\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">" + EOL);
            html.Append(formatHeaderRow());

            int count = cmsPageStack.Count;
            CmsPage page = new CmsPage();
            for (int x = 0; x < count; x++) // show all parent page (until reaching master home page)
            {
                page = cmsPageStack.Pop();
                if (x + 1 == count)
                    html.Append(formatRow(page, x, "&#9668; Current page", true));
                else
                    html.Append(formatRow(page, x, "", true));
            }

            CmsPage[] childPageArray = page.ChildPages;
            for (int x = 0; x < childPageArray.Length; x++) // show all child pages
            {
                if (x + 1 == childPageArray.Length)
                    html.Append(formatRow(childPageArray[x], count, "", true));
                else
                    html.Append(formatRow(childPageArray[x], count, "", false));
            }

            html.Append("</table>" + EOL);

            html.Append("<p>" + EOL);
            html.Append(PageUtils.getHiddenInputHtml("target", pageId));
            html.Append(PageUtils.getHiddenInputHtml(controlId + "action", "updateForm"));
            html.Append("<input type=\"submit\" value=\"Update\"> ");
            html.Append("<input type=\"button\" value=\"Cancel\" onclick=\"window.close();\">");
            html.Append("</p>" + EOL);
            html.Append(CmsContext.currentPage.getFormCloseHtml(controlId + "form"));

            return html.ToString();
        }

        /// <summary>
        /// Format the html table column header
        /// </summary>
        /// <returns></returns>
        protected string formatHeaderRow()
        {
            StringBuilder html = new StringBuilder("<tr>" + EOL);
            html.Append("<th>Page Title</th>" + EOL);
            html.Append("<th>Template</th>" + EOL);
            html.Append("<th>Show In<br />Menu?</th>" + EOL);
            html.Append("</tr>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// For the html table row to display all parent pages, and all child pages.
        /// </summary>
        /// <param name="cmsPage"></param>
        /// <param name="indentLevel"></param>
        /// <param name="remarks"></param>
        /// <param name="lastChild"></param>
        /// <returns></returns>
        protected string formatRow(CmsPage cmsPage, int indentLevel, string remarks, bool lastChild)
        {
            if (cmsPage.TemplateName[0] == '_')
                return "";

            string controlId = "MenuVisibilityPopup_" + cmsPage.ID.ToString();
            StringBuilder html = new StringBuilder("<tr>" + EOL);

            if (remarks == "")
                html.Append("<tr>" + EOL);
            else
                html.Append("<tr class=\"highlight\">" + EOL);

            string indent = Math.Max(0, indentLevel - 0.5).ToString();
            indent = indent.Replace(',', '.'); // bug fixing due to language issue (e.g. 1.5 in EN, but 1,5 in PT), CSS is EN only
            html.Append("<td style=\" white-space: nowrap; padding-left: " + indent + "em;\">" + EOL);

            if (indentLevel > 0 && lastChild)
                html.Append("<span>&#9492; </span>");
            else if (indentLevel > 0)
                html.Append("<span>&#9500; </span>");

            html.Append(cmsPage.Title);
            html.Append("</td>" + EOL);

            html.Append("<td style=\"font-size: smaller;\">" + EOL);
            html.Append(cmsPage.TemplateName);
            html.Append("</td>" + EOL);

            html.Append("<td align=\"center\"> " + EOL);
            if (cmsPage.ID != CmsContext.HomePage.ID)
            {
                bool boxChecked = cmsPage.ShowInMenu;
                bool editable = cmsPage.Zone.canWrite(CmsContext.currentWebPortalUser); // if zone is authorized, checkbox is editable by user
                string htmlName = controlId + "_show";
                if (editable)
                    html.Append(PageUtils.getCheckboxHtml("", htmlName, htmlName, "true", boxChecked, "", false) + EOL);
                else
                {
                    html.Append(PageUtils.getCheckboxHtml("", "", "", "", boxChecked, "", true) + EOL);
                    html.Append(PageUtils.getHiddenInputHtml(htmlName, "true") + EOL);
                }
            }
            html.Append("</td>" + EOL);

            if (remarks != "")
                html.Append("<td class=\"rmk\">" + remarks + "</td>" + EOL);

            html.Append("</tr>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Update the ShowInMenu indicator.  If the new value is equal
        /// to the old value, the SQL update will be skipped.
        /// </summary>
        /// <param name="cmsPage"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string updateCmsPage(CmsPage cmsPage, string controlId)
        {
            if (cmsPage.ID == CmsContext.HomePage.ID)  // home page has nothing to do with ShowInMenu indicator
                return "";

            string[] checkboxVal = PageUtils.getFromForm(controlId + cmsPage.ID + "_show");
            bool show = false;
            if (checkboxVal.Length == 1 && checkboxVal[0] == "true")
                show = true;

            if (cmsPage.ShowInMenu != show) // if the value is changing from T>F or F>T, run SQL
            {
                cmsPage.ShowInMenu = show;
                bool result = new CmsPageDb().updateShowInMenuIndicator(cmsPage, cmsPage.ShowInMenu);
                return result ? "" : "<p class=\"err\">Error updating '" + cmsPage.Title + "' page, please contact administrator.</p>";
            }
            return "";
        }
    }
}