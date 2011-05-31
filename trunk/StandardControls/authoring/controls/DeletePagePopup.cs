using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public class DeletePagePopup : BaseCmsControl
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
                html = html + "<span style=\"color: red\">Invalid Target parameter. No page to delete.</span>";
            }
            else
            {
                if (!CmsContext.pageExists(targetPageId))
                {
                    html = html + "<span style=\"color: red\">Target page does not exist. No page to delete.</span>";
                }
                else
                {

                    CmsPage page = CmsContext.getPageById(targetPageId);

                    if (!page.currentUserCanWrite)
                    {
                        return ("Access Denied");                        
                    }

                    if (page.isZoneBoundary == true) // if the cms page is a zone boundary, do not allow delete
                    {
                        html += "<span style=\"color: red\">Cannot delete the page because it is located at the zone boundary.</span>";
                        html += "<p><input type=\"button\" onclick=\"window.close();\" value=\"close this window\">";
                        html += "</center>";
                        return (html);                        
                    }

                    bool success = page.DeleteThisPage();

                    if (!success)
                    {
                        html = html + "<span style=\"color: red\">Database error: could not delete page.</span>";
                    }
                    else
                    {
                        string script = "<script>" + Environment.NewLine;
                        script = script + "function go(url){" + Environment.NewLine;
                        script = script + "opener.location.href = url;" + Environment.NewLine;
                        script = script + "window.close();\n}";
                        script = script + "</script>" + Environment.NewLine;
                        html = html + "<span style=\"color: green; font-weight: bold;\">The Page has successfully been deleted.</span>";
                        html = html + "<p><input type=\"button\" onclick=\"go('" + page.ParentPage.Url + "');\" value=\"close this window\">";
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
