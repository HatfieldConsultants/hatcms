using System;
using System.IO;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;

using Hatfield.Web.Portal;
using HatCMS.Placeholders;
using HatCMS.WebEditor.Helpers;
using HatCMS.setup;
using HatCMS.Placeholders.RegisterProject;
using HatCMS.Admin;

namespace HatCMS.Controls.Admin
{
    public partial class AdminMenuControl : System.Web.UI.UserControl
    {
        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("GotoEditModePath", "/_admin/action/gotoEdit"), CmsConfig.Languages));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-1.4.1.min.js"));
            ret.Add(CmsFileDependency.UnderAppPath("css/_system/AdminTools.css"));
            return ret.ToArray();
        }
            

        private CmsBaseAdminTool.CmsAdminToolClass selectedToolToRun
        {
            get
            {
                return (CmsBaseAdminTool.CmsAdminToolClass)PageUtils.getFromForm("RunTool", typeof(CmsBaseAdminTool.CmsAdminToolClass), CmsBaseAdminTool.CmsAdminToolClass.AdminMenu);
            }
        }

        private enum AdminMenuTab { Reports, Tools }
        private AdminMenuTab selectedMenuTab
        {
            get
            {
                return (AdminMenuTab)PageUtils.getFromForm("tab", typeof(AdminMenuTab), AdminMenuTab.Reports);
            }
        }

        private string getCategoryDisplayTitle(CmsBaseAdminTool.CmsAdminToolCategory cat)
        {
            string title = "";
            switch (cat)
            {
                case CmsBaseAdminTool.CmsAdminToolCategory.Report_Image: title = "Image Reports"; break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Report_Page: title = "Page Reports"; break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Report_Feedback: title = "Feedback Reports"; break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Report_Projects: title = "Registered Project Reports"; break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Report_Other: title = "Other Reports"; break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Tool_Search: title = "Search Tools"; break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Tool_Utility: title = "Utilities"; break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Tool_Security: title = "Security Zones"; break;
                default:
                    throw new ArgumentException("Error: invalid/unknown CmsAdminToolCategory in getCategoryDisplayTitle()");
            }
            return title;
        }

        /// <summary>
        /// get the url for a tab
        /// </summary>
        /// <param name="adminPage"></param>
        /// <param name="tool"></param>
        /// <returns></returns>
        private string getTabUrl(CmsPage adminPage, AdminMenuTab Tab)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string tabName = Tab.ToString();
            pageParams.Add("tab", tabName);            
            string url = adminPage.getUrl(pageParams);
            return url;
        }

        /// <summary>
        /// get the url for a tool
        /// </summary>
        /// <param name="adminPage"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
        private string getToolRunUrl(CmsPage adminPage, AdminMenuTab Tab, CmsBaseAdminTool.CmsAdminToolClass toolClassToRun)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string tabName = Tab.ToString();
            string toolNameToRun = toolClassToRun.ToString();
            pageParams.Add("tab", tabName);
            pageParams.Add("RunTool", toolNameToRun);
            string url = adminPage.getUrl(pageParams);
            return url;
        }

               

        protected override void Render(HtmlTextWriter writer)
        {
            if (!CmsContext.currentPage.currentUserCanRead)
            {
                writer.Write("Access Denied");
                return;
            }

            CmsContext.currentPage.HeadSection.AddCSSFile("css/_system/AdminTools.css");

            CmsAdminToolInfo[] allToolInfos = CmsBaseAdminTool.getAllAdminToolInfos();

            StringBuilder html = new StringBuilder();
            html.Append(RenderAdminMenu(allToolInfos, CmsContext.currentLanguage));

            if (selectedToolToRun != CmsBaseAdminTool.CmsAdminToolClass.AdminMenu)
            {
                CmsBaseAdminTool c = CmsBaseAdminTool.getAdminTool(selectedToolToRun);
                html.Append(c.Render()); // execute the admin tool
            }

            writer.Write(html.ToString());
        } // Render

        #region AdminMenu

        private Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> getToolsForTab(AdminMenuTab tab, Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> haystack)
        {
            Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> ret = new Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>>();
            foreach (CmsBaseAdminTool.CmsAdminToolCategory toolCat in haystack.Keys)
            {
                string toolCatName = Enum.GetName(typeof(CmsBaseAdminTool.CmsAdminToolCategory), toolCat);
                switch (tab)
                {
                    case AdminMenuTab.Reports:
                        if (toolCatName.StartsWith("Report", StringComparison.CurrentCultureIgnoreCase))
                            ret.Add(toolCat, haystack[toolCat]);
                        break;
                    case AdminMenuTab.Tools:
                        if (toolCatName.StartsWith("Tool", StringComparison.CurrentCultureIgnoreCase))
                            ret.Add(toolCat, haystack[toolCat]);
                        break;
                    default:
                        throw new Exception("Error: invalid AdminMenuTab");
                }                
            } // foreach
            return ret;
        }

        private string RenderAdminMenu(CmsAdminToolInfo[] allToolInfos, CmsLanguage langToRenderFor)
        {
            StringBuilder html = new StringBuilder();
            CmsPage page = CmsContext.currentPage;
                      
            Dictionary<CmsBaseAdminTool.CmsAdminToolCategory, List<CmsAdminToolInfo>> toolsToDisplay = getToolsForTab(selectedMenuTab, CmsAdminToolInfo.ToCategoryKeyedDictionary(allToolInfos));

            html.Append("<table class=\"AdminMenu\">");
            html.Append("<tr>");
            switch (selectedMenuTab)
            {
                case AdminMenuTab.Reports:                    
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getTabUrl(page, AdminMenuTab.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getTabUrl(page, AdminMenuTab.Tools) + "\">Tools</a></td>");
                    break;
                case AdminMenuTab.Tools:
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getTabUrl(page, AdminMenuTab.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getTabUrl(page, AdminMenuTab.Tools) + "\">Tools</a></td>");
                    break;
            }
            html.Append("</tr>");
            html.Append("<tr><td colspan=\"2\">");
            foreach (CmsBaseAdminTool.CmsAdminToolCategory category in toolsToDisplay.Keys)
            {
                string catDisplayTitle = getCategoryDisplayTitle(category);
                html.Append("<div class=\"AdminTool menu\"><strong>" + catDisplayTitle + ":</strong> ");
                List<string> toolLinks = new List<string>();
                foreach (CmsAdminToolInfo tool in toolsToDisplay[category])
                {
                    string toolName = tool.MenuDisplayText[langToRenderFor];                    
                    string url = getToolRunUrl(page, selectedMenuTab, tool.Class);
                    string link = "<a href=\"" + url + "\">" + toolName + "</a>";
                    toolLinks.Add(link);
                } // foreach

                html.Append(String.Join(" | ", toolLinks.ToArray()));

                html.Append("</div>");
            } // foreach category

            html.Append("</tr></td>");
            html.Append("</table>");

            

            return html.ToString();
        }
        #endregion 

    }

}


