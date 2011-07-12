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
    public class RecreateUserImageGalleriesFromDisk : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Utility, AdminMenuTab.Tools, "Refresh Image Galleries");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        public override string Render()
        {
            StringBuilder html = new StringBuilder();

            int numGalleriesUpdated = HatCMS.Placeholders.UserImageGallery.UpdateDatabaseCacheOfImageInfos();

            html.Append("<p style=\"color: green;\">" + numGalleriesUpdated.ToString() + " image galleries updated.<br>");


            return html.ToString();
        }        

       

    }
}
