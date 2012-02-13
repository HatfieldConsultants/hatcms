using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Web;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public class GotoEditModeAction : BaseCmsControl
    {

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            return ret.ToArray();
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            int target = PageUtils.getFromForm("target", Int32.MinValue);

            CmsPage targetPage = CmsContext.getPageById(target);
            if (targetPage.Id < 0)
            {
                return("Invalid target pageId");                
            }

            if (!targetPage.currentUserCanWrite)
            {
                return ("Access Denied");                
            }

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

            CmsContext.setEditModeAndRedirect(CmsEditMode.Edit, targetPage, paramList);
            return "";
        }
        
    }
}
