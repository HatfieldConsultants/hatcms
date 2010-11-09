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
        private string SelectedParentPagePath
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "SelectedParentPagePath", "");
            }
        }

        private string SelectedImage
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "SelectedImage", "");
            }
        }

        private string UnSelectedImage
        {
            get
            {
                return CmsControlUtils.getControlParameterKeyValue(this, "UnSelectedImage", "");
            }
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
            if (SelectedParentPagePath != "" && SelectedImage != "" && parentOrSelfHasPath(CmsContext.currentPage, SelectedParentPagePath))
            {
                Response.Write("<img src=\"" + SelectedImage + "\">");
            }
            else if (UnSelectedImage != "")
            {
                Response.Write("<img src=\"" + UnSelectedImage + "\">");
            }
        } // render
    }
}