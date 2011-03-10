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
using System.Collections.Generic;
using System.Collections.Specialized;

namespace HatCMS.Admin
{
    public class PageUrlsById : CmsBaseAdminTool
    {
        public override CmsAdminToolInfo GetToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Report_Page, CmsAdminToolClass.PageUrlsById, "Page Urls by Id");
        }

        public override string Render()
        {
            StringBuilder html = new StringBuilder();
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            html.Append(TABLE_START_HTML);
            html.Append("<tr><th>Page Id</th><th>URL Macro</th><th>Urls</th></tr>");
            foreach (int pageId in allPages.Keys)
            {
                html.Append("<tr>");
                html.Append("<td>" + pageId.ToString() + "</td>");

                html.Append("<td>");
                List<string> outputMacros = new List<string>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    string macro = HtmlLinkMacroFilter.getLinkMacro(allPages[pageId], lang);
                    if (outputMacros.IndexOf(macro) < 0)
                        outputMacros.Add(macro);
                }

                html.Append(string.Join("<br />", outputMacros.ToArray()));

                html.Append("</td>");

                html.Append("<td>");
                List<string> outputUrls = new List<string>();
                foreach (string url in allPages[pageId].Urls)
                {
                    outputUrls.Add("<a href=\"" + url + "\" target=\"blank\">" + url + "</a>");
                } // foreach

                html.Append(string.Join("<br />", outputUrls.ToArray()));

                html.Append("</td>");

                html.Append("<td>");

                NameValueCollection srParams = new NameValueCollection();

                srParams.Add("AdminTool", Enum.GetName(typeof(CmsBaseAdminTool.CmsAdminToolClass), CmsBaseAdminTool.CmsAdminToolClass.SearchAndReplace));
                string searchForUrl = allPages[pageId].Urls[0];
                if (CmsConfig.Languages.Length > 1 && searchForUrl.StartsWith("/" + CmsConfig.Languages[0].shortCode, StringComparison.CurrentCultureIgnoreCase))
                {
                    searchForUrl = searchForUrl.Substring(("/" + CmsConfig.Languages[0].shortCode).Length);
                }
                srParams.Add("searchfor", searchForUrl);
                srParams.Add("replacewith", outputMacros[0]);

                string srUrl = CmsContext.currentPage.getUrl(srParams, CmsUrlFormat.FullIncludingProtocolAndDomainName);
                html.Append("<input type=\"text\" value=\"" + srUrl + "\">");

                html.Append("</td>");

                html.Append("</tr>");
            } // foreach
            html.Append("</table>");
            return html.ToString();
        } // RenderPageUrlsById


    }
}
