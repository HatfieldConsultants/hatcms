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


        private string getMenuDisplay(CmsBaseAdminTool.CmsAdminToolClass tool)
        {
            string ret = "";
            switch (tool)
            {
                case CmsBaseAdminTool.CmsAdminToolClass.AdminMenu: ret = "Admin Menu"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.SearchAndReplace: ret = "Global Search &amp; Replace"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.SearchHtmlContent: ret = "Search in editable HTML"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.ListUserFeedback: ret = "List User Feedback"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.SearchSingleImagesByCaption: ret = "Search Images by caption"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.LastModifiedTable: ret = "Pages by last modified date"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.DuplicateSingleImages: ret = "Duplicate Images"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.SingleImageMissingCaptions: ret = "Images without captions"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.PageImageSummary: ret = "Images by Page"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.UnusedFiles: ret = "Unused files"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.ValidateConfig: ret = "Validate CMS Config"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.PagesByTemplate: ret = "Pages by template"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.EmptyThumbnailCache: ret = "Empty Image Cache"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.PageUrlsById: ret = "Page Urls by Id"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.ListRegisteredProjects: ret = "List Registered Projects"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.ZoneManagement: ret = "Create/Edit Zones"; break;
                case CmsBaseAdminTool.CmsAdminToolClass.ZoneAuthority: ret = "User permissions"; break;
                default:
                    throw new ArgumentException("An unknown AdminTool was passed to getMenuDisplay()");
            }
            return ret;
        }

        private Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>> CategorizedAdminReports
        {
            get
            {
                Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>> ret = new Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>>();
                ret.Add("Image Reports", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.DuplicateSingleImages, CmsBaseAdminTool.CmsAdminToolClass.PageImageSummary, CmsBaseAdminTool.CmsAdminToolClass.SingleImageMissingCaptions }));
                ret.Add("Page Reports", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.LastModifiedTable, CmsBaseAdminTool.CmsAdminToolClass.PagesByTemplate, CmsBaseAdminTool.CmsAdminToolClass.PageUrlsById }));
                ret.Add("Feedback Reports", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.ListUserFeedback }));
                ret.Add("Registered Project Reports", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.ListRegisteredProjects }));
                ret.Add("Other Reports", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.UnusedFiles, CmsBaseAdminTool.CmsAdminToolClass.ValidateConfig }));
                return ret;
            }
        }

        private Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>> CategorizedAdminTools
        {
            get
            {
                Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>> ret = new Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>>();
                ret.Add("Search Tools", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.SearchAndReplace, CmsBaseAdminTool.CmsAdminToolClass.SearchHtmlContent, CmsBaseAdminTool.CmsAdminToolClass.SearchSingleImagesByCaption }));
                ret.Add("Utilities", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.EmptyThumbnailCache, CmsBaseAdminTool.CmsAdminToolClass.ValidateConfig }));
                ret.Add("Security Zones", new List<CmsBaseAdminTool.CmsAdminToolClass>(new CmsBaseAdminTool.CmsAdminToolClass[] { CmsBaseAdminTool.CmsAdminToolClass.ZoneManagement, CmsBaseAdminTool.CmsAdminToolClass.ZoneAuthority }));
                return ret;
            }
        }





        private CmsBaseAdminTool.CmsAdminToolClass selectedAdminTool
        {
            get
            {
                return (CmsBaseAdminTool.CmsAdminToolClass)PageUtils.getFromForm("AdminTool", typeof(CmsBaseAdminTool.CmsAdminToolClass), CmsBaseAdminTool.CmsAdminToolClass.AdminMenu);
            }
        }

        private string getUrl(CmsPage adminPage, CmsBaseAdminTool.CmsAdminToolClass tool)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string menuName = Enum.GetName(typeof(CmsBaseAdminTool.CmsAdminToolClass), tool);
            pageParams.Add("AdminTool", menuName);
            string url = adminPage.getUrl(pageParams);
            return url;
        }

        private CmsBaseAdminTool.CmsAdminToolCategory selectedAdminMenu
        {
            get
            {
                CmsBaseAdminTool.CmsAdminToolClass selTool = selectedAdminTool;
                if (selTool == CmsBaseAdminTool.CmsAdminToolClass.AdminMenu)
                    return (CmsBaseAdminTool.CmsAdminToolCategory)PageUtils.getFromForm("AdminMenu", typeof(CmsBaseAdminTool.CmsAdminToolCategory), CmsBaseAdminTool.CmsAdminToolCategory.Reports);
                else
                {
                    foreach (string cat in CategorizedAdminReports.Keys)
                    {
                        if (CategorizedAdminReports[cat].IndexOf(selTool) > -1)
                            return CmsBaseAdminTool.CmsAdminToolCategory.Reports;
                    }
                    foreach (string cat in CategorizedAdminTools.Keys)
                    {
                        if (CategorizedAdminTools[cat].IndexOf(selTool) > -1)
                            return CmsBaseAdminTool.CmsAdminToolCategory.Tools;
                    }
                    return CmsBaseAdminTool.CmsAdminToolCategory.Reports;
                }
            }
        }

        private string getUrl(CmsPage adminPage, CmsBaseAdminTool.CmsAdminToolCategory menu)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string menuName = Enum.GetName(typeof(CmsBaseAdminTool.CmsAdminToolCategory), menu);
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

            if (selectedAdminTool != CmsBaseAdminTool.CmsAdminToolClass.AdminMenu)
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
            Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>> toolsToDisplay = new Dictionary<string, List<CmsBaseAdminTool.CmsAdminToolClass>>();

            html.Append("<table class=\"AdminMenu\">");
            html.Append("<tr>");
            switch (selectedAdminMenu)
            {
                case CmsBaseAdminTool.CmsAdminToolCategory.Reports:
                    toolsToDisplay = CategorizedAdminReports;
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getUrl(page, CmsBaseAdminTool.CmsAdminToolCategory.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getUrl(page, CmsBaseAdminTool.CmsAdminToolCategory.Tools) + "\">Tools</a></td>");
                    break;
                case CmsBaseAdminTool.CmsAdminToolCategory.Tools:
                    toolsToDisplay = CategorizedAdminTools;
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getUrl(page, CmsBaseAdminTool.CmsAdminToolCategory.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getUrl(page, CmsBaseAdminTool.CmsAdminToolCategory.Tools) + "\">Tools</a></td>");
                    break;
            }
            html.Append("</tr>");
            html.Append("<tr><td colspan=\"2\">");
            foreach (string category in toolsToDisplay.Keys)
            {
                html.Append("<div class=\"AdminTool menu\"><strong>" + category + ":</strong> ");
                List<string> toolLinks = new List<string>();
                foreach (CmsBaseAdminTool.CmsAdminToolClass tool in toolsToDisplay[category])
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


