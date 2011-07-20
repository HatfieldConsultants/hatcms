using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Web;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public class GotoViewModeAction : BaseCmsControl
    {

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            return ret.ToArray();
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            int targetPageId = PageUtils.getFromForm("target", Int32.MinValue);
            CmsPage targetPage = CmsContext.getPageById(targetPageId);

            string appendToTargetUrl = PageUtils.getFromForm("appendToTargetUrl", "");
            NameValueCollection paramList = new NameValueCollection();
            if (appendToTargetUrl.Trim() != "")
            {
                // -- split by | (pipe), and then by = (equals).
                string[] parts = appendToTargetUrl.Split(new char[] { '|' });
                foreach (string s in parts)
                {
                    string[] subParts = s.Split(new char[] { '=' });
                    if (subParts.Length == 2)
                        paramList.Add(subParts[0], subParts[1]);
                }
            }

            CmsContext.setEditModeAndRedirect(CmsEditMode.View, targetPage, paramList);
            return "";
        }
        
    }
}
