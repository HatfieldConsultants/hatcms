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
    public class ReindexSearchResults : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Utility, AdminMenuTab.Tools, "Re-index all pages for the search engine");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        public override string Render()
        {            

            bool success = true;
            try
            {
                HatCMS.Controls.SearchResults.ReIndexAllPages();
            }
            catch
            {
                success = false;
            }

            if (success)
                return "<p style=\"color: green\">Successfully re-indexed all pages for the search engine</p>";
            else
                return "<p style=\"color: red\">Error: could not re-index all pages for the search engine</p>";

        }

        public override System.Web.UI.WebControls.GridView RenderToGridViewForOutputToExcelFile()
        {
            return null; // not implemented.
        }
       

    }
}
