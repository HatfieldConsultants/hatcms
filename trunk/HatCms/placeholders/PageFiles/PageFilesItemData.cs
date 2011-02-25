using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Placeholders
{
    public class PageFilesItemData
    {
        public int Id = -1;

        /// <summary>
        /// just the filename, without any path information
        /// </summary>
        public string Filename = "";
        public string Title = "";
        public string Author = "";
        public string CreatorUsername = "";
        public string Abstract = "";
        public string AbstractHtml
        {
            get
            {
                string html = Abstract;
                html = html.Replace(Environment.NewLine, "<p>");
                return html;
            }
        }

        public static string getFilenameOnDisk(CmsPage page, int identifier, CmsLanguage language, string userFilename)
        {
            string prependToFilename = "";
            if (CmsConfig.getConfigValue("DMSFileStorageLocationVersion", "V1") == "V1")
                prependToFilename = page.ID.ToString() + identifier.ToString();

            string baseUrl = GetFileStorageDirectoryUrl(page, identifier, language);

            string fn = baseUrl + prependToFilename + userFilename;
            string fnOnDisk = System.Web.HttpContext.Current.Server.MapPath(fn);
            return fnOnDisk;
        }

        public static string GetFileStorageDirectoryUrl(CmsPage page, int identifier, CmsLanguage language)
        {
            return FileLibraryDetailsData.getFileStorageFolderUrl(page, identifier, language);
        }

        public string getFilenameOnDisk(CmsPage page, int identifier, CmsLanguage language)
        {
            return getFilenameOnDisk(page, identifier, language, this.Filename);
        }

        public string getDownloadUrl(CmsPage page, int identifier, CmsLanguage lang)
        {

            string baseUrl = GetFileStorageDirectoryUrl(page, identifier, lang);

            string url = baseUrl + System.IO.Path.GetFileName(getFilenameOnDisk(page, identifier, lang));

            return url;

        }


        public long FileSize = 0;
        public DateTime lastModified = DateTime.MinValue;

    }
}
