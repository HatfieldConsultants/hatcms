using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Controls
{
    public partial class ImageBySelectedParentPagePath : System.Web.UI.UserControl
    {
        private string getSelectedParentPagePath(CmsPage controlsPage)
        {

            return CmsControlUtils.getControlParameterKeyValue(controlsPage, this, "SelectedParentPagePath", "");
            
        }

        private string getSelectedImage(CmsPage controlsPage)
        {
            return CmsControlUtils.getControlParameterKeyValue(controlsPage, this, "SelectedImage", "");
        }

        private string getUnSelectedImage(CmsPage controlsPage)
        {            
                return CmsControlUtils.getControlParameterKeyValue(controlsPage, this, "UnSelectedImage", "");         
        }

        private bool parentOrSelfHasPath(CmsPage page, string path)
        {
            CmsPage p = page;
            while (p.ID > -1)
            {
                if (String.Compare(path, p.Path, true) == 0)
                    return true;
                p = p.ParentPage;
            } // while
            return false;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            CmsPage currentPage = CmsContext.currentPage;
            if (getSelectedParentPagePath(currentPage) != "" && getSelectedImage(currentPage) != "" && parentOrSelfHasPath(currentPage, getSelectedParentPagePath(currentPage)))
            {
                writer.Write("<img src=\"" + getSelectedImage(currentPage) + "\">");
            }
            else if (getUnSelectedImage(currentPage) != "")
            {
                writer.Write("<img src=\"" + getUnSelectedImage(currentPage) + "\">");
            }
        } // render
    }
}