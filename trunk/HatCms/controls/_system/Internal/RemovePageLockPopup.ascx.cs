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
using Hatfield.Web.Portal;

namespace HatCMS.Controls.Internal
{
    public partial class RemovePageLockPopup : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected override void Render(System.Web.UI.HtmlTextWriter writer)
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

                    if (!targetPage.currentUserCanWrite)
                    {
                        writer.WriteLine("Access Denied");
                        return;
                    }

                    targetPage.clearCurrentPageLock();
                    bool success = (targetPage.getCurrentPageLockData() == null);
                                        
                    if (!success)
                    {
                        html = html + "<span style=\"color: red\">Database error: could not remove page edit lock.</span>";
                    }
                    else
                    {
                        string script = "<script>"+Environment.NewLine;
                        script = script + "function go(url){"+Environment.NewLine;
                        script = script + "opener.location.href = url;"+Environment.NewLine;
                        script = script + "window.close();\n}";
                        script = script + "</script>"+Environment.NewLine;
                        html = html + "<span style=\"color: green; font-weight: bold;\">The Page Edit Lock has successfully been removed.</span>";
                        html = html + "<p><input type=\"button\" onclick=\"go('" + targetPage.Url + "');\" value=\"close this window\">";
                        writer.WriteLine(script + html);
                        return;
                    }
                }
            }
            html = html + "<p><input type=\"button\" onclick=\"window.close();\" value=\"close this window\">";
            html = html + "</center>";
            writer.WriteLine(html);
        }

    }
}