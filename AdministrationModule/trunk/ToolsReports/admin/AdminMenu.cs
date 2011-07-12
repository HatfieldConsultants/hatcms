using System;
using System.Text;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS.Admin
{
    public class AdminMenu : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory._AdminMenu, AdminMenuTab._AdminMenu, "Admin Menu");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("GotoEditModePath", "/_admin/action/gotoEdit"), CmsConfig.Languages));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-1.4.1.min.js"));
            
            // AdminTools.css is now embedded in this project's Assembly.
            ret.Add(CmsFileDependency.UnderAppPath("css/_system/AdminTools.css", CmsDependency.ExistsMode.MustNotExist));
            return ret.ToArray();
        }

        public static BaseCmsAdminTool getToolToRun()
        {
            string name = PageUtils.getFromForm("RunTool", Guid.NewGuid().ToString());
            if (BaseCmsAdminTool.AdminToolExists(name))
            {
                BaseCmsAdminTool ret = BaseCmsAdminTool.getAdminToolInstanceByName(name);
                if (ret != null)
                    return ret;
            }
            return new AdminMenu();
        }


        private Dictionary<BaseCmsAdminTool.CmsAdminToolCategory, List<BaseCmsAdminTool>> getToolsForTab(AdminMenuTab tab, BaseCmsAdminTool[] haystack)
        {
            Dictionary<BaseCmsAdminTool.CmsAdminToolCategory, List<BaseCmsAdminTool>> ret = new Dictionary<CmsAdminToolCategory, List<BaseCmsAdminTool>>();
            foreach(BaseCmsAdminTool tool in haystack)            
            {
                BaseCmsAdminTool.CmsAdminToolCategory toolCat = tool.getToolInfo().Category;
                string toolCatName = toolCat.ToString();
                switch (tab)
                {
                    case AdminMenuTab._AdminMenu:
                        // -- nothing to add for the admin menu
                        break;
                    case AdminMenuTab.Reports:
                        if (toolCatName.StartsWith("Report", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!ret.ContainsKey(toolCat))
                                ret[toolCat] = new List<BaseCmsAdminTool>();

                            ret[toolCat].Add(tool);
                        }
                        break;
                    case AdminMenuTab.Tools:
                        if (toolCatName.StartsWith("Tool", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (!ret.ContainsKey(toolCat))
                                ret[toolCat] = new List<BaseCmsAdminTool>();

                            ret[toolCat].Add(tool);
                        }
                        break;
                    default:
                        throw new Exception("Error: invalid AdminMenuTab");
                }
            } // foreach
            return ret;
        }

        private AdminMenuTab getMenuTabToDisplay(BaseCmsAdminTool toolToRun)
        {
            CmsAdminToolInfo toolInfo = toolToRun.getToolInfo();
            if (toolInfo.MenuTab != AdminMenuTab._AdminMenu)
                return toolInfo.MenuTab;

            return (AdminMenuTab) PageUtils.getFromForm("tab", typeof(AdminMenuTab), AdminMenuTab.Reports);
        }

        private string getTabUrl(CmsPage page, AdminMenuTab selTab)
        {
            NameValueCollection urlParams = new NameValueCollection();
            urlParams.Add("tab", selTab.ToString());
            return page.getUrl(urlParams);
        }

        private string getToolRunUrl(CmsPage page, BaseCmsAdminTool toolToRun)
        {
            NameValueCollection urlParams = new NameValueCollection();
            urlParams.Add("RunTool", toolToRun.GetType().Name);
            return page.getUrl(urlParams);
            
        }

        public override string Render()
        {
            StringBuilder html = new StringBuilder();
            CmsPage page = CmsContext.currentPage;
            CmsLanguage langToRenderFor = CmsContext.currentLanguage;

            page.HeadSection.AddEmbeddedCSSFile(CSSGroup.ControlOrPlaceholder, typeof(AdminMenu).Assembly, "AdminTools.css");

            BaseCmsAdminTool toolToRun = getToolToRun();

            BaseCmsAdminTool.AdminMenuTab selectedMenuTab = getMenuTabToDisplay(toolToRun);
            BaseCmsAdminTool[] allTools = BaseCmsAdminTool.GetAllCachedAdminToolInstances();

            Dictionary<BaseCmsAdminTool.CmsAdminToolCategory, List<BaseCmsAdminTool>> toolsToDisplay = getToolsForTab(selectedMenuTab, allTools);

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
            foreach (BaseCmsAdminTool.CmsAdminToolCategory category in toolsToDisplay.Keys)
            {
                string catDisplayTitle = getCategoryDisplayTitle(category);
                html.Append("<div class=\"AdminTool menu\"><strong>" + catDisplayTitle + ":</strong> ");
                List<string> toolLinks = new List<string>();
                foreach (BaseCmsAdminTool tool in toolsToDisplay[category])
                {
                    CmsAdminToolInfo toolInfo = tool.getToolInfo();
                    string toolMenuText = toolInfo.MenuDisplayText[langToRenderFor];
                    string url = getToolRunUrl(page, tool);
                    string link = "<a href=\"" + url + "\">" + toolMenuText + "</a>";
                    toolLinks.Add(link);
                } // foreach

                html.Append(String.Join(" | ", toolLinks.ToArray()));

                html.Append("</div>");
            } // foreach category

            html.Append("</tr></td>");
            html.Append("</table>");



            return html.ToString();
        }
        

    }
}
