using System;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS.Controls.EditingSystem
{
    public class EndEditForm : BaseCmsControl
    {
        public override CmsDependency[] getDependencies()
        {
            return new CmsDependency[0];
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            // -- render based on the current edit mode
            if (CmsContext.currentEditMode == CmsEditMode.Edit)
            {
                if (PageUtils.getFromForm("EndEditForm", "") == "submit")
                {

                    NameValueCollection paramList = new NameValueCollection();
                    string appendToTargetUrl = PageUtils.getFromForm("appendToTargetUrl", "");
                    if (appendToTargetUrl != "")
                    {
                        string[] p1 = appendToTargetUrl.Split(new char[] { '|', ',' });
                        foreach (string s in p1)
                        {
                            string[] p2 = s.Split(new char[] { '=' });
                            if (p2.Length == 2)
                                paramList.Add(p2[0], p2[1]);
                        }

                    }

                    CmsContext.setEditModeAndRedirect(CmsEditMode.View, CmsContext.currentPage, paramList);
                    return "";
                    // -- setEditModeAndRedirect ends response
                } // if submit

                StringBuilder html = new StringBuilder();
                html.Append(PageUtils.getHiddenInputHtml("EndEditForm", "submit"));
                html.Append(PageUtils.getHiddenInputHtml(CmsContext.EditModeFormName, "1")); // track the edit mode


                html.Append(CmsContext.currentPage.getFormCloseHtml(StartEditForm.FormId));

                return html.ToString();
            } // if in edit mode   
            else
            {
                // -in View mode
                return "";
            }
        }
    }
}
