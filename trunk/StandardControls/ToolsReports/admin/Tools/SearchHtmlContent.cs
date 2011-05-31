using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using HatCMS.Placeholders;
using Hatfield.Web.Portal;
using System.Collections.Generic;

namespace HatCMS.Admin
{
    public class SearchHtmlContent : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Search, AdminMenuTab.Tools, "Search in editable HTML");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.AddRange(PlaceholderUtils.getDependencies("HtmlContent"));
            return ret.ToArray();
        }

        public override string Render()
        {
            CmsPage currentPage = CmsContext.currentPage;
            string searchText = PageUtils.getFromForm("AuditSearch", "");
            searchText = searchText.Trim();

            StringBuilder html = new StringBuilder();

            // -- start query form
            string formId = "searchHtmlAudit";
            html.Append(currentPage.getFormStartHtml(formId));
            html.Append("<strong>Search Editable HTML Content (slow!): </strong> ");
            html.Append(PageUtils.getInputTextHtml("AuditSearch", "AuditSearch", searchText, 40, 1024));            
            html.Append("<input type=\"submit\" value=\"search\">");
            html.Append(PageUtils.getHiddenInputHtml("AdminTool", GetType().Name ));
            html.Append(currentPage.getFormCloseHtml(formId));

            if (searchText != "")
            {
                // do the search
                html.Append("<table>");
                html.Append("<tr><td colspan=\"2\">Page (link opens in another window)</td></tr>" + Environment.NewLine);

                Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
                int numRowsOutput = 0;
                foreach (int pageId in allPages.Keys)
                {
                    CmsPage page = allPages[pageId];

                    foreach (CmsLanguage lang in CmsConfig.Languages)
                    {
                        string placeholderHTML = page.renderPlaceholdersToString("HtmlContent", lang, CmsPage.RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters);
                        placeholderHTML = placeholderHTML.Replace('\r', ' '); // remove line breaks
                        placeholderHTML = placeholderHTML.Replace('\n', ' ');
                        placeholderHTML = placeholderHTML.Replace(Environment.NewLine, " ");
                        // string plainText = StringUtils.StripHTMLTags(placeholderHTML);
                        if (placeholderHTML.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) > -1)
                        {
                            html.Append("<tr>");
                            string pageUrl = page.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName, lang);
                            html.Append("<td><a href=\"" + pageUrl + "\" target=\"_blank\">" + page.getPath(lang) + "</a></td>");

                            string snippet = getHtmlContentSearchSnippet(placeholderHTML, searchText);
                            html.Append("<td>" + snippet + "</td>"); ;
                            html.Append("</tr>" + Environment.NewLine);
                            numRowsOutput++;
                            break; // next page
                        }

                    } // foreach language

                } // foreach page

                if (numRowsOutput < 1)
                {
                    html.Append("<tr><td><em>No pages found</em></td></tr>");
                }

                html.Append("</table>" + Environment.NewLine);
            }  // if doSearch

            return html.ToString();
        } // RenderSearchHtmlContent

        private string getHtmlContentSearchSnippet(string htmlContent, string searchText)
        {
            int snippetWindowPre = 20;
            int snippetWindowPost = 20;

            string plainText = (htmlContent);

            int index = plainText.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase);

            int snippetStart = index - snippetWindowPre;
            if (snippetStart < 0)
                snippetStart = 0;

            int snippetEnd = index + snippetWindowPost;
            if (snippetEnd >= plainText.Length)
                snippetEnd = plainText.Length - 1;

            string snippet = plainText.Substring(snippetStart, snippetEnd - snippetStart);
            snippet = HttpContext.Current.Server.HtmlEncode(snippet);

            snippet = StringUtils.Replace(snippet, searchText, "<strong>" + searchText + "</strong>", true); // case insensitive

            return snippet;
        } // getHtmlContentSearchSnippet


    }
}
