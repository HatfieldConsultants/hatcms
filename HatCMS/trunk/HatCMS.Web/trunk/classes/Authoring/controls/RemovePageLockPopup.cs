using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public class RemovePageLockPopup : BaseCmsControl
    {

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            return ret.ToArray();
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            string html = "<p><center>";
            int targetPageId = PageUtils.getFromForm("target", Int32.MinValue);
            if (targetPageId < 0)
            {
                html = html + "<span style=\"color: red\">Invalid Target parameter. No lock to remove.</span>";
            }
            else
            {
                if (!CmsContext.pageExists(targetPageId))
                {
                    html = html + "<span style=\"color: red\">Target page does not exist. No lock to remove.</span>";
                }
                else
                {
                    CmsPage targetPage = CmsContext.getPageById(targetPageId);

                    if (!targetPage.currentUserCanWrite || ! targetPage.currentUserCanRead)
                    {
                        return("Access Denied");                        
                    }

                    targetPage.clearCurrentPageLock();
                    bool success = (targetPage.getCurrentPageLockData() == null);

                    if (!success)
                    {
                        html = html + "<span style=\"color: red\">Database error: could not remove page edit lock.</span>";
                    }
                    else
                    {
                        string script = "<script>" + Environment.NewLine;
                        script = script + "function go(url){" + Environment.NewLine;
                        script = script + "opener.location.href = url;" + Environment.NewLine;
                        script = script + "window.close();\n}";
                        script = script + "</script>" + Environment.NewLine;
                        html = html + "<span style=\"color: green; font-weight: bold;\">The Page Edit Lock has successfully been removed.</span>";
                        html = html + "<p><input type=\"button\" onclick=\"go('" + targetPage.Url + "');\" value=\"close this window\">";
                        return (script + html);                        
                    }
                }
            }
            html = html + "<p><input type=\"button\" onclick=\"window.close();\" value=\"close this window\">";
            html = html + "</center>";
            return (html);
        }
        
    }
}
