using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Controls.EditingSystem
{
    public class StartEditForm : BaseCmsControl
    {
        public static string FormId = "HatCmsEditForm";
        
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/beforeUnload.js", CmsDependency.ExistsMode.MustNotExist)); // beforeUnload.js is now Embedded
            ret.Add(CmsFileDependency.UnderAppPath("controls/_system/StartEditForm.ascx", CmsDependency.ExistsMode.MustNotExist)); // this class replaces this
            return ret.ToArray();  
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            StringBuilder html = new StringBuilder();
            if (CmsContext.currentPage.currentUserCanWrite && CmsContext.currentEditMode == CmsEditMode.Edit)
            {
                html.Append(CmsContext.currentPage.getFormStartHtml(FormId, "submitting = true;"));
                CmsContext.currentPage.HeadSection.AddEmbeddedJavascriptFile(JavascriptGroup.FrontEnd, typeof(StartEditForm).Assembly, "beforeUnload.js");
            }

            return html.ToString();
        }
    }
}
