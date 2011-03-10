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
using HatCMS.Controls.Admin;

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

        private enum CmsAdminToolCategory { Reports, Tools }

        // Note: if using an AdminController based class, the name of the class must match EXACTLY the name listed here,
        // and be in the HatCMS.Controls.Admin namespace.
        public enum CmsAdminToolEnum { 
            AdminMenu, 
            SearchAndReplace, 
            SearchHtmlContent, 
            ListUserFeedback, 
            SearchSingleImagesByCaption, 
            LastModifiedTable, 
            DuplicateSingleImages, 
            SingleImageMissingCaptions, 
            PageImageSummary, 
            UnusedFiles, 
            ValidateConfig, 
            PagesByTemplate, 
            EmptyThumbnailCache, 
            PageUrlsById,
            ListRegisteredProjects,
            ZoneManagement,
            ZoneAuthority
        }

        private string getMenuDisplay(CmsAdminToolEnum tool)
        {
            string ret = "";
            switch (tool)
            {
                case CmsAdminToolEnum.AdminMenu: ret = "Admin Menu"; break;
                case CmsAdminToolEnum.SearchAndReplace: ret = "Global Search &amp; Replace"; break;
                case CmsAdminToolEnum.SearchHtmlContent: ret = "Search in editable HTML"; break;
                case CmsAdminToolEnum.ListUserFeedback: ret = "List User Feedback"; break;
                case CmsAdminToolEnum.SearchSingleImagesByCaption: ret = "Search Images by caption"; break;
                case CmsAdminToolEnum.LastModifiedTable: ret = "Pages by last modified date"; break;
                case CmsAdminToolEnum.DuplicateSingleImages: ret = "Duplicate Images"; break;
                case CmsAdminToolEnum.SingleImageMissingCaptions: ret = "Images without captions"; break;
                case CmsAdminToolEnum.PageImageSummary: ret = "Images by Page"; break;
                case CmsAdminToolEnum.UnusedFiles: ret = "Unused files"; break;
                case CmsAdminToolEnum.ValidateConfig: ret = "Validate CMS Config"; break;
                case CmsAdminToolEnum.PagesByTemplate: ret = "Pages by template"; break;
                case CmsAdminToolEnum.EmptyThumbnailCache: ret = "Empty Image Cache"; break;
                case CmsAdminToolEnum.PageUrlsById: ret = "Page Urls by Id"; break;
                case CmsAdminToolEnum.ListRegisteredProjects: ret = "List Registered Projects"; break;
                case CmsAdminToolEnum.ZoneManagement: ret = "Create/Edit Zones"; break;
                case CmsAdminToolEnum.ZoneAuthority: ret = "User permissions"; break;
                default:
                    throw new ArgumentException("An unknown AdminTool was passed to getMenuDisplay()");
            }
            return ret;
        }

        private Dictionary<string, List<CmsAdminToolEnum>> CategorizedAdminReports
        {
            get
            {
                Dictionary<string, List<CmsAdminToolEnum>> ret = new Dictionary<string, List<CmsAdminToolEnum>>();                
                ret.Add("Image Reports", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.DuplicateSingleImages, CmsAdminToolEnum.PageImageSummary, CmsAdminToolEnum.SingleImageMissingCaptions }));
                ret.Add("Page Reports", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.LastModifiedTable, CmsAdminToolEnum.PagesByTemplate, CmsAdminToolEnum.PageUrlsById }));
                ret.Add("Feedback Reports", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.ListUserFeedback }));
                ret.Add("Registered Project Reports", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.ListRegisteredProjects }));
                ret.Add("Other Reports", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.UnusedFiles, CmsAdminToolEnum.ValidateConfig }));
                return ret;
            }
        }

        private Dictionary<string, List<CmsAdminToolEnum>> CategorizedAdminTools
        {
            get
            {
                Dictionary<string, List<CmsAdminToolEnum>> ret = new Dictionary<string, List<CmsAdminToolEnum>>();
                ret.Add("Search Tools", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.SearchAndReplace, CmsAdminToolEnum.SearchHtmlContent, CmsAdminToolEnum.SearchSingleImagesByCaption }));
                ret.Add("Utilities", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.EmptyThumbnailCache, CmsAdminToolEnum.ValidateConfig }));
                ret.Add("Security Zones", new List<CmsAdminToolEnum>(new CmsAdminToolEnum[] { CmsAdminToolEnum.ZoneManagement, CmsAdminToolEnum.ZoneAuthority }));
                return ret;
            }
        }

        

        

        private CmsAdminToolEnum selectedAdminTool
        {
            get
            {
                return (CmsAdminToolEnum)PageUtils.getFromForm("AdminTool", typeof(CmsAdminToolEnum), CmsAdminToolEnum.AdminMenu);
            }
        }

        private string getUrl(CmsPage adminPage, CmsAdminToolEnum tool)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string menuName = Enum.GetName(typeof(CmsAdminToolEnum), tool);
            pageParams.Add("AdminTool", menuName);
            string url = adminPage.getUrl(pageParams);
            return url;
        }

        private CmsAdminToolCategory selectedAdminMenu
        {
            get
            {
                CmsAdminToolEnum selTool = selectedAdminTool;
                if (selTool == CmsAdminToolEnum.AdminMenu)
                    return (CmsAdminToolCategory)PageUtils.getFromForm("AdminMenu", typeof(CmsAdminToolCategory), CmsAdminToolCategory.Reports);
                else
                {
                    foreach (string cat in CategorizedAdminReports.Keys)
                    {
                        if (CategorizedAdminReports[cat].IndexOf(selTool) > -1)
                            return CmsAdminToolCategory.Reports;
                    }
                    foreach (string cat in CategorizedAdminTools.Keys)
                    {
                        if (CategorizedAdminTools[cat].IndexOf(selTool) > -1)
                            return CmsAdminToolCategory.Tools;
                    }
                    return CmsAdminToolCategory.Reports;
                }
            }
        }

        private string getUrl(CmsPage adminPage, CmsAdminToolCategory menu)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string menuName = Enum.GetName(typeof(CmsAdminToolCategory), menu);
            pageParams.Add("AdminMenu", menuName);
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

            StringBuilder html = new StringBuilder();
            html.Append(RenderAdminMenu());

            if (selectedAdminTool != CmsAdminToolEnum.AdminMenu)
            {
                CmsBaseAdminTool c = CmsBaseAdminTool.getAdminTool(selectedAdminTool);
                html.Append(c.Render());
            }

            writer.Write(html.ToString());
        } // Render

        #region AdminMenu

        private string RenderAdminMenu()
        {
            StringBuilder html = new StringBuilder();
            CmsPage page = CmsContext.currentPage;
            Dictionary<string, List<CmsAdminToolEnum>> toolsToDisplay = new Dictionary<string, List<CmsAdminToolEnum>>();

            html.Append("<table class=\"AdminMenu\">");
            html.Append("<tr>");
            switch (selectedAdminMenu)
            {
                case CmsAdminToolCategory.Reports:
                    toolsToDisplay = CategorizedAdminReports;
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getUrl(page, CmsAdminToolCategory.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getUrl(page, CmsAdminToolCategory.Tools) + "\">Tools</a></td>");
                    break;
                case CmsAdminToolCategory.Tools:
                    toolsToDisplay = CategorizedAdminTools;
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getUrl(page, CmsAdminToolCategory.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getUrl(page, CmsAdminToolCategory.Tools) + "\">Tools</a></td>");
                    break;
            }
            html.Append("</tr>");
            html.Append("<tr><td colspan=\"2\">");
            foreach (string category in toolsToDisplay.Keys)
            {
                html.Append("<div class=\"AdminTool menu\"><strong>" + category + ":</strong> ");
                List<string> toolLinks = new List<string>();
                foreach (CmsAdminToolEnum tool in toolsToDisplay[category])
                {
                    string toolName = getMenuDisplay(tool);                    
                    string url = getUrl(page, tool);
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


