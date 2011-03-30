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
using System.IO;
using HatCMS.Placeholders;
using HatCMS.WebEditor.Helpers;
using System.Collections.Generic;

namespace HatCMS.Admin
{
    public class UnusedFiles : CmsBaseAdminTool
    {
        public override CmsAdminToolInfo GetToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Report_Other, CmsAdminToolClass.UnusedFiles, "Unused files");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        public override string Render()
        {
            string rootDirName = HttpContext.Current.Server.MapPath(InlineImageBrowser2.UserFilesPath);
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
            html.Append("</ol>" + Environment.NewLine);
            html.Append("</p>");

            return html.ToString();
        }


        private List<string> removeUrlsUsedOnPage(CmsPage pageToScan, CmsLanguage pageLanguageToScan, List<string> allUrlsToVerify)
        {
            string[] linksToFind = allUrlsToVerify.ToArray();
            string pageHtml = pageToScan.renderAllPlaceholdersToString(pageLanguageToScan, CmsPage.RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters);

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

                if (f.Name.StartsWith(CmsLocalFileOnDisk.DeletedFileFilenamePrefix)) // skip resources that have been deleted
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


    }
}
