using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text;

namespace HatCMS.Admin
{
    public class EmptyThumbnailCache : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Utility, AdminMenuTab.Tools, "Empty Image Cache");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        public override string Render()
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
                    deleted++;
                }
                catch
                { }

            } // foreach

            int numCached = CmsLocalImageOnDisk.DeleteAllCachedThumbnailUrls();

            html.Append(deleted.ToString() + " files and " + numCached + " URLs in the thumbnail cache have been deleted.<br>");



            return html.ToString();
        }        

       

    }
}
