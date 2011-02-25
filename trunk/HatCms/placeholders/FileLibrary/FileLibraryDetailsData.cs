using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Placeholders
{
    public class FileLibraryDetailsData
    {
        private int pageId = -1;
        public int PageId
        {
            get { return pageId; }
            set { pageId = value; }
        }

        private int identifier = -1;
        public int Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }

        private CmsLanguage lang = CmsConfig.Languages[0];
        public CmsLanguage Lang
        {
            get { return lang; }
            set { lang = value; }
        }

        private string fileName = "";
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private int categoryId = -1;
        public int CategoryId
        {
            get { return categoryId; }
            set { categoryId = value; }
        }

        private string author = "";
        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        private string description = "";
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private DateTime lastModified = DateTime.Now;
        public DateTime LastModified
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        private string createdBy = "";
        public string CreatedBy
        {
            get { return createdBy; }
            set { createdBy = value; }
        }

        private int eventPageId = -1;
        public int EventPageId
        {
            get { return eventPageId; }
            set { eventPageId = value; }
        }

        public string fileExtension
        {
            get { return Path.GetExtension(FileName).ToLower(); }
        }

        /// <summary>
        /// Search the category name from a list using the instance's category id
        /// </summary>
        /// <param name="categoryList"></param>
        /// <returns></returns>
        public string getCategoryName(List<FileLibraryCategoryData> categoryList)
        {
            foreach (FileLibraryCategoryData c in categoryList)
            {
                if (c.CategoryId == CategoryId)
                    return c.CategoryName;
            }
            return CategoryId.ToString();
        }

        /// <summary>
        /// Check if an event is required under the current category
        /// </summary>
        /// <param name="categoryList"></param>
        /// <returns></returns>
        public bool isEventRequired(List<FileLibraryCategoryData> categoryList)
        {
            foreach (FileLibraryCategoryData c in categoryList)
            {
                if (c.CategoryId == CategoryId)
                    return c.EventRequired;
            }
            return false;
        }

        public static FileLibraryDetailsData[] getFilesByCategory(List<FileLibraryDetailsData> haystack, FileLibraryCategoryData categoryToMatch)
        {
            List<FileLibraryDetailsData> ret = new List<FileLibraryDetailsData>();
            foreach (FileLibraryDetailsData f in haystack)
            {
                if (f.CategoryId == categoryToMatch.CategoryId)
                    ret.Add(f);
            }
            return ret.ToArray();
        }

        /// <summary>
        /// derive the file name on the web server disk
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="identifier"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string getTargetNameOnDisk(CmsPage aggregatorPage, int identifier, CmsLanguage language, string fileName)
        {
            StringBuilder sb = new StringBuilder(getFileStorageFolderUrl(aggregatorPage, identifier, language));

            if (CmsConfig.getConfigValue("DMSFileStorageLocationVersion", "V1") == "V1")
            {
                sb.Append(aggregatorPage.ID);
                sb.Append(identifier);
            }

            sb.Append(fileName);
            return System.Web.HttpContext.Current.Server.MapPath(sb.ToString());
        }

        /// <summary>
        /// derive the file storage folder path
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static string getFileStorageFolderUrl(CmsPage fileDetailsPage, int identifier, CmsLanguage language)
        {            
            string DMSFileStorageFolderUrl = CmsConfig.getConfigValue("DMSFileStorageFolderUrl", "");

            DMSFileStorageFolderUrl = VirtualPathUtility.ToAbsolute(DMSFileStorageFolderUrl);
            DMSFileStorageFolderUrl = VirtualPathUtility.AppendTrailingSlash(DMSFileStorageFolderUrl);            

            string subDir = "";
            if (CmsConfig.getConfigValue("DMSFileStorageLocationVersion", "V1") == "V2")
                subDir = fileDetailsPage.ID.ToString() + identifier.ToString() + language.shortCode.ToLower() + "/";

            return DMSFileStorageFolderUrl + subDir;
        }

        /// <summary>
        /// Derive a download url for the current file
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="identifier"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string getDownloadUrl(CmsPage aggregatorPage, int identifier, CmsLanguage language, string fileName)
        {
            string baseUrl = getFileStorageFolderUrl(aggregatorPage, identifier, language);
            string url = baseUrl + fileName;
            return url;
        }

        /// <summary>
        /// Derive a html anchor to download the current file
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="identifier"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string getDownloadAnchorHtml(CmsPage aggregatorPage, int identifier, CmsLanguage lang, string fileName)
        {
            return getDownloadAnchorHtml(aggregatorPage, identifier, lang, fileName, fileName, "_blank", "");
        }

        /// <summary>
        /// Derive a html anchor to download the current file (overload)
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="identifier"></param>
        /// <param name="fileName"></param>
        /// <param name="displayHtml"></param>
        /// <param name="target"></param>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public static string getDownloadAnchorHtml(CmsPage aggregatorPage, int identifier, CmsLanguage lang, string fileName, string displayHtml, string target, string cssClass)
        {
            StringBuilder html = new StringBuilder("<a href=\"");
            html.Append(getDownloadUrl(aggregatorPage, identifier, lang, fileName));
            html.Append("\"");
            if (target != "")
                html.Append(" target=\"" + target + "\" ");
            if (cssClass != "")
                html.Append(" class=\"" + cssClass + "\" ");
            html.Append(">");
            html.Append(displayHtml);
            html.Append("</a>");
            return html.ToString();
        }

        /// <summary>
        /// Get the file size on disk in bytes
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="identifier"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static long getFileSize(CmsPage aggregatorPage, int identifier, CmsLanguage lang, string fileName)
        {
            string nameOnDisk = getTargetNameOnDisk(aggregatorPage, identifier, lang, fileName);
            FileInfo fi = new FileInfo(nameOnDisk);
            return fileName.Length;
        }
    }
}
