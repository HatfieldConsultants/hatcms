using System;
using System.Text;
using System.Collections.Generic;
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
    public class FloatingEditMenu : BaseCmsControl
    {
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/FloatingEditMenu.js", CmsDependency.ExistsMode.MustNotExist )); // FloatingEditMenu.js is now embedded.
            ret.Add(CmsFileDependency.UnderAppPath("images/_system/hatCms_logo.png"));
            
            ret.Add(CmsFileDependency.UnderAppPath("controls/_system/FloatingEditMenu.ascx", CmsDependency.ExistsMode.MustNotExist)); // this class removes this

            ret.AddRange(new CmsPageEditMenu.DefaultStandardActionRenderers().getDependencies());
            return ret.ToArray();
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {            
            // -- don't render anything unless the user is logged in.
            if (!CmsContext.currentUserIsLoggedIn)
                return "";

            StringBuilder html = new StringBuilder();
            CmsPage page = CmsContext.currentPage;

            if (!page.currentUserCanWrite) // if the page is not writable, skip rendering the edit menu
                return "";

            // -- use the PerRequest cache to ensure that this control is only displayed once (not multiple times per language)
            string cacheName = "FloatingEditMenu";
            if (PerRequestCache.CacheContains(cacheName))
            {
                throw new TemplateExecutionException(page.TemplateName, "The FloatingEditMenu control should be placed after the ##EndPageBody## template file statement.");                
            }
            PerRequestCache.AddToCache(cacheName, true);

            page.HeadSection.AddEmbeddedJavascriptFile(JavascriptGroup.ControlOrPlaceholder, typeof(FloatingEditMenu).Assembly, "FloatingEditMenu.js");


            string divId = "editConsole_" + page.Id.ToString();
            string persistKey = "editConsole_" + page.TemplateName + CmsContext.currentEditMode.ToString();
            persistKey = persistKey.Replace("/", "_");
            persistKey = persistKey.Replace("\\", "_");
            persistKey = persistKey.Replace(" ", "_");

            string consoleDivId = "editConsoleOptions_" + page.Id.ToString();

            string leftPos = "580px";
            string topPos = "30px";
            // -- read the position of the floating toolbar from the cookie.
            // note that the cookie name must be the same as is defined in FloatingEditMenu.js.
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                System.Web.HttpRequest req = HttpContext.Current.Request;
                if (req.Cookies[persistKey + "_left"] != null && req.Cookies[persistKey + "_top"] != null)
                {
                    string l = req.Cookies[persistKey + "_left"].Value;
                    string t = req.Cookies[persistKey + "_top"].Value;
                    if (l.EndsWith("px") && t.EndsWith("px"))
                    {
                        try
                        {
                            // if the edit menu is off the screen, move it back into view.
                            // note: for max screen sizes, you can not use Request.Browser.ScreenPixelsWidth
                            int ll = Convert.ToInt32(l.Substring(0, l.Length - "px".Length));
                            int tt = Convert.ToInt32(t.Substring(0, t.Length - "px".Length));

                            if (ll < 10)
                                ll = 10;
                            if (tt < 10)
                                tt = 10;

                            leftPos = ll.ToString() + "px";
                            topPos = tt.ToString() + "px";
                        }
                        catch
                        { }
                    }
                }
            }

            html.Append("<div id=\"" + divId + "\" ondblclick=\"OpenCloseDiv('" + consoleDivId + "')\" style=\"PADDING-RIGHT: 0px; PADDING-LEFT: 0px; Z-INDEX: 10; PADDING-BOTTOM: 0px; WIDTH: 200px; PADDING-TOP: 0px; POSITION: absolute; LEFT: " + leftPos + "; TOP: " + topPos + "; BACKGROUND-COLOR: transparent; TEXT-ALIGN: left\">");
            html.Append("<table class=\"wbcedit\" onmouseover=\"drag('" + divId + "','" + persistKey + "')\" onfocus=\"this.blur()\" cellSpacing=\"0\" cellPadding=\"0\" border=\"0\">");
            html.Append("<tr>");
            html.Append("<td ondblclick=\"OpenCloseDiv('" + consoleDivId + "')\"  style=\"cursor: move;  font-family: arial; padding-left: 5px; padding-right: 5px; background-color: #4a87bd; opacity:0.95; filter:alpha(opacity=95); text-align: center;\">");
            html.Append("<span style=\"font-weight: bold;\">Edit Menu</span><span style=\"font-size: 8pt;\"><br /><nobr>(drag here to move menu)</span></nobr></td>");
            html.Append("<td style=\"background-color: #4a87bd; opacity:0.95; filter:alpha(opacity=95);\"><img src=\"" + CmsContext.ApplicationPath + "images/_system/hatCms_logo.png\" /></td>");
            html.Append("</tr>");
            html.Append("</table>");
            html.Append("<div class=\"wbcedit\" id=\"" + consoleDivId + "\">");
            html.Append("<table borderColor=\"#4a87bd\" cellSpacing=\"0\" cellPadding=\"5\" width=\"100%\" border=\"3\" style=\"border-top-right-radius: 4px; -moz-border-radius-topright: 4px; -webkit-border-top-right-radius: 4px; border-bottom-right-radius: 4px; -moz-border-radius-bottomright: 4px; -webkit-border-bottom-right-radius: 4px; border-bottom-left-radius: 4px; -moz-border-radius-bottomleft: 4px; -webkit-border-bottom-left-radius: 4px;\">");
            html.Append("	<tr>");
            html.Append("<td nowrap=\"nowrap\" style=\"background: yellow; opacity:0.95; filter:alpha(opacity=95); font-size: 10pt; font-family: arial;\" >");

            html.Append(getCurrentEditMenuActionsHtml(page));

            html.Append("</td>");
            html.Append("</tr>");
            html.Append("</table>");
            html.Append("</div>");
            html.Append("</div>");

            return html.ToString();
        }

        private string getCurrentEditMenuActionsHtml(CmsPage page)
        {
            List<string> lines = new List<string>();
            CmsLanguage lang = CmsContext.currentLanguage;

            foreach (CmsPageEditMenuAction action in page.EditMenu.CurrentEditMenuActions)
            {
                if (action.doRenderToString != null)
                    lines.Add(action.doRenderToString(action, page, lang));
            } // foreach

            return String.Join("<br />", lines.ToArray());
        }

    }
}
