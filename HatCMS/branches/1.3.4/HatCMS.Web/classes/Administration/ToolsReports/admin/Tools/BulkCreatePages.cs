using System;
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
using System.IO;
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.Admin
{
    public class BulkCreatePages : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Utility, AdminMenuTab.Tools, "Bulk Create Pages");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        private string getForm()
        {
            StringBuilder html = new StringBuilder();
            string formId = "BulkCreatePagesForm";
            html.Append(CmsContext.currentPage.getFormStartHtml(formId));
            html.Append("<p><strong>Bulk Create Pages</strong></p>");
            html.Append(PageUtils.getHiddenInputHtml("RunTool", this.GetType().Name) + EOL);
            html.Append(PageUtils.getHiddenInputHtml(formId, "1"));
            html.Append("Bar ('|') Seperated Values: (one line per-page to create)<br>");
            html.Append("Line format: [newPageName | newPageTitle | newPageMenuTitle | newPageSearchEngineDescription | newPageShowInMenu | newPageTemplate | newPageParentId]<br>");
            html.Append(PageUtils.getTextAreaHtml("CSVPageInfo", "CSVPageInfo", "", 50, 10));
            html.Append("<br><input type=\"submit\" value=\"Preview\">");
            html.Append(CmsContext.currentPage.getFormCloseHtml(formId));
            return html.ToString();
        }

        private string processSubmission()
        {
            StringBuilder html = new StringBuilder();
            string CSV = PageUtils.getFromForm("CSVPageInfo", "");
            string[] lines = CSV.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            html.Append("<table border=\"1\">");
            foreach (string line in lines)
            {
                string[] vals = line.Split(new char[] { '|' });
                if (vals.Length >= 7)
                {
                    string newPageName = vals[0].Trim();
                    string newPageTitle = vals[1].Trim();
                    string newPageMenuTitle = vals[2].Trim();
                    string newPageSearchEngineDescription = vals[3].Trim();
                    bool newPageShowInMenu = Convert.ToBoolean(vals[4]);
                    string newPageTemplate = vals[5].Trim();
                    int newPageParentId = Convert.ToInt32(vals[6]);

                    CmsCreateNewPageOptions newPageOpts = CmsCreateNewPageOptions.GetInstanceWithNoUserPrompts(newPageName, newPageTitle, newPageMenuTitle, newPageSearchEngineDescription, newPageShowInMenu, newPageTemplate, newPageParentId);
                    NameValueCollection createPageParams = newPageOpts.GetCreatePagePopupParams();

                    string link = CmsPageEditMenu.DefaultStandardActionRenderers.RenderLink("CreateNewPagePath", "/_admin/createPage", createPageParams, CmsContext.currentPage, CmsContext.currentLanguage, "Create Page");
                    html.Append("<tr>");
                    html.Append("<td>" + link + "</td>");
                    html.Append("<td>" + line + "</td>");
                    html.Append("</tr>");
                }
            }
            html.Append("</table>");

            return html.ToString();
        }

        public override string Render()
        {
            StringBuilder html = new StringBuilder();

            int run = PageUtils.getFromForm("BulkCreatePagesForm", 0);
            if (run == 1)
                html.Append(processSubmission());

            html.Append(getForm());

            return html.ToString();
        }

        public override System.Web.UI.WebControls.GridView RenderToGridViewForOutputToExcelFile()
        {
            return null; // not implemented.
        }

       

    }
}
