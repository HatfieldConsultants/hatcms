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
using HatCMS.placeholders.RegisterProject;
using HatCMS.controls.Admin;

namespace HatCMS.Controls.Admin
{
    public partial class Audit : System.Web.UI.UserControl
    {
        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("GotoEditModePath", "/_admin/action/gotoEdit"), CmsConfig.Languages));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-1.4.1.min.js"));
            ret.Add(CmsFileDependency.UnderAppPath("css/_system/AdminTools.css"));
            return ret.ToArray();
        }
        
        
        public enum AdminTool { 
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

        private string getMenuDisplay(AdminTool tool)
        {
            string ret = "";
            switch (tool)
            {
                case AdminTool.AdminMenu: ret = "Admin Menu"; break;
                case AdminTool.SearchAndReplace: ret = "Global Search &amp; Replace"; break;
                case AdminTool.SearchHtmlContent: ret = "Search in editable HTML"; break;
                case AdminTool.ListUserFeedback: ret = "List User Feedback"; break;
                case AdminTool.SearchSingleImagesByCaption: ret = "Search Images by caption"; break;
                case AdminTool.LastModifiedTable: ret = "Pages by last modified date"; break;
                case AdminTool.DuplicateSingleImages: ret = "Duplicate Images"; break;
                case AdminTool.SingleImageMissingCaptions: ret = "Images without captions"; break;
                case AdminTool.PageImageSummary: ret = "Images by Page"; break;
                case AdminTool.UnusedFiles: ret = "Unused files"; break;
                case AdminTool.ValidateConfig: ret = "Validate CMS Config"; break;
                case AdminTool.PagesByTemplate: ret = "Pages by template"; break;
                case AdminTool.EmptyThumbnailCache: ret = "Empty Image Cache"; break;
                case AdminTool.PageUrlsById: ret = "Page Urls by Id"; break;
                case AdminTool.ListRegisteredProjects: ret = "List Registered Projects"; break;
                case AdminTool.ZoneManagement: ret = "Zone Management"; break;
                case AdminTool.ZoneAuthority: ret = "Zone Authority"; break;
                default:
                    throw new ArgumentException("An unknown AdminTool was passed to getMenuDisplay()");
            }
            return ret;
        }

        private Dictionary<string, List<AdminTool>> CategorizedAdminReports
        {
            get
            {
                Dictionary<string, List<AdminTool>> ret = new Dictionary<string, List<AdminTool>>();                
                ret.Add("Image Reports", new List<AdminTool>(new AdminTool[] { AdminTool.DuplicateSingleImages, AdminTool.PageImageSummary, AdminTool.SingleImageMissingCaptions }));
                ret.Add("Page Reports", new List<AdminTool>(new AdminTool[] { AdminTool.LastModifiedTable, AdminTool.PagesByTemplate, AdminTool.PageUrlsById }));
                ret.Add("Feedback Reports", new List<AdminTool>(new AdminTool[] { AdminTool.ListUserFeedback }));
                ret.Add("Registered Project Reports", new List<AdminTool>(new AdminTool[] { AdminTool.ListRegisteredProjects }));
                ret.Add("Other Reports", new List<AdminTool>(new AdminTool[] { AdminTool.UnusedFiles, AdminTool.ValidateConfig }));
                return ret;
            }
        }

        private Dictionary<string, List<AdminTool>> CategorizedAdminTools
        {
            get
            {
                Dictionary<string, List<AdminTool>> ret = new Dictionary<string, List<AdminTool>>();
                ret.Add("Search Tools", new List<AdminTool>(new AdminTool[] { AdminTool.SearchAndReplace, AdminTool.SearchHtmlContent, AdminTool.SearchSingleImagesByCaption }));
                ret.Add("Utilities", new List<AdminTool>(new AdminTool[] { AdminTool.EmptyThumbnailCache, AdminTool.ValidateConfig }));
                ret.Add("Zones", new List<AdminTool>(new AdminTool[] { AdminTool.ZoneManagement, AdminTool.ZoneAuthority }));
                return ret;
            }
        }

        private enum AdminMenu { Reports, Tools }

        

        private AdminTool selectedAdminTool
        {
            get
            {
                return (AdminTool)PageUtils.getFromForm("AdminTool", typeof(AdminTool), AdminTool.AdminMenu);
            }
        }

        private string getUrl(CmsPage adminPage, AdminTool tool)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string menuName = Enum.GetName(typeof(AdminTool), tool);
            pageParams.Add("AdminTool", menuName);
            string url = adminPage.getUrl(pageParams);
            return url;
        }

        private AdminMenu selectedAdminMenu
        {
            get
            {
                AdminTool selTool = selectedAdminTool;
                if (selTool == AdminTool.AdminMenu)
                    return (AdminMenu)PageUtils.getFromForm("AdminMenu", typeof(AdminMenu), AdminMenu.Reports);
                else
                {
                    foreach (string cat in CategorizedAdminReports.Keys)
                    {
                        if (CategorizedAdminReports[cat].IndexOf(selTool) > -1)
                            return AdminMenu.Reports;
                    }
                    foreach (string cat in CategorizedAdminTools.Keys)
                    {
                        if (CategorizedAdminTools[cat].IndexOf(selTool) > -1)
                            return AdminMenu.Tools;
                    }
                    return AdminMenu.Reports;
                }
            }
        }

        private string getUrl(CmsPage adminPage, AdminMenu menu)
        {
            NameValueCollection pageParams = new NameValueCollection();
            string menuName = Enum.GetName(typeof(AdminMenu), menu);
            pageParams.Add("AdminMenu", menuName);
            string url = adminPage.getUrl(pageParams);
            return url;
        }

        private string SingleImageHtmlDisplay(SingleImageData img)
        {
            StringBuilder html = new StringBuilder();
            string thumbUrl = showThumbPage.getThumbDisplayUrl(img.ImagePath, 150, 150);
            html.Append("<img src=\"" + thumbUrl + "\">");
            html.Append("<br />" + img.ImagePath + "");
            html.Append("<br />Caption: " + img.Caption + "");
            html.Append("<br />Credits: " + img.Credits + "");

            return html.ToString();
        }
               

        protected override void Render(HtmlTextWriter writer)
        {
            if (!CmsContext.currentUserCanAuthor)
            {
                writer.Write("Access Denied");
                return;
            }

            CmsContext.currentPage.HeadSection.AddCSSFile("css/_system/AdminTools.css");

            StringBuilder html = new StringBuilder();
            html.Append(RenderAdminMenu());
            switch (selectedAdminTool)
            {
                case AdminTool.SearchAndReplace:
                    html.Append(RenderSearchAndReplace());
                    break;
                case AdminTool.SearchHtmlContent:
                    html.Append(RenderSearchHtmlContent());
                    break;
                case AdminTool.LastModifiedTable:
                    html.Append(RenderLastModifiedTable());
                    break;
                case AdminTool.SingleImageMissingCaptions:
                    html.Append(RenderSingleImageMissingCaptions());
                    break;
                case AdminTool.UnusedFiles:
                    html.Append(RenderUnusedFiles());
                    break;
                case AdminTool.ValidateConfig:
                    html.Append(RenderValidateConfig());
                    break;
                case AdminTool.DuplicateSingleImages:
                    html.Append(RenderDuplicateSingleImages());
                    break;
                case AdminTool.PageImageSummary:
                    html.Append(RenderPageImageSummary());
                    break;
                case AdminTool.PagesByTemplate:
                    html.Append(RenderPagesByTemplate());
                    break;
                case AdminTool.SearchSingleImagesByCaption:
                    html.Append(RenderSingleImagesByCaption());
                    break;                
                case AdminTool.EmptyThumbnailCache:
                    html.Append(RenderEmptyThumbnailCache());
                    break;
                case AdminTool.PageUrlsById:
                    html.Append(RenderPageUrlsById());
                    break;
                case AdminTool.ListUserFeedback:
                    html.Append(RenderListUserFeedback());
                    break;
                case AdminTool.ListRegisteredProjects:
                    html.Append(RenderListRegisteredProjects());
                    break;
                case AdminTool.ZoneManagement:
                case AdminTool.ZoneAuthority:
                    AdminController c = AdminController.getController(selectedAdminTool);
                    html.Append(c.Render());
                    break;
                default:
                    break;
            } // switch

            writer.Write(html.ToString());
        } // Render

        #region AdminMenu

        private string RenderAdminMenu()
        {
            StringBuilder html = new StringBuilder();
            CmsPage page = CmsContext.currentPage;
            Dictionary<string, List<AdminTool>> toolsToDisplay = new Dictionary<string, List<AdminTool>>();

            html.Append("<table class=\"AdminMenu\">");
            html.Append("<tr>");
            switch (selectedAdminMenu)
            {
                case AdminMenu.Reports:
                    toolsToDisplay = CategorizedAdminReports;
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getUrl(page, AdminMenu.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getUrl(page, AdminMenu.Tools) + "\">Tools</a></td>");
                    break;
                case AdminMenu.Tools:
                    toolsToDisplay = CategorizedAdminTools;
                    html.Append("<td class=\"MenuNotSel\"><a href=\"" + getUrl(page, AdminMenu.Reports) + "\">Reports</a></td>");
                    html.Append("<td class=\"MenuSel\"><a href=\"" + getUrl(page, AdminMenu.Tools) + "\">Tools</a></td>");
                    break;
            }
            html.Append("</tr>");
            html.Append("<tr><td colspan=\"2\">");
            foreach (string category in toolsToDisplay.Keys)
            {
                html.Append("<div class=\"AdminTool menu\"><strong>" + category + ":</strong> ");
                List<string> toolLinks = new List<string>();
                foreach (AdminTool tool in toolsToDisplay[category])
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

        #region LastModifiedTable
        private string RenderLastModifiedTable()
        {
            StringBuilder html = new StringBuilder();
            Dictionary<int, CmsPage> pages = CmsContext.HomePage.getLinearizedPages();
            // change the NameObjectCollection into a sortable list
            List<CmsPage> allPages = new List<CmsPage>();
            foreach(int pageId in pages.Keys)
            {
                allPages.Add(pages[pageId]);
            }
            CmsPage[] sortedPages = CmsPage.SortPagesByLastModifiedDate(allPages.ToArray());

            html.Append("<table border=\"1\">"+Environment.NewLine);
            string rowHeader = ("<tr><th>Last Modified</th><th>Created on</th><th>Title</th><th>Path</th></tr>");
            string lastTitle = "";
            foreach (CmsPage p in sortedPages)
            {
                string title = getLastModifiedTitle(p);
                if (title != lastTitle)
                {                    
                    html.Append("<tr><th colspan=\"4\" style=\"background-color: #CCC;\"><strong>"+title+"</strong></th></tr>"+Environment.NewLine);
                    html.Append(rowHeader);
                }

                string tdStyle = "";
                if (DateTime.Compare(p.CreatedDateTime.Date, p.LastUpdatedDateTime.Date) == 0)
                    tdStyle = " style=\"background-color: yellow\"";
                    

                html.Append("<tr>");
                html.Append("<td" + tdStyle + ">" + p.LastUpdatedDateTime.ToString("d MMM yyyy") + "</td>");
                html.Append("<td" + tdStyle + ">" + p.CreatedDateTime.ToString("d MMM yyyy") + "</td>");
                html.Append("<td" + tdStyle + ">" + p.Title + "</td>");
                html.Append("<td" + tdStyle + "><a target=\"_blank\" href=\"" + p.Url + "\">" + p.Path + "</a></td>");
                html.Append("</tr>" + Environment.NewLine);

                lastTitle = title;
            } // foreach
            html.Append("</table>"+Environment.NewLine);

            return html.ToString();
        } // RenderLastModifiedTable

        private string getLastModifiedTitle(CmsPage p)
        {
            TimeSpan timespan = TimeSpan.FromTicks(DateTime.Now.Ticks - p.LastUpdatedDateTime.Ticks);
            if (timespan.TotalDays < 7)            
                return "Less than a week ago";            
            else if (timespan.TotalDays < 31)
                return "Less than a month ago";
            else if (timespan.TotalDays < 365)
            {
                int monthsAgo = Convert.ToInt32( Math.Round(timespan.TotalDays / 31));
                return monthsAgo.ToString() + " months ago";
            }
            else
            {
                int yearsAgo = Convert.ToInt32(Math.Round(timespan.TotalDays / 365));
                return yearsAgo.ToString() + " years ago";
            }

        }

#endregion 

        #region SearchHtmlContent
        private string RenderSearchHtmlContent()
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
            html.Append(PageUtils.getHiddenInputHtml("AdminTool", Enum.GetName(typeof(AdminTool), AdminTool.SearchHtmlContent)));
            html.Append(currentPage.getFormCloseHtml(formId));

            if (searchText != "")
            {
                // do the search
                html.Append("<table>");
                html.Append("<tr><td colspan=\"2\">Page (link opens in another window)</td></tr>"+Environment.NewLine);

                Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
                int numRowsOutput = 0;
                foreach (int pageId in allPages.Keys)
                {
                    CmsPage page = allPages[pageId];

                    foreach (CmsLanguage lang in CmsConfig.Languages)
                    {
                        string placeholderHTML = page.renderPlaceholdersToString("HtmlContent", lang);
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

            string snippet = plainText.Substring(snippetStart, snippetEnd-snippetStart);
            snippet = Server.HtmlEncode(snippet);

            snippet = StringUtils.Replace(snippet, searchText, "<strong>" + searchText + "</strong>", true); // case insensitive

            return snippet;
        } // getHtmlContentSearchSnippet
#endregion

        #region SingleImageMissingCaptions

        private string RenderSingleImageMissingCaptions()
        {
            StringBuilder html = new StringBuilder();

            html.Append("<p>The following images are missing captions:");

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            List<CmsPage> pagesToGetImagesFrom = new List<CmsPage>();
            foreach (int pageId in allPages.Keys)
            {
                pagesToGetImagesFrom.Add(allPages[pageId]);
            }
            SingleImageDb singleImageDb = new SingleImageDb();
            List<SingleImageData> imageDatas = new List<SingleImageData>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                imageDatas.AddRange(singleImageDb.getSingleImages(pagesToGetImagesFrom.ToArray(), lang));
            }

            List<SingleImageData> imageDatasWithoutCaptions = new List<SingleImageData>();
            foreach (SingleImageData img in imageDatas)
            {
                if (img.ImagePath.Trim() != "" && img.Caption.Trim() == "")
                    imageDatasWithoutCaptions.Add(img);
            }

            if (imageDatasWithoutCaptions.Count < 1)
                html.Append("<br><strong>No images are missing captions! (" + imageDatas.Count.ToString() + " images audited)</strong>");
            else
            {
                html.Append("<br>" + imageDatasWithoutCaptions.Count.ToString() + " images are missing captions (" + imageDatas.Count.ToString() + " images audited)");
                html.Append("<table border=\"1\">"+Environment.NewLine);
                html.Append("<tr><th>Image</th><th>On Page</th></tr>");
                foreach (SingleImageData img in imageDatasWithoutCaptions)
                {
                    html.Append("<tr>");
                    html.Append("<td>" + SingleImageHtmlDisplay(img) + "</td>");
                    html.Append("<td>");
                    CmsPage containingPage = img.getPageContainingImage(pagesToGetImagesFrom.ToArray());
                    if (containingPage != null)
                        html.Append("<a target=\"_blank\" href=\"" + containingPage.Url + "\">" + containingPage.Path + "</a>");
                    else
                        html.Append("Invalid page found!!");

                    html.Append("</td>");
                    html.Append("</tr>"+Environment.NewLine);
                }
                html.Append("</table>"+Environment.NewLine);
            }


            html.Append("</p>");

            return html.ToString();

        }
        #endregion

        #region UnusedFiles
        private string RenderUnusedFiles()
        {
            string rootDirName = Server.MapPath(InlineImageBrowser2.UserFilesPath);
            string[] fileUrls = recursiveGatherFileUrls(new DirectoryInfo(rootDirName));
            int totalUrls = fileUrls.Length;

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            
            List<string> fileUrlsNotUsed = new List<string>(fileUrls);
            foreach (int pageId in allPages.Keys)
            {
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    if (fileUrlsNotUsed.Count == 0)
                        break;
                    CmsPage p = allPages[pageId];
                    fileUrlsNotUsed = removeUrlsUsedOnPage(p, lang, fileUrlsNotUsed);
                }
            } // foreach 

            StringBuilder html = new StringBuilder();
            html.Append("<p>User Files that do not appear to be used anywhere: (" + fileUrlsNotUsed.Count + " of " + totalUrls.ToString() + " links)");
            html.Append("<ol>");
            foreach (string url in fileUrlsNotUsed)
            {
                html.Append("<li><a target=\"_blank\" href=\"" + CmsContext.ApplicationPath + url + "\">" + url + "</a></li>" + Environment.NewLine);
            }
            html.Append("</ol>"+Environment.NewLine);
            html.Append("</p>");

            return html.ToString();
        }


        private List<string> removeUrlsUsedOnPage(CmsPage pageToScan, CmsLanguage pageLanguageToScan, List<string> allUrlsToVerify)
        {            
            string[] linksToFind = allUrlsToVerify.ToArray();
            string pageHtml = pageToScan.renderAllPlaceholdersToString(pageLanguageToScan);

            string[] foundLinks = ContentUtils.FindFileLinksInHtml(pageHtml, linksToFind);

            if (foundLinks.Length > 0)
            {
                foreach (string url in foundLinks)
                {
                    allUrlsToVerify.Remove(url);
                } // foreach
            }

            return allUrlsToVerify;
        }
        

        /// <summary>
        /// note: urls gathered in this function do not have the ApplicationPath in them
        /// </summary>
        /// <param name="baseDir"></param>
        /// <returns></returns>
        private string[] recursiveGatherFileUrls(DirectoryInfo baseDir)
        {
            List<string> ret = new List<string>();

            FileInfo[] files = baseDir.GetFiles();
            foreach (FileInfo f in files)
            {
                // skip hidden files
                if ((f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    break;

                if (f.Name.StartsWith(CmsResource.DeletedFileFilenamePrefix)) // skip resources that have been deleted
                    break;

                string url = InlineImageBrowser2.ReverseMapPath(f.FullName);
                // -- urls should not have the Application path in them
                if (url.StartsWith(CmsContext.ApplicationPath))
                    url = url.Substring(CmsContext.ApplicationPath.Length); 
                ret.Add(url);
            }

            foreach (DirectoryInfo subDir in baseDir.GetDirectories())
            {
                // skip hidden files
                if ((subDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    break;
                // skip system files
                if ((subDir.Attributes & FileAttributes.System) == FileAttributes.System)
                    break;
                ret.AddRange(recursiveGatherFileUrls(subDir));
            } // foreach

            return ret.ToArray();
        } // recursiveGatherFileUrls
        #endregion

        #region ValidateConfig
        private string RenderValidateConfig()
        {
            StringBuilder html = new StringBuilder();
            setupPage.ConfigValidationMessage[] msgs = setupPage.VerifyConfig(Context);
            setupPage.ConfigValidationMessage[] errorMessages = setupPage.ConfigValidationMessage.getAllInvalidMessages(msgs);
            if (errorMessages.Length == 0)
            {
                html.Append("<p style=\"color: green;\">Configuration has been validated without errors</p>");
            }
            else
            {

                html.Append("<div style=\"color: red;\">Error validating config: </div>");
                html.Append("<ul>");

                foreach (setupPage.ConfigValidationMessage m in errorMessages)
                {
                    html.Append("<li>" + m.message + "</li>");
                }
                html.Append("</ul>");

            }
            return html.ToString();
        }
        #endregion


        #region DuplicateSingleImages
        private string RenderDuplicateSingleImages()
        {
            
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            
            SingleImageDb db = new SingleImageDb();           

            List<PageImages> pageImages = new List<PageImages>();

            foreach (int pageId in allPages.Keys)
            {
                CmsPage pageToTest = allPages[pageId];
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    SingleImageData[] imgDataArr = db.getSingleImages(new CmsPage[] { allPages[pageId] }, lang);
                    PageImages pi = new PageImages(pageToTest, lang, imgDataArr);
                    pageImages.Add(pi);
                }
            }

            Dictionary<string, List<PageLanguage>> duplicates = new Dictionary<string, List<PageLanguage>>();
            foreach (PageImages pi in pageImages)
            {
                foreach (SingleImageData img in pi.Images)
                {
                    PageLanguage[] matchingPages = PageImages.GetMatchingPagesForImage(img, pageImages.ToArray());
                    if (matchingPages.Length > 1)
                    {
                        if (!duplicates.ContainsKey(img.ImagePath))
                            duplicates[img.ImagePath] = new List<PageLanguage>();

                        duplicates[img.ImagePath].AddRange(matchingPages);
                    }
                }
            }

            // -- remove duplicate PageLanguage items
            Dictionary<string, List<PageLanguage>> toReport = new Dictionary<string, List<PageLanguage>>();
            foreach (string imgPath in duplicates.Keys)
            {
                toReport[imgPath] = PageLanguage.RemoveDuplicates(duplicates[imgPath]);
            } // foreach


            // -- display results

            StringBuilder html = new StringBuilder();
            html.Append("<p><strong>Duplicate images used on this site:</strong></p>");
            html.Append("<table border=\"1\">");
            html.Append("<tr><th>Image</th><th>Found on pages</th></tr>");
            if (toReport.Keys.Count > 0)
            {
                foreach (string imgPath in toReport.Keys)
                {
                    html.Append("<tr>");
                    string thumbUrl = showThumbPage.getThumbDisplayUrl(imgPath, 150, 150);
                    html.Append("<td><img src=\"" + thumbUrl + "\"><br />" + imgPath + "</td>");
                    html.Append("<td>");
                    html.Append("<ul>");
                    foreach (PageLanguage targetPage in toReport[imgPath])
                    {
                        html.Append("<li><a href=\"" + targetPage.Page.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName, targetPage.Language) + "\" target=\"_blank\">" + targetPage.Page.getTitle(targetPage.Language) + " (" + targetPage.Language.shortCode + ") </li>");
                    }
                    html.Append("</ul>");
                    html.Append("</td>");
                    html.Append("</tr>");
                } // foreach
            }
            else
            {
                html.Append("<tr><td><em>No duplicate images found</em></td></tr>");                  
            }
            html.Append("</table>");

            return html.ToString();
        }

        private SingleImageData[] ImgPathAlreadyExists(SingleImageData[] haystack, SingleImageData needle)
        {
            List<SingleImageData> ret = new List<SingleImageData>();
            foreach (SingleImageData img in haystack)
            {
                if (String.Compare(img.ImagePath,needle.ImagePath, true) == 0)
                    ret.Add( img );
            }
            return ret.ToArray();
        }        

        private class PageLanguage
        {
            public CmsPage Page;
            public CmsLanguage Language;

            public PageLanguage(CmsPage p, CmsLanguage l)
            {
                Page = p;
                Language = l;                
            }

            public static List<PageLanguage> RemoveDuplicates(List<PageLanguage> arr)
            {
                List<string> existingPageIdLangCodes = new List<string>();
                List<PageLanguage> ret = new List<PageLanguage>();
                foreach (PageLanguage pl in arr)
                {
                    // string key = pl.Page.ID.ToString() + pl.Language.shortCode;
                    // note: to show duplicates between language versions of the same page, use the language code here!
                    string key = pl.Page.ID.ToString();
                    if (existingPageIdLangCodes.IndexOf(key) < 0)
                    {
                        ret.Add(pl);
                        existingPageIdLangCodes.Add(key);
                    }
                } // foreach

                return ret;
            }
        }

        private class PageImages
        {
            public CmsPage Page;
            public CmsLanguage Language;
            public SingleImageData[] Images;

            public PageImages(CmsPage p, CmsLanguage l, SingleImageData[] images)
            {
                Page = p;
                Language = l;
                Images = images;
            }

            public static PageLanguage[] GetMatchingPagesForImage(SingleImageData needle, PageImages[] haystack)
            {
                List<PageLanguage> ret = new List<PageLanguage>();
                foreach (PageImages pi in haystack)
                {
                    foreach (SingleImageData img in pi.Images)
                    {
                        if (String.Compare(img.ImagePath, needle.ImagePath, true) == 0)
                            ret.Add(new PageLanguage(pi.Page, pi.Language));
                    } // foreach
                } // foreach
                return ret.ToArray();
            }
            
        }


        #endregion

        #region PageImageSummary
        private string RenderPageImageSummary()
        {
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            StringBuilder html = new StringBuilder();
            html.Append("<p><strong>Page - Image Summary</strong></p>");
            html.Append("<table border=\"1\">");
            html.Append("<tr><th>Page</th><th>Images</th></tr>");
            SingleImageDb db = new SingleImageDb();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage targetPage = allPages[pageId];
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    html.Append("<tr>");
                    html.Append("<td><a href=\"" + targetPage.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName) + "\" target=\"_blank\">" + targetPage.Path + "</a> (\"" + targetPage.Title + "\")</td>");
                    html.Append("<td><table border=\"0\">");
                    SingleImageData[] pageImages = db.getSingleImages(new CmsPage[] { targetPage }, lang);
                    if (pageImages.Length > 0)
                    {
                        foreach (SingleImageData imgData in pageImages)
                        {
                            string thumbUrl = showThumbPage.getThumbDisplayUrl(imgData.ImagePath, 150, 150);
                            html.Append("<tr><td><img src=\"" + thumbUrl + "\"><br />Caption: " + imgData.Caption + "<br />Source:" + imgData.Credits + "<br />Path: " + imgData.ImagePath + "</td></tr>");
                        } // foreach
                    }
                    else
                    {
                        html.Append("<tr><td><em>Page doesn't have any images...</em></td></tr>");
                    }


                    html.Append("</table></td>");
                    html.Append("<tr>");
                } // foreach language
            } // foreach page
            html.Append("</table>");
            return html.ToString();
        }
        #endregion

        #region RenderPagesByTemplate
        private string RenderPagesByTemplate()
        {
            Dictionary<string, List<CmsPage>> reportStorage = new Dictionary<string, List<CmsPage>>();

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage targetPage = allPages[pageId];
                if (!reportStorage.ContainsKey(targetPage.TemplateName.ToLower()))
                    reportStorage[targetPage.TemplateName.ToLower()] = new List<CmsPage>();

                reportStorage[targetPage.TemplateName.ToLower()].Add(targetPage);
            } // foreach

            StringBuilder html = new StringBuilder();
            html.Append("<table border=\"0\">");
            foreach (string templateName in reportStorage.Keys)
            {
                html.Append("<tr><td style=\"background-color: #CCC;\"><strong>" + templateName + "</strong><td></tr>");
                foreach (CmsPage targetPage in reportStorage[templateName])
                {
                    html.Append("<tr><td><a href=\"" + targetPage.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName) + "\" target=\"_blank\">" + targetPage.Title + "</td></tr>");
                } // foreach
            } // foreach
            html.Append("</table>");
            return html.ToString();

        }
        #endregion

        #region RenderSingleImagesByCaption
        private string RenderSingleImagesByCaption()
        {
            CmsPage currentPage = CmsContext.currentPage;
            string searchText = PageUtils.getFromForm("AuditSearch", "");
            searchText = searchText.Trim();

            StringBuilder html = new StringBuilder();

            // -- start query form
            string formId = "searchImageCaptions";
            html.Append(currentPage.getFormStartHtml(formId));
            html.Append("<strong>Search Image Captions: </strong> ");
            html.Append(PageUtils.getInputTextHtml("AuditSearch", "AuditSearch", searchText, 40, 1024));
            html.Append("<input type=\"submit\" value=\"search\">");
            html.Append(PageUtils.getHiddenInputHtml("AdminTool", Enum.GetName(typeof(AdminTool), AdminTool.SearchSingleImagesByCaption)));
            html.Append(currentPage.getFormCloseHtml(formId));
                        
            if (searchText != "")
            {
                Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
                List<CmsPage> pages = new List<CmsPage>(allPages.Values);
                
                SingleImageDb db = new SingleImageDb();
                List<SingleImageData> imgDatas = new List<SingleImageData>();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    imgDatas.AddRange(db.getSingleImages(pages.ToArray(), lang));
                }

                html.Append("<p><strong>Images containing \""+searchText+"\":</strong></p>");
                html.Append("<table border=\"1\">");
                html.Append("<tr><th>Image</th><th>Page Link</th></tr>");
                foreach (SingleImageData img in imgDatas)
                {
                    
                    
                    if (img.Caption.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) > -1 ||
                        img.Credits.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) > -1)
                    {
                        html.Append("<tr>");
                        html.Append("<td>");
                        html.Append(SingleImageHtmlDisplay(img));
                        html.Append("</td>");
                        CmsPage targetPage = CmsContext.getPageById(img.PageId);
                        html.Append("<td><a target=\"_blank\" href=\""+targetPage.getUrl( CmsUrlFormat.FullIncludingProtocolAndDomainName)+"\">"+targetPage.Title+"</a></td>");
                        html.Append("</tr>");
                    } // if
                    
                } // foreach
                html.Append("</table>");
            }

            return html.ToString();
        }
        #endregion


        #region EmptyThumbnailCache
        private string RenderEmptyThumbnailCache()
        {
            StringBuilder html = new StringBuilder();

            string thumbDir = showThumbPage.ThumbImageCacheDirectory;
            FileInfo[] files = (new DirectoryInfo(thumbDir)).GetFiles();
            html.Append("Attempting to delete " + files.Length + " files in the thumbnail cache...<br>");
            int deleted = 0;
            foreach (FileInfo f in files)
            {                
                try
                {
                    f.Delete();
                    deleted ++;
                }
                catch
                {}
                
            } // foreach

            int numCached = CmsResource.DeleteAllCachedThumbnailUrls();

            html.Append(deleted.ToString() + " files and " + numCached + " URLs in the thumbnail cache have been deleted.<br>");

            

            return html.ToString();
        }
        #endregion

        #region SearchAndReplace
        private string RenderSearchAndReplace()
        {
            string searchForFormName = "searchfor";
            string replaceWithFormName = "replacewith";

            string searchFor = PageUtils.getFromForm(searchForFormName, "");
            string replaceWith = PageUtils.getFromForm(replaceWithFormName, "");

            //@@TODO: add an option to only replace text (not HTML); use this: http://gallery.msdn.microsoft.com/ScriptJunkie/en-us/jQuery-replaceText-String-ce62ea13

            // -- because javascript regular expressions are used for the replacement, some search expressions will have unintended consequences 
            //    note some of these are now escaped using EscapeJSRegex so can be used. "[", "]", "+", "*", "?", "$", "^",".",
            //     ref: http://www.w3schools.com/jsref/jsref_obj_regexp.asp
            string[] invalidSearchItems = new string[] {  "{", "}",  "\\w", "\\W", "\\d", "\\D", "\\s", "\\S", "\\b", "\\B", "\\0", "\\n", "\\f", "\\r", "\\t", "\\v", "\\x", "\\u" };
            string _errorMessage = "";
            if (searchFor.Trim() != "")
            {
                foreach (string s in invalidSearchItems)
                {
                    if (searchFor.IndexOf(s) > -1)
                    {
                        _errorMessage = "Error: searches can not contain \"" + s + "\".";
                    }
                } // foreach
            }

            if (searchFor.Trim() != "" && replaceWith.Trim() != "" && _errorMessage == "")
            {
                StringBuilder script = new StringBuilder();
                script.Append("var langCode = '" + CmsContext.currentLanguage.shortCode + "'; " + Environment.NewLine);
                script.Append("var buttonId = ''; " + Environment.NewLine);
                script.Append("function go(url, lcode, bId){" + Environment.NewLine);
                script.Append(" opener.location.href = url;" + Environment.NewLine);
                script.Append(" langCode = lcode;" + Environment.NewLine);
                script.Append(" buttonId = bId;" + Environment.NewLine);

                script.Append(" setTimeout(poll, 1000);" + Environment.NewLine);

                // http://plugins.jquery.com/project/popupready
                script.Append("function poll() {" + Environment.NewLine);
                script.Append("if (jQuery(\"body *\", opener.document).length == 0 ) {" + Environment.NewLine);
                script.Append(" setTimeout(poll, 1000);" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);
                script.Append("else {" + Environment.NewLine);
                script.Append(" $(opener.document).focus(); " + Environment.NewLine);
                script.Append(" setTimeout(doReplace, 1000); " + Environment.NewLine);
                script.Append("}" + Environment.NewLine);
                script.Append("}// poll" + Environment.NewLine);
                                                                

                script.Append("} // go" + Environment.NewLine);

                script.Append("function numOccurrences(txt){" + Environment.NewLine);
                script.Append(" var r = txt.match(new RegExp('" + EscapeJSRegex(searchFor) + "','gi')); " + Environment.NewLine);
                script.Append(" if (r) return r.length; " + Environment.NewLine);
                script.Append(" return 0;" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);


                script.Append("function setMsg(bodyEl, startNum, endNum, el){" + Environment.NewLine);
                script.Append(" var pos = el.offset(); " + Environment.NewLine);
                script.Append(" var e = $( opener.document.createElement('div') ); var nm =''; var n =1; " + Environment.NewLine);
                script.Append(" if (endNum - startNum > 1){ nm = (startNum+1) + ' - '+(endNum); n = endNum - (startNum); } else { nm= (endNum); }" + Environment.NewLine);
                script.Append(" e.html(nm+\": Replaced '" + searchFor + "' with '" + replaceWith + "' \"+n+\" times \");" + Environment.NewLine);
                script.Append(" e.css({'padding': '5px','font-size':'8pt', 'display':'block','z-index':'5000', 'font-weight':'bold', 'position':'absolute', 'top': pos.top, 'left': pos.left, 'background-color':'yellow', 'border': '2px solid red'});" + Environment.NewLine);

                script.Append(" bodyEl.append(e);" + Environment.NewLine);
                // script.Append(" alert($(document).scrollTop());" + Environment.NewLine);
                // Note: there is a bug in JQuery.offset() function that uses the document's scrollTop, not the context's scrollTop
                //       bug report: http://dev.jquery.com/ticket/6539
                script.Append(" e.css({'top': pos.top - 20 -  $(document).scrollTop() });" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);
                


                script.Append("function doReplace(){" + Environment.NewLine);
                script.Append(" var bodyEl = $('#lang_'+langCode, opener.document);" + Environment.NewLine);

                script.Append(" var numChanges = 0;" + Environment.NewLine);

                script.Append(" $('#lang_'+langCode+' input:text,#lang_'+langCode+' textarea', opener.document).each(function(){" + Environment.NewLine);

                script.Append("     var ths = $(this); " + Environment.NewLine);
                // script.Append("     alert(ths.val());" + Environment.NewLine);
                script.Append("     if (ths.is(':visible') &&  ths.val().trim() != '' && ths.val().search(new RegExp('" + EscapeJSRegex(searchFor) + "','gi')) > -1 ) {" + Environment.NewLine);
                script.Append("         var startNum = numChanges; " + Environment.NewLine);
                script.Append("         numChanges+= numOccurrences(ths.val());" + Environment.NewLine);
                script.Append("         setMsg(bodyEl, startNum, numChanges, ths);" + Environment.NewLine);
                script.Append("         var v = ths.val().replace(new RegExp('" + EscapeJSRegex(searchFor) + "','gi'), '" + replaceWith + "');" + Environment.NewLine);
                script.Append("         ths.val(v);" + Environment.NewLine);
                script.Append("     }// if visible" + Environment.NewLine);
                script.Append(" });" + Environment.NewLine);

                script.Append(" if(opener.CKEDITOR && (opener.CKEDITOR.status == 'basic_loaded' || opener.CKEDITOR.status == 'basic_ready' || opener.CKEDITOR.status == 'ready') ){ " + Environment.NewLine);
                script.Append("     for(var edName in opener.CKEDITOR.instances) { " + Environment.NewLine);
                script.Append("         if ($('#'+edName, opener.document).closest('#lang_'+langCode, opener.document).is(':visible')) {" + Environment.NewLine);
                script.Append("             var d = opener.CKEDITOR.instances[edName].getData();" + Environment.NewLine);
                script.Append("             var numD = numOccurrences(d); " + Environment.NewLine);
                script.Append("             if (numD > 0) {" + Environment.NewLine);
                script.Append("                 var d2 = d.replace(new RegExp('" + EscapeJSRegex(searchFor) + "','gi'), '" + replaceWith + "'); " + Environment.NewLine);
                script.Append("                 var nStart = numChanges; numChanges += numD;" + Environment.NewLine);
                script.Append("                 setMsg(bodyEl, nStart, numChanges, $('#'+opener.CKEDITOR.instances[edName].container.$.id, opener.document));" + Environment.NewLine);
                script.Append("                 opener.CKEDITOR.instances[edName].setData(d2);" + Environment.NewLine);
                script.Append("             } // if " + Environment.NewLine);
                script.Append("         } // if " + Environment.NewLine);
                // script.Append("         alert('parent: '+$('#'+edName, opener.document).closest('#lang_en', opener.document).is(':visible'));" + Environment.NewLine);
                // window.CKEDITOR.instances['htmlcontent_1_1_en'].getData()
                script.Append("     }// for each editor" + Environment.NewLine);
                script.Append(" }" + Environment.NewLine);


                script.Append("$('#'+buttonId).val(numChanges+' replacements made');" + Environment.NewLine);
                script.Append(" alert('The text on this page has been updated ('+numChanges+' replacements made).\\nPlease save the page to continue.');" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);
                // another $(opener.document).ready way: http://plugins.jquery.com/taxonomy/term/1219

                CmsContext.currentPage.HeadSection.AddJSStatements(script.ToString());

                CmsContext.currentPage.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            }

            StringBuilder html = new StringBuilder();
            html.Append(CmsContext.currentPage.getFormStartHtml("SearchReplaceForm"));
            html.Append("<table>");
            html.Append("<tr>");
            html.Append("<td>Search for:</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(searchForFormName, searchForFormName, searchFor, 30, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Replace with:</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(replaceWithFormName, replaceWithFormName, replaceWith, 30, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td colspan=\"2\"><input type=\"submit\" value=\"Search in all pages\" /> (no replacement of page contents will be done)</td>");
            html.Append("</tr>");

            html.Append("</table>");
            html.Append("<em>Warning! This searches and replaces the raw HTML for a page, so be careful what you search for! The search is not case-sensitive, and the replacement is done exactly.</em>");

            if (_errorMessage != "")
            {
                html.Append("<p style=\"color: red; font-weight: bold;\">"+_errorMessage+"</p>");
            }
            
            html.Append(PageUtils.getHiddenInputHtml("AdminTool", Enum.GetName(typeof(AdminTool), AdminTool.SearchAndReplace)));
            html.Append(CmsContext.currentPage.getFormCloseHtml("SearchReplaceForm"));
            // -- do the search
            if (searchFor.Trim() != "" && replaceWith.Trim() != "" && _errorMessage == "")
            {
                html.Append("<table border=\"1\">");
                PageSearchResults[] searchResults = getAllPagesContainingText(searchFor);
                if (searchResults.Length == 0)
                {
                    html.Append("<p><strong>No pages were found that contained \"" + searchFor + "\"</strong></p>");
                }
                else
                {
                    int i = 1;
                    foreach (PageSearchResults searchResult in searchResults)
                    {
                        CmsPage p = searchResult.Page;

                        html.Append("<tr>");
                        html.Append("<td>" + p.getTitle(searchResult.Language) + " (" + searchResult.Language.shortCode + ")</td>");

                        NameValueCollection paramList = new NameValueCollection();
                        paramList.Add("target", p.ID.ToString());
                        string openEditUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("GotoEditModePath", "/_admin/action/gotoEdit"), paramList, searchResult.Language);

                        string buttonId = "searchReplaceb_" + i.ToString();
                        i++;

                        html.Append("<td>");
                        html.Append("<input id=\"" + buttonId + "\" type=\"button\" onclick=\"go('" + openEditUrl + "', '" + searchResult.Language.shortCode + "','" + buttonId + "'); return false;\" value=\"open page &amp; replace\">");

                        html.Append("</td>");

                        html.Append("</tr>");
                    } // foreach page Id
                }
                html.Append("</table>");
            }
            return html.ToString();
        }

        /// <summary>
        /// Escape characters in a search string so that they can be used in a RexExp object as plain text (not their special meta-character values)
        /// </summary>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        private string EscapeJSRegex(string searchFor)
        {
            string doubleSlash = @"\\";

            // "+", "*", "?", "$", "^"
            StringBuilder ret = new StringBuilder(searchFor);

            ret.Replace("[", doubleSlash + "[");
            ret.Replace("]", doubleSlash + "]");
            ret.Replace(".", doubleSlash+".");
            ret.Replace("?", doubleSlash+"?");
            ret.Replace("$", doubleSlash+"$");
            ret.Replace("^", doubleSlash+"^");
            ret.Replace("+", doubleSlash + "+");
            ret.Replace("*", doubleSlash + "*");
            

            return ret.ToString();
        }

        private class PageSearchResults
        {
            public CmsLanguage Language;
            public CmsPage Page;

            public PageSearchResults(CmsLanguage lang, CmsPage page)
            {
                Language = lang;
                Page = page;
            }
        }
        

        private PageSearchResults[] getAllPagesContainingText(string searchFor)
        {
            List<PageSearchResults> ret = new List<PageSearchResults>();
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage p = allPages[pageId];

                CmsPlaceholderDefinition[] phDefs = p.getAllPlaceholderDefinitions();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    bool foundInPage = false;
                    foreach (CmsPlaceholderDefinition phDef in phDefs)
                    {
                        if (foundInPage)
                            break;

                        string phVal = PlaceholderUtils.renderPlaceholderToString(p, lang, phDef);
                        if (phVal.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            foundInPage = true;
                            break;
                        }

                    } // foreach placeholder type

                    if (foundInPage)
                    {
                        ret.Add(new PageSearchResults(lang, p));
                    }
                } // foreach language

            } // foreach page

            return ret.ToArray();
        }

        #endregion

        private string RenderPageUrlsById()
        {
            StringBuilder html = new StringBuilder();
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            html.Append("<table border=\"1\">");
            html.Append("<tr><th>Page Id</th><th>URL Macro</th><th>Urls</th></tr>");
            foreach (int pageId in allPages.Keys)
            {
                html.Append("<tr>");
                html.Append("<td>"+pageId.ToString()+"</td>");

                html.Append("<td>");
                List<string> outputMacros = new List<string>();
                foreach(CmsLanguage lang in CmsConfig.Languages)
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
                
                srParams.Add("AdminTool", Enum.GetName(typeof(AdminTool), AdminTool.SearchAndReplace));
                string searchForUrl = allPages[pageId].Urls[0];
                if (CmsConfig.Languages.Length > 1 && searchForUrl.StartsWith("/"+CmsConfig.Languages[0].shortCode, StringComparison.CurrentCultureIgnoreCase))
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

        private string RenderListUserFeedback()
        {
            UserFeedbackDb db = new UserFeedbackDb();
            UserFeedbackSubmittedData[] arr = db.FetchAllUserFeedbackSubmittedData();
            if (arr.Length == 0)
            {
                return "<p><em>No User feedback has been submitted</em></p>";
            }
            StringBuilder html = new StringBuilder();
            html.Append("<p>");
            html.Append("<table border=\"1\">");
            html.Append("<caption><h2>User feedback <a style=\"font-size: small;\" href=\"/_system/download.ashx?adminTool=" + selectedAdminTool.ToString() + "\">(download)</a></h2></caption>");
            html.Append("<tr>");
            html.Append("<th>Submitted</th>");
            html.Append("<th>Name</th><th>Email Address</th><th>Location</th><th>Question</th><th>Answer</th><th>ReferringUrl</th>");
            html.Append("</tr>");
            foreach(UserFeedbackSubmittedData d in arr)
            {
                html.Append("<tr>");
                html.Append("<td>"+d.dateTimeSubmitted.ToString("yyyy-MM-dd")+"</td>");
                html.Append("<td>" + d.Name + "</td><td>" + d.EmailAddress + "</td><td>" + d.Location + "</td><td>" + d.TextAreaQuestion + "</td><td>" + d.TextAreaValue + "</td><td>" + d.ReferringUrl + "</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            html.Append("</p>");
            return html.ToString();
        }

        /// <summary>
        /// List the registered projects in descending order;
        /// Provide a download link.
        /// </summary>
        /// <returns></returns>
        private string RenderListRegisteredProjects()
        {
            RegisterProjectDb db = new RegisterProjectDb();
            List<RegisterProjectDb.RegisterProjectData> list = db.fetchAll();
            if (list.Count == 0)
                return "<p><em>No registered project</em></p>";

            StringBuilder html = new StringBuilder();
            html.Append("<p>");
            html.Append("<table border=\"1\" cellspacing=\"0\" cellpadding=\"2\" style=\"border-collapse: collapse;\">");
            html.Append("<caption><h2>Registered Projects <a style=\"font-size: small;\" href=\"/_system/download.ashx?adminTool=" + selectedAdminTool.ToString() + "\">(download)</a></h2></caption>");
            html.Append("<tr>");
            html.Append("<th>Name</th><th>Location</th><th>Description</th><th>Contact Person</th><th>Email</th><th>Telephone</th><th>Cellphone</th><th>Website</th><th>Funding Source</th><th>Date/Time Created</th><th>IP Address</th>");
            html.Append("</tr>");

            foreach (RegisterProjectDb.RegisterProjectData d in list)
            {
                html.Append("<tr>");
                html.Append("<td>" + d.Name + "</td>");
                html.Append("<td>" + d.Location + "</td>");
                html.Append("<td>" + d.Description +"</td>");
                html.Append("<td>" + d.ContactPerson +"</td>");
                html.Append("<td><a href=\"mailto:" + d.Email + "\">" + d.Email + "</a></td>");
                html.Append("<td>" + d.Telephone +"</td>");
                html.Append("<td>" + d.Cellphone +"</td>");
                html.Append("<td>" + d.Website +"</td>");
                html.Append("<td>" + d.FundingSource +"</td>");
                html.Append("<td>" + d.CreatedDateTime.ToString() +"</td>");
                html.Append("<td>" + d.ClientIp +"</td>");
                html.Append("</tr>");
            }

            html.Append("</table>");
            html.Append("</p>");
            return html.ToString();
        }
    }

}


