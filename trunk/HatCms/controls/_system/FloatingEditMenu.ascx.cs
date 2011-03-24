namespace HatCMS.Controls
{
	using System;
    using System.Collections.Generic;
    using System.Text;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Collections.Specialized;

    using Hatfield.Web.Portal;
    using HatCMS.Placeholders;

	/// <summary>
	///		Renders the FloatingEditMenu to any user that is logged in. Options are different based on access levels, and the currently viewed page.
	/// </summary>
	public partial class FloatingEditMenu : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// nothing to initialize
		}

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/FloatingEditMenu.js", new DateTime(2010, 5, 13)));
            ret.Add(CmsFileDependency.UnderAppPath("images/_system/hatCms_logo.png"));
            ret.AddRange(new CmsPageEditMenu.DefaultStandardActionRenderers().getDependencies());
            return ret.ToArray();
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            // -- don't render anything unless the user is logged in.
            if (!CmsContext.currentUserIsLoggedIn)
                return;            

            StringBuilder html = new StringBuilder();
            CmsPage page = CmsContext.currentPage;

            if (!page.currentUserCanWrite) // if the page is not writable, skip rendering the edit menu
                return;

            // -- use the PerRequest cache to ensure that this control is only displayed once (not multiple times per language)
            string cacheName = "FloatingEditMenu";
            if (PerRequestCache.CacheContains(cacheName))
            {
                throw new TemplateExecutionException(page.TemplateName, "The FloatingEditMenu control should be placed after the ##EndPageBody## statement.");
                return;
            }
            PerRequestCache.AddToCache(cacheName, true);

            page.HeadSection.AddJavascriptFile("js/_system/FloatingEditMenu.js");


            string divId = "editConsole_" + page.ID.ToString();
            string persistKey = "editConsole_" + page.TemplateName + CmsContext.currentEditMode.ToString();
            persistKey = persistKey.Replace("/", "_");
            persistKey = persistKey.Replace("\\", "_");
            persistKey = persistKey.Replace(" ", "_");

            string consoleDivId = "editConsoleOptions_" + page.ID.ToString();

            string leftPos = "580px";
            string topPos = "30px";
            if (Request.Cookies[persistKey + "_left"] != null && Request.Cookies[persistKey + "_top"] != null)
            {
                string l = Request.Cookies[persistKey + "_left"].Value;
                string t = Request.Cookies[persistKey + "_top"].Value;
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
            writer.WriteLine(html.ToString());

        } // Render


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

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
	}
}
